using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MyApp.Model;
using System.Security.Claims;

namespace MyApp.Helpers;

public class CustomClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public CustomClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user);

        if (!string.IsNullOrEmpty(user.FullName) && principal.Identity != null)
        {
            ((ClaimsIdentity)principal.Identity).AddClaim(
                new Claim("FullName", user.FullName));
        }

        return principal;
    }
}
