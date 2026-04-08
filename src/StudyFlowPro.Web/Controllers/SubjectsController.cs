using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyFlowPro.Web.Data;
using StudyFlowPro.Web.Models;
using StudyFlowPro.Web.Models.Enums;
using StudyFlowPro.Web.ViewModels.Subjects;

namespace StudyFlowPro.Web.Controllers;

[Authorize]
public sealed class SubjectsController : AppController
{
    private readonly ApplicationDbContext _dbContext;

    public SubjectsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(string? ownerId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Subjects
            .AsNoTracking()
            .Include(subject => subject.Owner)
            .Include(subject => subject.StudyTasks)
            .AsQueryable();

        if (!CurrentUserIsAdmin)
        {
            query = query.Where(subject => subject.OwnerId == CurrentUserId);
        }
        else if (!string.IsNullOrWhiteSpace(ownerId))
        {
            query = query.Where(subject => subject.OwnerId == ownerId);
        }

        var items = await query
            .OrderBy(subject => subject.Name)
            .Select(subject => new SubjectListItemViewModel
            {
                Id = subject.Id,
                Name = subject.Name,
                Code = subject.Code,
                AccentColor = subject.AccentColor,
                Description = subject.Description,
                OwnerName = subject.Owner.FullName,
                OpenTasks = subject.StudyTasks.Count(task => task.Status != StudyTaskStatus.Done),
                CompletedTasks = subject.StudyTasks.Count(task => task.Status == StudyTaskStatus.Done),
                NextDueDate = subject.StudyTasks
                    .Where(task => task.Status != StudyTaskStatus.Done)
                    .OrderBy(task => task.DueDate)
                    .Select(task => (DateOnly?)task.DueDate)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var viewModel = new SubjectsIndexViewModel
        {
            IsAdmin = CurrentUserIsAdmin,
            SelectedOwnerId = ownerId,
            OwnerOptions = CurrentUserIsAdmin
                ? await BuildOwnerOptionsAsync(cancellationToken)
                : [],
            Items = items
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var subject = await FindSubjectAsync(id, includeTasks: true, cancellationToken);
        if (subject is null)
        {
            return NotFound();
        }

        var tasks = subject.StudyTasks
            .OrderBy(task => task.DueDate)
            .ThenByDescending(task => task.Priority)
            .Select(task => new SubjectTaskSummaryViewModel
            {
                Id = task.Id,
                Title = task.Title,
                DueDate = task.DueDate,
                Status = task.Status,
                Priority = task.Priority
            })
            .ToList();

        var viewModel = new SubjectDetailsViewModel
        {
            Id = subject.Id,
            Name = subject.Name,
            Code = subject.Code,
            AccentColor = subject.AccentColor,
            Description = subject.Description,
            OwnerName = subject.Owner.FullName,
            TotalTasks = tasks.Count,
            CompletedTasks = tasks.Count(task => task.Status == StudyTaskStatus.Done),
            Tasks = tasks
        };

        return View(viewModel);
    }

    public IActionResult Create()
    {
        return View("CreateEdit", new SubjectFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SubjectFormViewModel viewModel, CancellationToken cancellationToken)
    {
        await ValidateSubjectCodeAsync(viewModel.Code, CurrentUserId, null, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View("CreateEdit", viewModel);
        }

        var subject = new Subject
        {
            Name = viewModel.Name.Trim(),
            Code = viewModel.Code.Trim().ToUpperInvariant(),
            AccentColor = viewModel.AccentColor.Trim(),
            Description = string.IsNullOrWhiteSpace(viewModel.Description) ? null : viewModel.Description.Trim(),
            OwnerId = CurrentUserId
        };

        _dbContext.Subjects.Add(subject);
        await _dbContext.SaveChangesAsync(cancellationToken);

        SetFlashMessage("Subject created successfully.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var subject = await FindSubjectAsync(id, includeTasks: false, cancellationToken);
        if (subject is null)
        {
            return NotFound();
        }

        var viewModel = new SubjectFormViewModel
        {
            Id = subject.Id,
            Name = subject.Name,
            Code = subject.Code,
            AccentColor = subject.AccentColor,
            Description = subject.Description
        };

        return View("CreateEdit", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SubjectFormViewModel viewModel, CancellationToken cancellationToken)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        var subject = await FindSubjectAsync(id, includeTasks: false, cancellationToken);
        if (subject is null)
        {
            return NotFound();
        }

        await ValidateSubjectCodeAsync(viewModel.Code, subject.OwnerId, subject.Id, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View("CreateEdit", viewModel);
        }

        subject.Name = viewModel.Name.Trim();
        subject.Code = viewModel.Code.Trim().ToUpperInvariant();
        subject.AccentColor = viewModel.AccentColor.Trim();
        subject.Description = string.IsNullOrWhiteSpace(viewModel.Description) ? null : viewModel.Description.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        SetFlashMessage("Subject updated.");
        return RedirectToAction(nameof(Details), new { id = subject.Id });
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var subject = await FindSubjectAsync(id, includeTasks: true, cancellationToken);
        if (subject is null)
        {
            return NotFound();
        }

        var viewModel = new SubjectDetailsViewModel
        {
            Id = subject.Id,
            Name = subject.Name,
            Code = subject.Code,
            AccentColor = subject.AccentColor,
            Description = subject.Description,
            OwnerName = subject.Owner.FullName,
            TotalTasks = subject.StudyTasks.Count,
            CompletedTasks = subject.StudyTasks.Count(task => task.Status == StudyTaskStatus.Done)
        };

        return View(viewModel);
    }

    [HttpPost, ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var subject = await FindSubjectAsync(id, includeTasks: true, cancellationToken);
        if (subject is null)
        {
            return NotFound();
        }

        if (subject.StudyTasks.Count != 0)
        {
            SetFlashMessage("Delete or reassign the linked study tasks before removing this subject.", "warning");
            return RedirectToAction(nameof(Details), new { id = subject.Id });
        }

        _dbContext.Subjects.Remove(subject);
        await _dbContext.SaveChangesAsync(cancellationToken);

        SetFlashMessage("Subject deleted.");
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateSubjectCodeAsync(string code, string ownerId, int? subjectId, CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        var isDuplicate = await _dbContext.Subjects
            .AnyAsync(
                subject => subject.OwnerId == ownerId
                    && subject.Code == normalizedCode
                    && (!subjectId.HasValue || subject.Id != subjectId.Value),
                cancellationToken);

        if (isDuplicate)
        {
            ModelState.AddModelError(nameof(SubjectFormViewModel.Code), "That subject code is already in use.");
        }
    }

    private async Task<Subject?> FindSubjectAsync(int id, bool includeTasks, CancellationToken cancellationToken)
    {
        var query = _dbContext.Subjects
            .Include(subject => subject.Owner)
            .AsQueryable();

        if (includeTasks)
        {
            query = query.Include(subject => subject.StudyTasks);
        }

        if (!CurrentUserIsAdmin)
        {
            query = query.Where(subject => subject.OwnerId == CurrentUserId);
        }

        return await query.SingleOrDefaultAsync(subject => subject.Id == id, cancellationToken);
    }

    private async Task<IReadOnlyList<SelectListItem>> BuildOwnerOptionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.FullName)
            .Select(user => new SelectListItem
            {
                Value = user.Id,
                Text = user.FullName
            })
            .ToListAsync(cancellationToken);
    }
}
