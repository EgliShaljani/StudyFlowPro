using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyFlowPro.Web.Data;
using StudyFlowPro.Web.Models;
using StudyFlowPro.Web.Services;
using StudyFlowPro.Web.ViewModels.Dashboard;

namespace StudyFlowPro.Web.Controllers;

[Authorize]
public sealed class DashboardController : AppController
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDashboardMetricsService _dashboardMetricsService;

    public DashboardController(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IDashboardMetricsService dashboardMetricsService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _dashboardMetricsService = dashboardMetricsService;
    }

    public async Task<IActionResult> Index(string? ownerId, CancellationToken cancellationToken)
    {
        var currentUser = await _userManager.GetUserAsync(User)
            ?? throw new InvalidOperationException("The current user could not be loaded.");

        var effectiveOwnerId = CurrentUserIsAdmin ? ownerId : currentUser.Id;
        var ownerOptions = CurrentUserIsAdmin
            ? await BuildOwnerOptionsAsync(cancellationToken)
            : [];

        var taskQuery = _dbContext.StudyTasks
            .AsNoTracking()
            .Include(task => task.Subject)
            .Include(task => task.Owner)
            .AsQueryable();

        if (!CurrentUserIsAdmin)
        {
            taskQuery = taskQuery.Where(task => task.OwnerId == currentUser.Id);
        }
        else if (!string.IsNullOrWhiteSpace(effectiveOwnerId))
        {
            taskQuery = taskQuery.Where(task => task.OwnerId == effectiveOwnerId);
        }

        var tasks = await taskQuery
            .OrderBy(task => task.DueDate)
            .ThenByDescending(task => task.Priority)
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var metrics = _dashboardMetricsService.Calculate(tasks, today);

        var upcomingTasks = tasks
            .Where(task => !task.IsCompleted && task.DueDate >= today)
            .OrderBy(task => task.DueDate)
            .ThenByDescending(task => task.Priority)
            .Take(5)
            .Select(task => new DashboardDeadlineItemViewModel
            {
                Id = task.Id,
                Title = task.Title,
                SubjectName = task.Subject.Name,
                SubjectAccentColor = task.Subject.AccentColor,
                DueDate = task.DueDate,
                Priority = task.Priority,
                Status = task.Status,
                OwnerName = task.Owner.FullName
            })
            .ToList();

        var overdueTasks = tasks
            .Where(task => task.IsOverdue(today))
            .OrderBy(task => task.DueDate)
            .ThenByDescending(task => task.Priority)
            .Take(5)
            .Select(task => new DashboardDeadlineItemViewModel
            {
                Id = task.Id,
                Title = task.Title,
                SubjectName = task.Subject.Name,
                SubjectAccentColor = task.Subject.AccentColor,
                DueDate = task.DueDate,
                Priority = task.Priority,
                Status = task.Status,
                OwnerName = task.Owner.FullName
            })
            .ToList();

        var subjectProgress = tasks
            .GroupBy(task => new { task.SubjectId, task.Subject.Name, task.Subject.Code, task.Subject.AccentColor })
            .Select(group =>
            {
                var subjectTasks = group.ToList();
                var completed = subjectTasks.Count(task => task.IsCompleted);
                var nextDueDate = subjectTasks
                    .Where(task => !task.IsCompleted)
                    .OrderBy(task => task.DueDate)
                    .Select(task => (DateOnly?)task.DueDate)
                    .FirstOrDefault();

                return new DashboardSubjectProgressViewModel
                {
                    SubjectId = group.Key.SubjectId,
                    SubjectName = group.Key.Name,
                    SubjectCode = group.Key.Code,
                    SubjectAccentColor = group.Key.AccentColor,
                    TotalTasks = subjectTasks.Count,
                    CompletedTasks = completed,
                    OpenTasks = subjectTasks.Count - completed,
                    CompletionRate = subjectTasks.Count == 0
                        ? 0
                        : Math.Round((double)completed / subjectTasks.Count * 100, 1),
                    NextDueDate = nextDueDate
                };
            })
            .OrderByDescending(subject => subject.OpenTasks)
            .ThenBy(subject => subject.NextDueDate)
            .ToList();

        var viewModel = new DashboardViewModel
        {
            WelcomeName = currentUser.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? currentUser.FullName,
            SelectedOwnerId = CurrentUserIsAdmin ? effectiveOwnerId : currentUser.Id,
            IsAdmin = CurrentUserIsAdmin,
            OwnerOptions = ownerOptions,
            Metrics = new DashboardMetricsViewModel
            {
                TotalTasks = metrics.TotalTasks,
                CompletedTasks = metrics.CompletedTasks,
                PendingTasks = metrics.PendingTasks,
                OverdueTasks = metrics.OverdueTasks,
                CompletionRate = metrics.CompletionRate
            },
            UpcomingTasks = upcomingTasks,
            OverdueTasks = overdueTasks,
            SubjectProgress = subjectProgress
        };

        return View(viewModel);
    }

    private async Task<IReadOnlyList<SelectListItem>> BuildOwnerOptionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.FullName)
            .Select(user => new SelectListItem
            {
                Value = user.Id,
                Text = user.FullName
            })
            .ToListAsync(cancellationToken);
    }
}
