namespace StudyFlowPro.Web.ViewModels.Dashboard;

public sealed class DashboardSubjectProgressViewModel
{
    public int SubjectId { get; init; }

    public string SubjectName { get; init; } = string.Empty;

    public string SubjectCode { get; init; } = string.Empty;

    public string SubjectAccentColor { get; init; } = "#2563EB";

    public int TotalTasks { get; init; }

    public int CompletedTasks { get; init; }

    public int OpenTasks { get; init; }

    public double CompletionRate { get; init; }

    public DateOnly? NextDueDate { get; init; }
}
