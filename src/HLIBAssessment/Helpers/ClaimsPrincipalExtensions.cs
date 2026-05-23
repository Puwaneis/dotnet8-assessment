using System.Security.Claims;

namespace HLIBAssessment.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static bool IsImpersonating(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.HasClaim("IsImpersonating", "true");
    }
}
