using StudyFlowPro.Web.ViewModels.Tasks;
using System.ComponentModel.DataAnnotations;

namespace StudyFlowPro.Tests;

public class StudyTaskFormViewModelTests
{
    [Fact]
    public void Validation_Fails_WhenDueDateIsMissing()
    {
        var model = new StudyTaskFormViewModel
        {
            Title = "Prepare exam recap",
            SubjectId = 2,
            DueDate = null
        };

        var validationResults = Validate(model);

        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(StudyTaskFormViewModel.DueDate)));
    }

    [Fact]
    public void Validation_Fails_WhenSubjectIdIsInvalid()
    {
        var model = new StudyTaskFormViewModel
        {
            Title = "Prepare exam recap",
            SubjectId = 0,
            DueDate = new DateOnly(2026, 4, 15)
        };

        var validationResults = Validate(model);

        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(StudyTaskFormViewModel.SubjectId)));
    }

    private static List<ValidationResult> Validate(StudyTaskFormViewModel model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }
}
