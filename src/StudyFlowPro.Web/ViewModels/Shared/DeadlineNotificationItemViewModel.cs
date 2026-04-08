namespace StudyFlowPro.Web.ViewModels.Shared;

public sealed class DeadlineNotificationItemViewModel
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string SubjectName { get; init; } = string.Empty;

    public string SubjectColor { get; init; } = "#2563EB";

    public DateOnly DueDate { get; init; }

    public bool IsOverdue { get; init; }

    public string Priority { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}
