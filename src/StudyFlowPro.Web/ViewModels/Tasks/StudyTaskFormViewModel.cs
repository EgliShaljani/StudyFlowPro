using Microsoft.AspNetCore.Mvc.Rendering;
using StudyFlowPro.Web.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace StudyFlowPro.Web.ViewModels.Tasks;

public sealed class StudyTaskFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Add a task title.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Task titles should be between 3 and 120 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(800, ErrorMessage = "Keep the task description under 800 characters.")]
    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Choose a subject.")]
    public int SubjectId { get; set; }

    [Required(ErrorMessage = "Choose a due date.")]
    public DateOnly? DueDate { get; set; }

    [Required]
    public StudyTaskPriority Priority { get; set; } = StudyTaskPriority.Medium;

    [Required]
    public StudyTaskStatus Status { get; set; } = StudyTaskStatus.ToDo;

    [Range(0.5, 40, ErrorMessage = "Estimated effort should be between 0.5 and 40 hours.")]
    public decimal EstimatedHours { get; set; } = 2;

    public IReadOnlyList<SelectListItem> SubjectOptions { get; set; } = [];
}
