namespace StudyFlowPro.Web.ViewModels.Shared;

public sealed class DeadlineNotificationDigestViewModel
{
    public DateTime GeneratedAt { get; init; }

    public int TotalCount { get; init; }

    public IReadOnlyList<DeadlineNotificationItemViewModel> Items { get; init; } = [];
}
