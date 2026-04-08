using System.Security.Claims;

namespace StudyFlowPro.Web.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserIdOrNull(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public static string GetUserIdOrThrow(this ClaimsPrincipal principal)
    {
        return principal.GetUserIdOrNull()
            ?? throw new InvalidOperationException("A signed-in user is required for this action.");
    }
}
