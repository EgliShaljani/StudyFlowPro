namespace StudyFlowPro.Web.Services;

public interface IDeadlineNotificationService
{
    Task RefreshForAllAsync(CancellationToken cancellationToken = default);

    Task RefreshForUserAsync(string userId, CancellationToken cancellationToken = default);
}
