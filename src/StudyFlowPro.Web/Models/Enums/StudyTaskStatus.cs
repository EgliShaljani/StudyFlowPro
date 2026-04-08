using System.ComponentModel.DataAnnotations;

namespace StudyFlowPro.Web.Models.Enums;

public enum StudyTaskStatus
{
    [Display(Name = "To Do")]
    ToDo = 0,

    [Display(Name = "In Progress")]
    InProgress = 1,

    [Display(Name = "Done")]
    Done = 2
}
