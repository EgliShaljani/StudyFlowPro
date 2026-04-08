using Microsoft.AspNetCore.Mvc;
using StudyFlowPro.Web.Extensions;
using StudyFlowPro.Web.Models.Enums;

namespace StudyFlowPro.Web.Controllers;

public abstract class AppController : Controller
{
    protected string CurrentUserId => User.GetUserIdOrThrow();

    protected bool CurrentUserIsAdmin => User.IsInRole(ApplicationRoles.Admin);

    protected void SetFlashMessage(string message, string tone = "success")
    {
        TempData["StatusMessage"] = message;
        TempData["StatusTone"] = tone;
    }
}
