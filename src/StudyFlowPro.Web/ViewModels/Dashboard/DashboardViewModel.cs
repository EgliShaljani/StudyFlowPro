using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudyFlowPro.Web.ViewModels.Dashboard;

public sealed class DashboardViewModel
{
    public string WelcomeName { get; init; } = string.Empty;

    public bool IsAdmin { get; init; }

    public string? SelectedOwnerId { get; init; }

    public IReadOnlyList<SelectListItem> OwnerOptions { get; init; } = [];

    public DashboardMetricsViewModel Metrics { get; init; } = new();

    public IReadOnlyList<DashboardDeadlineItemViewModel> UpcomingTasks { get; init; } = [];

    public IReadOnlyList<DashboardDeadlineItemViewModel> OverdueTasks { get; init; } = [];

    public IReadOnlyList<DashboardSubjectProgressViewModel> SubjectProgress { get; init; } = [];
}
