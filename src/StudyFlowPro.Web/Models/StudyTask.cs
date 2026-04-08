using StudyFlowPro.Web.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace StudyFlowPro.Web.Models;

public class StudyTask
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Add a task title.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Task titles should be between 3 and 120 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(800, ErrorMessage = "Keep the task description under 800 characters.")]
    public string? Description { get; set; }

    [Required]
    public int SubjectId { get; set; }

    public Subject Subject { get; set; } = null!;

    [Required(ErrorMessage = "Choose a due date.")]
    public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    [Required]
    public StudyTaskPriority Priority { get; set; } = StudyTaskPriority.Medium;

    [Required]
    public StudyTaskStatus Status { get; set; } = StudyTaskStatus.ToDo;

    [Range(0.5, 40, ErrorMessage = "Estimated effort should be between 0.5 and 40 hours.")]
    public decimal EstimatedHours { get; set; } = 2;

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    public ApplicationUser Owner { get; set; } = null!;

    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedOnUtc { get; set; }

    public bool IsCompleted => Status == StudyTaskStatus.Done;

    public bool IsOverdue(DateOnly today) => !IsCompleted && DueDate < today;

    public bool IsDueSoon(DateOnly today, int daysAhead = 1) =>
        !IsCompleted && DueDate >= today && DueDate <= today.AddDays(daysAhead);
}
