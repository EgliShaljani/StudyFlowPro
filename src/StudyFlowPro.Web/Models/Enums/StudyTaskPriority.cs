using System.ComponentModel.DataAnnotations;

namespace StudyFlowPro.Web.Models.Enums;

public enum StudyTaskPriority
{
    [Display(Name = "Low")]
    Low = 0,

    [Display(Name = "Medium")]
    Medium = 1,

    [Display(Name = "High")]
    High = 2,

    [Display(Name = "Urgent")]
    Urgent = 3
}
