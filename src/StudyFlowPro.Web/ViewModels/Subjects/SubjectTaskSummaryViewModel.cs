using StudyFlowPro.Web.Models.Enums;

namespace StudyFlowPro.Web.ViewModels.Subjects;

public sealed class SubjectTaskSummaryViewModel
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public DateOnly DueDate { get; init; }

    public StudyTaskStatus Status { get; init; }

    public StudyTaskPriority Priority { get; init; }
}
