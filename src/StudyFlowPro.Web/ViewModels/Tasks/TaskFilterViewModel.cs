using StudyFlowPro.Web.Models.Enums;

namespace StudyFlowPro.Web.ViewModels.Tasks;

public sealed class TaskFilterViewModel
{
    public string? SearchTerm { get; set; }

    public int? SubjectId { get; set; }

    public StudyTaskPriority? Priority { get; set; }

    public StudyTaskStatus? Status { get; set; }

    public DateOnly? DueFrom { get; set; }

    public DateOnly? DueTo { get; set; }

    public string? OwnerId { get; set; }
}
