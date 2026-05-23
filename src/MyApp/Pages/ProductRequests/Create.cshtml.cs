using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyApp.Common;
using MyApp.Data;
using MyApp.Model;

namespace MyApp.Pages.ProductRequests;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context) => _context = context;

    [BindProperty]
    public ProductRequest NewRequest { get; set; } = new();

    public SelectList ProductSelectList { get; set; } = null!;

    public async Task OnGetAsync()
    {
        var products = await _context.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
        ProductSelectList = new SelectList(products, "Id", "Name");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var products = await _context.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
            ProductSelectList = new SelectList(products, "Id", "Name");
            return Page();
        }

        NewRequest.Status = RequestStatus.Draft;
        NewRequest.CreatedBy = User.Identity!.Name!;
        NewRequest.CreatedAt = DateTime.UtcNow;
        NewRequest.UpdatedAt = DateTime.UtcNow;

        _context.ProductRequests.Add(NewRequest);
        await _context.SaveChangesAsync(); // Save first so NewRequest.Id is populated

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "ProductRequest",
            EntityId = NewRequest.Id.ToString(),
            Action = AuditType.Created,
            ChangedBy = User.Identity!.Name!,
            ChangedAt = DateTime.UtcNow,
            Remarks = $"Type: {NewRequest.RequestType}, Qty: {NewRequest.RequestedQuantity}"
        });
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
