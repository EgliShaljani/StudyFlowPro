namespace StudyFlowPro.Web.Services;

public sealed record DashboardMetrics(
    int TotalTasks,
    int CompletedTasks,
    int PendingTasks,
    int OverdueTasks)
{
    public double CompletionRate => TotalTasks == 0
        ? 0
        : Math.Round((double)CompletedTasks / TotalTasks * 100, 1);
}
