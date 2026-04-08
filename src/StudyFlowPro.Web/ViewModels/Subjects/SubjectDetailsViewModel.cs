namespace StudyFlowPro.Web.ViewModels.Subjects;

public sealed class SubjectDetailsViewModel
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Code { get; init; } = string.Empty;

    public string AccentColor { get; init; } = "#2563EB";

    public string? Description { get; init; }

    public string OwnerName { get; init; } = string.Empty;

    public int TotalTasks { get; init; }

    public int CompletedTasks { get; init; }

    public IReadOnlyList<SubjectTaskSummaryViewModel> Tasks { get; init; } = [];
}
