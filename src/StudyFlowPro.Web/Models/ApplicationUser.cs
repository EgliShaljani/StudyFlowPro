using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace StudyFlowPro.Web.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Subject> Subjects { get; set; } = [];

    public ICollection<StudyTask> StudyTasks { get; set; } = [];
}
