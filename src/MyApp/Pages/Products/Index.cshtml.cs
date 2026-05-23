using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Model;

namespace MyApp.Pages.Products;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context) => _context = context;

    public List<Product> Products { get; set; } = [];

    public async Task OnGetAsync()
    {
        Products = await _context.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
    }
}
