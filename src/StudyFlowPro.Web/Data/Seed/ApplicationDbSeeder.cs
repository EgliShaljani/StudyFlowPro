using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyFlowPro.Web.Models;
using StudyFlowPro.Web.Models.Enums;

namespace StudyFlowPro.Web.Data.Seed;

public static class ApplicationDbSeeder
{
    private const string SeedPassword = "StudyFlow123!";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

        await EnsureRoleAsync(roleManager, ApplicationRoles.Admin);
        await EnsureRoleAsync(roleManager, ApplicationRoles.Student);

        var adminUser = await EnsureUserAsync(
            userManager,
            email: "admin@studyflowpro.dev",
            fullName: "Ava Benson",
            role: ApplicationRoles.Admin);

        var primaryStudent = await EnsureUserAsync(
            userManager,
            email: "student@studyflowpro.dev",
            fullName: "Mia Shaljani",
            role: ApplicationRoles.Student);

        var secondStudent = await EnsureUserAsync(
            userManager,
            email: "alex@studyflowpro.dev",
            fullName: "Alex Romero",
            role: ApplicationRoles.Student);

        if (await dbContext.Subjects.AnyAsync())
        {
            return;
        }

        var subjects = new List<Subject>
        {
            new()
            {
                Name = "Software Engineering",
                Code = "SE401",
                AccentColor = "#2563EB",
                Description = "Architecture reviews, sprint planning, and weekly lab checkpoints.",
                OwnerId = primaryStudent.Id
            },
            new()
            {
                Name = "Data Science",
                Code = "DS220",
                AccentColor = "#10B981",
                Description = "Model evaluation, notebook cleanup, and dataset validation.",
                OwnerId = primaryStudent.Id
            },
            new()
            {
                Name = "Business Communication",
                Code = "BC150",
                AccentColor = "#F59E0B",
                Description = "Presentation prep, peer feedback, and report polish.",
                OwnerId = secondStudent.Id
            },
            new()
            {
                Name = "Team Leadership",
                Code = "TL510",
                AccentColor = "#EC4899",
                Description = "Mentoring notes, workshop prep, and roadmap planning.",
                OwnerId = adminUser.Id
            }
        };

        dbContext.Subjects.AddRange(subjects);
        await dbContext.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var taskSeed = new List<StudyTask>
        {
            new()
            {
                Title = "Finalize API architecture diagram",
                Description = "Export the updated service boundaries and add data flow notes for the review meeting.",
                SubjectId = subjects.Single(subject => subject.Code == "SE401").Id,
                DueDate = today.AddDays(1),
                Priority = StudyTaskPriority.High,
                Status = StudyTaskStatus.InProgress,
                EstimatedHours = 3.5m,
                OwnerId = primaryStudent.Id
            },
            new()
            {
                Title = "Submit sprint retrospective summary",
                Description = "Condense wins, blockers, and follow-up actions into a concise one-page document.",
                SubjectId = subjects.Single(subject => subject.Code == "SE401").Id,
                DueDate = today.AddDays(-1),
                Priority = StudyTaskPriority.Urgent,
                Status = StudyTaskStatus.ToDo,
                EstimatedHours = 1.5m,
                OwnerId = primaryStudent.Id
            },
            new()
            {
                Title = "Tune random forest baseline",
                Description = "Compare feature subsets and document metric deltas before the next standup.",
                SubjectId = subjects.Single(subject => subject.Code == "DS220").Id,
                DueDate = today.AddDays(4),
                Priority = StudyTaskPriority.Medium,
                Status = StudyTaskStatus.ToDo,
                EstimatedHours = 4m,
                OwnerId = primaryStudent.Id
            },
            new()
            {
                Title = "Publish cleaned notebook findings",
                Description = "Wrap up the narrative cells and export the final notebook PDF for submission.",
                SubjectId = subjects.Single(subject => subject.Code == "DS220").Id,
                DueDate = today.AddDays(-3),
                Priority = StudyTaskPriority.Low,
                Status = StudyTaskStatus.Done,
                EstimatedHours = 2m,
                OwnerId = primaryStudent.Id,
                CompletedOnUtc = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Title = "Refine final presentation deck",
                Description = "Tighten the story arc, reduce text-heavy slides, and rehearse transitions.",
                SubjectId = subjects.Single(subject => subject.Code == "BC150").Id,
                DueDate = today,
                Priority = StudyTaskPriority.High,
                Status = StudyTaskStatus.InProgress,
                EstimatedHours = 2.5m,
                OwnerId = secondStudent.Id
            },
            new()
            {
                Title = "Prepare workshop facilitation plan",
                Description = "Outline the agenda, breakout prompts, and follow-up actions for the cohort session.",
                SubjectId = subjects.Single(subject => subject.Code == "TL510").Id,
                DueDate = today.AddDays(2),
                Priority = StudyTaskPriority.Medium,
                Status = StudyTaskStatus.ToDo,
                EstimatedHours = 2m,
                OwnerId = adminUser.Id
            }
        };

        dbContext.StudyTasks.AddRange(taskSeed);
        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string role)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is null)
        {
            existingUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                CreatedOnUtc = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(existingUser, SeedPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Unable to seed user {email}: {errors}");
            }
        }

        existingUser.FullName = fullName;
        existingUser.EmailConfirmed = true;
        await userManager.UpdateAsync(existingUser);

        if (!await userManager.IsInRoleAsync(existingUser, role))
        {
            await userManager.AddToRoleAsync(existingUser, role);
        }

        return existingUser;
    }
}
