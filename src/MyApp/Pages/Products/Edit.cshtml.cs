using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyApp.Common;
using MyApp.Data;
using MyApp.Model;

namespace MyApp.Pages.Products;

[Authorize(Roles = AppRoles.Admin)]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context) => _context = context;

    [BindProperty]
    public Product Product { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        Product = product;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var existing = await _context.Products.FindAsync(Product.Id);
        if (existing == null) return NotFound();

        existing.Name = Product.Name;
        existing.Description = Product.Description;
        existing.Price = Product.Price;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }
}
