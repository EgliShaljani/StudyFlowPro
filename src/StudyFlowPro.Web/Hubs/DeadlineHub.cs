using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StudyFlowPro.Web.Extensions;
using StudyFlowPro.Web.Services;

namespace StudyFlowPro.Web.Hubs;

[Authorize]
public sealed class DeadlineHub : Hub
{
    private readonly IDeadlineNotificationService _deadlineNotificationService;

    public DeadlineHub(IDeadlineNotificationService deadlineNotificationService)
    {
        _deadlineNotificationService = deadlineNotificationService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.GetUserIdOrNull();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(userId));
            await _deadlineNotificationService.RefreshForUserAsync(userId, Context.ConnectionAborted);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.GetUserIdOrNull();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(userId));
        }

        await base.OnDisconnectedAsync(exception);
    }

    public Task RequestRefresh()
    {
        var userId = Context.User?.GetUserIdOrNull();
        return string.IsNullOrWhiteSpace(userId)
            ? Task.CompletedTask
            : _deadlineNotificationService.RefreshForUserAsync(userId, Context.ConnectionAborted);
    }

    public static string GetGroupName(string userId) => $"user:{userId}";
}
