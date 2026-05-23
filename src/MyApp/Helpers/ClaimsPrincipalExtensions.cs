using System.Security.Claims;

namespace MyApp.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static bool IsImpersonating(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.HasClaim("IsImpersonating", "true");
    }
}
