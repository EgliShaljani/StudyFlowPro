using StudyFlowPro.Web.Models.Enums;

namespace StudyFlowPro.Web.ViewModels.Tasks;

public sealed class TaskListItemViewModel
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string SubjectName { get; init; } = string.Empty;

    public string SubjectAccentColor { get; init; } = "#2563EB";

    public string OwnerName { get; init; } = string.Empty;

    public DateOnly DueDate { get; init; }

    public StudyTaskPriority Priority { get; init; }

    public StudyTaskStatus Status { get; init; }

    public decimal EstimatedHours { get; init; }

    public bool IsOverdue { get; init; }
}
