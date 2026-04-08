namespace StudyFlowPro.Web.ViewModels.Dashboard;

public sealed class DashboardMetricsViewModel
{
    public int TotalTasks { get; init; }

    public int CompletedTasks { get; init; }

    public int PendingTasks { get; init; }

    public int OverdueTasks { get; init; }

    public double CompletionRate { get; init; }
}
