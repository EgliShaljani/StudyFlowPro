using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudyFlowPro.Web.ViewModels.Subjects;

public sealed class SubjectsIndexViewModel
{
    public bool IsAdmin { get; init; }

    public string? SelectedOwnerId { get; init; }

    public IReadOnlyList<SelectListItem> OwnerOptions { get; init; } = [];

    public IReadOnlyList<SubjectListItemViewModel> Items { get; init; } = [];
}
