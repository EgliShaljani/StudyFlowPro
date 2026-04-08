using StudyFlowPro.Web.Models;
using StudyFlowPro.Web.Models.Enums;

namespace StudyFlowPro.Tests;

public class StudyTaskTests
{
    [Fact]
    public void IsOverdue_ReturnsTrue_WhenTaskIsPastDueAndIncomplete()
    {
        var task = new StudyTask
        {
            Title = "Review lecture notes",
            DueDate = new DateOnly(2026, 4, 5),
            Status = StudyTaskStatus.InProgress
        };

        var result = task.IsOverdue(new DateOnly(2026, 4, 8));

        Assert.True(result);
    }

    [Fact]
    public void IsOverdue_ReturnsFalse_WhenTaskIsDone()
    {
        var task = new StudyTask
        {
            Title = "Submit coursework",
            DueDate = new DateOnly(2026, 4, 2),
            Status = StudyTaskStatus.Done
        };

        var result = task.IsOverdue(new DateOnly(2026, 4, 8));

        Assert.False(result);
    }
}
