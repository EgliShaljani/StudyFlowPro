using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudyFlowPro.Web.ViewModels.Tasks;

public sealed class TasksIndexViewModel
{
    public TaskFilterViewModel Filter { get; init; } = new();

    public bool IsAdmin { get; init; }

    public IReadOnlyList<SelectListItem> OwnerOptions { get; init; } = [];

    public IReadOnlyList<SelectListItem> SubjectOptions { get; init; } = [];

    public IReadOnlyList<TaskListItemViewModel> Items { get; init; } = [];

    public int VisibleTaskCount { get; init; }

    public int OverdueTaskCount { get; init; }
}
