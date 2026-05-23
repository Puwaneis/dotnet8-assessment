using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyApp.Common;
using MyApp.Data;
using MyApp.Model;

namespace MyApp.Pages.ProductRequests;

[Authorize(Roles = $"{AppRoles.Manager},{AppRoles.Admin}")]
public class PendingModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public PendingModel(ApplicationDbContext context) => _context = context;

    public List<ProductRequest> Requests { get; set; } = [];

    public async Task OnGetAsync()
    {
        Requests = await _context.ProductRequests
            .AsNoTracking()
            .Include(r => r.Product)
            .Where(r => r.Status == RequestStatus.Submitted)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }
}
