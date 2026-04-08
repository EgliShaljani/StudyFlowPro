using StudyFlowPro.Web.Models;

namespace StudyFlowPro.Web.Services;

public interface IDashboardMetricsService
{
    DashboardMetrics Calculate(IEnumerable<StudyTask> tasks, DateOnly today);
}
