using Microsoft.AspNetCore.Identity;
using MyApp.Model;

namespace MyApp.Helpers;

public static class HttpContextExtensions
{
    public static async Task RefreshLoginAsync(this HttpContext context)
    {
        if (context.User == null)
            return;

        var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var signInManager = context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();

        var user = await userManager.GetUserAsync(context.User);
        if (user != null && signInManager.IsSignedIn(context.User))
            await signInManager.RefreshSignInAsync(user);
    }
}
