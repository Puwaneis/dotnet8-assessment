using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyApp.Common;
using MyApp.Data;
using MyApp.Model;

namespace MyApp.Pages.Products;

[Authorize(Roles = AppRoles.Admin)]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context) => _context = context;

    [BindProperty]
    public Product Product { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        Product.CreatedAt = DateTime.UtcNow;
        Product.UpdatedAt = DateTime.UtcNow;
        _context.Products.Add(Product);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
