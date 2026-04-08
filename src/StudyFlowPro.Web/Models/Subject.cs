using System.ComponentModel.DataAnnotations;

namespace StudyFlowPro.Web.Models;

public class Subject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Give this subject a name.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Subject names should be between 2 and 80 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Add a short code so the subject is easy to spot.")]
    [StringLength(20, ErrorMessage = "Keep the subject code under 20 characters.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Choose an accent color.")]
    [RegularExpression("^#(?:[0-9a-fA-F]{3}){1,2}$", ErrorMessage = "Use a valid HEX color such as #2563EB.")]
    public string AccentColor { get; set; } = "#2563EB";

    [StringLength(220, ErrorMessage = "Keep the overview under 220 characters.")]
    public string? Description { get; set; }

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    public ApplicationUser Owner { get; set; } = null!;

    public ICollection<StudyTask> StudyTasks { get; set; } = [];
}
