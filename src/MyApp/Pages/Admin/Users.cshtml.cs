using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyApp.Common;
using MyApp.Model;

namespace MyApp.Pages.Admin;

[Authorize(Roles = AppRoles.Admin)]
public class UsersModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersModel(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public List<UserViewModel> Users { get; set; } = [];

    public async Task OnGetAsync()
    {
        var users = await _userManager.Users.AsNoTracking().ToListAsync();
        var result = new List<UserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Role = roles.FirstOrDefault() ?? "(none)",
                IsActive = user.IsActive,
                IsSuspended = user.IsSuspended
            });
        }

        Users = result.OrderBy(u => u.Email).ToList();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleSuspendAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        user.IsSuspended = !user.IsSuspended;
        await _userManager.UpdateAsync(user);
        return RedirectToPage();
    }

    public record UserViewModel
    {
        public string Id { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public bool IsSuspended { get; init; }
    }
}
