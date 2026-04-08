using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudyFlowPro.Web.Data;
using StudyFlowPro.Web.Extensions;
using StudyFlowPro.Web.Hubs;
using StudyFlowPro.Web.Models.Enums;
using StudyFlowPro.Web.ViewModels.Shared;
using System.Collections.Concurrent;
using System.Text.Json;

namespace StudyFlowPro.Web.Services;

public sealed class DeadlineNotificationService : IDeadlineNotificationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<DeadlineHub> _hubContext;
    private readonly IOptionsMonitor<DeadlineNotificationSettings> _settings;
    private readonly ILogger<DeadlineNotificationService> _logger;
    private readonly ConcurrentDictionary<string, string> _fingerprints = new();

    public DeadlineNotificationService(
        ApplicationDbContext dbContext,
        IHubContext<DeadlineHub> hubContext,
        IOptionsMonitor<DeadlineNotificationSettings> settings,
        ILogger<DeadlineNotificationService> logger)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
        _settings = settings;
        _logger = logger;
    }

    public async Task RefreshForAllAsync(CancellationToken cancellationToken = default)
    {
        var userIds = await _dbContext.Users
            .AsNoTracking()
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);

        foreach (var userId in userIds)
        {
            await RefreshForUserAsync(userId, cancellationToken);
        }
    }

    public async Task RefreshForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var payload = await BuildDigestAsync(userId, cancellationToken);
        var fingerprint = JsonSerializer.Serialize(payload.Items.Select(item => new
        {
            item.Id,
            item.IsOverdue,
            item.DueDate,
            item.Status
        }));

        if (_fingerprints.TryGetValue(userId, out var existing) && existing == fingerprint)
        {
            return;
        }

        _fingerprints[userId] = fingerprint;

        try
        {
            await _hubContext.Clients
                .Group(DeadlineHub.GetGroupName(userId))
                .SendAsync("ReceiveDeadlineDigest", payload, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogDebug(exception, "Unable to broadcast deadline digest for user {UserId}", userId);
        }
    }

    private async Task<DeadlineNotificationDigestViewModel> BuildDigestAsync(string userId, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueSoonThreshold = today.AddDays(Math.Max(1, _settings.CurrentValue.UpcomingWindowDays));

        var tasks = await _dbContext.StudyTasks
            .AsNoTracking()
            .Include(task => task.Subject)
            .Where(task =>
                task.OwnerId == userId &&
                task.Status != StudyTaskStatus.Done &&
                (task.DueDate < today || task.DueDate <= dueSoonThreshold))
            .OrderBy(task => task.DueDate)
            .ThenByDescending(task => task.Priority)
            .Take(8)
            .ToListAsync(cancellationToken);

        var items = tasks
            .Select(task =>
            {
                var isOverdue = task.IsOverdue(today);
                var relativeMessage = isOverdue
                    ? "Needs attention now."
                    : task.DueDate == today
                        ? "Due today."
                        : task.DueDate == today.AddDays(1)
                            ? "Due tomorrow."
                            : $"Due in {task.DueDate.DayNumber - today.DayNumber} days.";

                return new DeadlineNotificationItemViewModel
                {
                    Id = task.Id,
                    Title = task.Title,
                    SubjectName = task.Subject.Name,
                    SubjectColor = task.Subject.AccentColor,
                    DueDate = task.DueDate,
                    IsOverdue = isOverdue,
                    Priority = task.Priority.GetDisplayName(),
                    Status = task.Status.GetDisplayName(),
                    Message = relativeMessage
                };
            })
            .ToList();

        return new DeadlineNotificationDigestViewModel
        {
            GeneratedAt = DateTime.UtcNow,
            TotalCount = items.Count,
            Items = items
        };
    }
}
