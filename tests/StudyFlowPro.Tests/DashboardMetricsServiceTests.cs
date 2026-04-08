using StudyFlowPro.Web.Models;
using StudyFlowPro.Web.Models.Enums;
using StudyFlowPro.Web.Services;

namespace StudyFlowPro.Tests;

public class DashboardMetricsServiceTests
{
    [Fact]
    public void Calculate_ReturnsAccurateSummaryCounts()
    {
        var service = new DashboardMetricsService();
        var tasks = new[]
        {
            new StudyTask { Title = "Task 1", DueDate = new DateOnly(2026, 4, 3), Status = StudyTaskStatus.Done },
            new StudyTask { Title = "Task 2", DueDate = new DateOnly(2026, 4, 5), Status = StudyTaskStatus.InProgress },
            new StudyTask { Title = "Task 3", DueDate = new DateOnly(2026, 4, 10), Status = StudyTaskStatus.ToDo }
        };

        var metrics = service.Calculate(tasks, new DateOnly(2026, 4, 8));

        Assert.Equal(3, metrics.TotalTasks);
        Assert.Equal(1, metrics.CompletedTasks);
        Assert.Equal(2, metrics.PendingTasks);
        Assert.Equal(1, metrics.OverdueTasks);
        Assert.Equal(33.3, metrics.CompletionRate);
    }
}
