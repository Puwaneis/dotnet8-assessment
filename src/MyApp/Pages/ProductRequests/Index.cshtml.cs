using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyApp.Common;
using MyApp.Data;
using MyApp.Model;

namespace MyApp.Pages.ProductRequests;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context) => _context = context;

    public List<ProductRequest> Requests { get; set; } = [];

    public async Task OnGetAsync()
    {
        var userId = User.Identity!.Name!;
        Requests = await _context.ProductRequests
            .AsNoTracking()
            .Include(r => r.Product)
            .Where(r => r.CreatedBy == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostSubmitAsync(int requestId)
    {
        var userId = User.Identity!.Name!;
        var request = await _context.ProductRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.CreatedBy == userId);

        if (request == null) return NotFound();
        if (request.Status != RequestStatus.Draft) return RedirectToPage();

        request.Status = RequestStatus.Submitted;
        request.UpdatedAt = DateTime.UtcNow;

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "ProductRequest",
            EntityId = request.Id.ToString(),
            Action = AuditType.Submitted,
            ChangedBy = userId,
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return RedirectToPage();
    }
}
