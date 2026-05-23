using System.Security.Claims;
using System.Security.Principal;

namespace MyApp.Helpers;

public static class IdentityExtension
{
    public static string? GetFullName(this IIdentity identity)
    {
        return ((ClaimsIdentity)identity).FindFirst("FullName")?.Value;
    }
}
