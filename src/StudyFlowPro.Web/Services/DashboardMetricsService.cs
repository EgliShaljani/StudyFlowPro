using StudyFlowPro.Web.Models;

namespace StudyFlowPro.Web.Services;

public sealed class DashboardMetricsService : IDashboardMetricsService
{
    public DashboardMetrics Calculate(IEnumerable<StudyTask> tasks, DateOnly today)
    {
        var taskList = tasks.ToList();
        var completedTasks = taskList.Count(task => task.IsCompleted);
        var overdueTasks = taskList.Count(task => task.IsOverdue(today));

        return new DashboardMetrics(
            taskList.Count,
            completedTasks,
            taskList.Count - completedTasks,
            overdueTasks);
    }
}
