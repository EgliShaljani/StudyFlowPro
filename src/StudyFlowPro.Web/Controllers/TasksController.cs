using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyFlowPro.Web.Data;
using StudyFlowPro.Web.Models;
using StudyFlowPro.Web.Models.Enums;
using StudyFlowPro.Web.Services;
using StudyFlowPro.Web.ViewModels.Tasks;

namespace StudyFlowPro.Web.Controllers;

[Authorize]
public sealed class TasksController : AppController
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDeadlineNotificationService _deadlineNotificationService;

    public TasksController(
        ApplicationDbContext dbContext,
        IDeadlineNotificationService deadlineNotificationService)
    {
        _dbContext = dbContext;
        _deadlineNotificationService = deadlineNotificationService;
    }

    public async Task<IActionResult> Index([FromQuery] TaskFilterViewModel filter, CancellationToken cancellationToken)
    {
        var query = _dbContext.StudyTasks
            .AsNoTracking()
            .Include(task => task.Subject)
            .Include(task => task.Owner)
            .AsQueryable();

        if (!CurrentUserIsAdmin)
        {
            query = query.Where(task => task.OwnerId == CurrentUserId);
        }
        else if (!string.IsNullOrWhiteSpace(filter.OwnerId))
        {
            query = query.Where(task => task.OwnerId == filter.OwnerId);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.Trim();
            query = query.Where(task =>
                task.Title.Contains(searchTerm) ||
                (task.Description != null && task.Description.Contains(searchTerm)));
        }

        if (filter.SubjectId.HasValue)
        {
            query = query.Where(task => task.SubjectId == filter.SubjectId.Value);
        }

        if (filter.Priority.HasValue)
        {
            query = query.Where(task => task.Priority == filter.Priority.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(task => task.Status == filter.Status.Value);
        }

        if (filter.DueFrom.HasValue)
        {
            query = query.Where(task => task.DueDate >= filter.DueFrom.Value);
        }

        if (filter.DueTo.HasValue)
        {
            query = query.Where(task => task.DueDate <= filter.DueTo.Value);
        }

        var tasks = await query
            .OrderBy(task => task.DueDate)
            .ThenByDescending(task => task.Priority)
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var items = tasks.Select(task => new TaskListItemViewModel
        {
            Id = task.Id,
            Title = task.Title,
            SubjectName = task.Subject.Name,
            SubjectAccentColor = task.Subject.AccentColor,
            OwnerName = task.Owner.FullName,
            DueDate = task.DueDate,
            Priority = task.Priority,
            Status = task.Status,
            EstimatedHours = task.EstimatedHours,
            IsOverdue = task.IsOverdue(today)
        }).ToList();

        var viewModel = new TasksIndexViewModel
        {
            Filter = filter,
            IsAdmin = CurrentUserIsAdmin,
            OwnerOptions = CurrentUserIsAdmin
                ? await BuildOwnerOptionsAsync(cancellationToken)
                : [],
            SubjectOptions = await BuildSubjectOptionsAsync(filter.OwnerId, filter.SubjectId, cancellationToken),
            Items = items,
            VisibleTaskCount = items.Count,
            OverdueTaskCount = items.Count(task => task.IsOverdue)
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var task = await FindTaskAsync(id, cancellationToken);
        if (task is null)
        {
            return NotFound();
        }

        var viewModel = new TaskDetailsViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            SubjectName = task.Subject.Name,
            SubjectAccentColor = task.Subject.AccentColor,
            OwnerName = task.Owner.FullName,
            DueDate = task.DueDate,
            Priority = task.Priority,
            Status = task.Status,
            EstimatedHours = task.EstimatedHours,
            CreatedOnUtc = task.CreatedOnUtc,
            CompletedOnUtc = task.CompletedOnUtc
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var subjectOptions = await BuildSubjectOptionsAsync(null, null, cancellationToken);
        if (subjectOptions.Count == 0)
        {
            SetFlashMessage("Create a subject before adding a study task.", "warning");
            return RedirectToAction("Create", "Subjects");
        }

        var viewModel = new StudyTaskFormViewModel
        {
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            SubjectOptions = subjectOptions
        };

        return View("CreateEdit", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudyTaskFormViewModel viewModel, CancellationToken cancellationToken)
    {
        var subject = await ValidateAndGetSubjectAsync(viewModel.SubjectId, null, cancellationToken);
        viewModel.SubjectOptions = await BuildSubjectOptionsAsync(null, viewModel.SubjectId, cancellationToken);

        if (!ModelState.IsValid || subject is null)
        {
            return View("CreateEdit", viewModel);
        }

        var task = new StudyTask
        {
            Title = viewModel.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(viewModel.Description) ? null : viewModel.Description.Trim(),
            SubjectId = subject.Id,
            OwnerId = subject.OwnerId,
            DueDate = viewModel.DueDate!.Value,
            Priority = viewModel.Priority,
            Status = viewModel.Status,
            EstimatedHours = viewModel.EstimatedHours,
            CreatedOnUtc = DateTime.UtcNow,
            CompletedOnUtc = viewModel.Status == StudyTaskStatus.Done ? DateTime.UtcNow : null
        };

        _dbContext.StudyTasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _deadlineNotificationService.RefreshForUserAsync(task.OwnerId, cancellationToken);

        SetFlashMessage("Study task created.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var task = await FindTaskAsync(id, cancellationToken);
        if (task is null)
        {
            return NotFound();
        }

        var viewModel = new StudyTaskFormViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            SubjectId = task.SubjectId,
            DueDate = task.DueDate,
            Priority = task.Priority,
            Status = task.Status,
            EstimatedHours = task.EstimatedHours,
            SubjectOptions = await BuildSubjectOptionsAsync(task.OwnerId, task.SubjectId, cancellationToken)
        };

        return View("CreateEdit", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StudyTaskFormViewModel viewModel, CancellationToken cancellationToken)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        var task = await FindTaskAsync(id, cancellationToken);
        if (task is null)
        {
            return NotFound();
        }

        var subject = await ValidateAndGetSubjectAsync(viewModel.SubjectId, task.OwnerId, cancellationToken);
        viewModel.SubjectOptions = await BuildSubjectOptionsAsync(task.OwnerId, viewModel.SubjectId, cancellationToken);

        if (!ModelState.IsValid || subject is null)
        {
            return View("CreateEdit", viewModel);
        }

        task.Title = viewModel.Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(viewModel.Description) ? null : viewModel.Description.Trim();
        task.SubjectId = subject.Id;
        task.DueDate = viewModel.DueDate!.Value;
        task.Priority = viewModel.Priority;
        task.Status = viewModel.Status;
        task.EstimatedHours = viewModel.EstimatedHours;
        task.CompletedOnUtc = task.Status == StudyTaskStatus.Done
            ? task.CompletedOnUtc ?? DateTime.UtcNow
            : null;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _deadlineNotificationService.RefreshForUserAsync(task.OwnerId, cancellationToken);

        SetFlashMessage("Study task updated.");
        return RedirectToAction(nameof(Details), new { id = task.Id });
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var task = await FindTaskAsync(id, cancellationToken);
        if (task is null)
        {
            return NotFound();
        }

        var viewModel = new TaskDetailsViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            SubjectName = task.Subject.Name,
            SubjectAccentColor = task.Subject.AccentColor,
            OwnerName = task.Owner.FullName,
            DueDate = task.DueDate,
            Priority = task.Priority,
            Status = task.Status,
            EstimatedHours = task.EstimatedHours,
            CreatedOnUtc = task.CreatedOnUtc,
            CompletedOnUtc = task.CompletedOnUtc
        };

        return View(viewModel);
    }

    [HttpPost, ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var task = await FindTaskAsync(id, cancellationToken);
        if (task is null)
        {
            return NotFound();
        }

        _dbContext.StudyTasks.Remove(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _deadlineNotificationService.RefreshForUserAsync(task.OwnerId, cancellationToken);

        SetFlashMessage("Study task deleted.");
        return RedirectToAction(nameof(Index));
    }

    private async Task<StudyTask?> FindTaskAsync(int id, CancellationToken cancellationToken)
    {
        var query = _dbContext.StudyTasks
            .Include(task => task.Subject)
            .Include(task => task.Owner)
            .AsQueryable();

        if (!CurrentUserIsAdmin)
        {
            query = query.Where(task => task.OwnerId == CurrentUserId);
        }

        return await query.SingleOrDefaultAsync(task => task.Id == id, cancellationToken);
    }

    private async Task<Subject?> ValidateAndGetSubjectAsync(int subjectId, string? enforcedOwnerId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Subjects.AsQueryable();

        if (!CurrentUserIsAdmin)
        {
            query = query.Where(subject => subject.OwnerId == CurrentUserId);
        }
        else if (!string.IsNullOrWhiteSpace(enforcedOwnerId))
        {
            query = query.Where(subject => subject.OwnerId == enforcedOwnerId);
        }

        var subject = await query.SingleOrDefaultAsync(subject => subject.Id == subjectId, cancellationToken);
        if (subject is null)
        {
            ModelState.AddModelError(nameof(StudyTaskFormViewModel.SubjectId), "Choose a valid subject.");
        }

        return subject;
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

    private async Task<IReadOnlyList<SelectListItem>> BuildSubjectOptionsAsync(
        string? ownerId,
        int? selectedSubjectId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Subjects
            .AsNoTracking()
            .OrderBy(subject => subject.Code)
            .ThenBy(subject => subject.Name)
            .AsQueryable();

        if (!CurrentUserIsAdmin)
        {
            query = query.Where(subject => subject.OwnerId == CurrentUserId);
        }
        else if (!string.IsNullOrWhiteSpace(ownerId))
        {
            query = query.Where(subject => subject.OwnerId == ownerId);
        }

        return await query
            .Select(subject => new SelectListItem
            {
                Value = subject.Id.ToString(),
                Text = $"{subject.Code} · {subject.Name}",
                Selected = selectedSubjectId.HasValue && subject.Id == selectedSubjectId.Value
            })
            .ToListAsync(cancellationToken);
    }
}
