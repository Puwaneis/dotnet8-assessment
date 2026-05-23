using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyApp.Common;
using MyApp.Data;
using MyApp.Model;
using MyApp.Services;

namespace MyApp.Pages.ProductRequests;

[Authorize(Roles = $"{AppRoles.Manager},{AppRoles.Admin}")]
public class ReviewModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;

    public ReviewModel(ApplicationDbContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    public ProductRequest ProductRequest { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var request = await _context.ProductRequests
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null) return NotFound();
        if (request.Status != RequestStatus.Submitted) return RedirectToPage("./Pending");

        ProductRequest = request;
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id, string? reviewRemark)
    {
        var request = await _context.ProductRequests
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null) return NotFound();
        if (request.Status != RequestStatus.Submitted) return RedirectToPage("./Pending");

        var (isValid, errorMessage) = ProductRequestWorkflow.ValidateApproval(request);

        if (!isValid)
        {
            request.Status = RequestStatus.Rejected;
            request.ReviewRemark = errorMessage;
        }
        else
        {
            request.Status = RequestStatus.Approved;
            request.ReviewRemark = reviewRemark;
            ProductRequestWorkflow.ApplyQuantityChange(request, request.Product);
        }

        request.UpdatedAt = DateTime.UtcNow;

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "ProductRequest",
            EntityId = request.Id.ToString(),
            Action = request.Status == RequestStatus.Approved ? AuditType.Approved : AuditType.Rejected,
            ChangedBy = User.Identity!.Name!,
            ChangedAt = DateTime.UtcNow,
            Remarks = reviewRemark
        });

        await _context.SaveChangesAsync();

        await _emailSender.SendAsync(
            request.CreatedBy,
            $"Request #{request.Id} {request.Status}",
            $"Your quantity request #{request.Id} for {request.Product.Name} has been {request.Status}. Remark: {request.ReviewRemark}");

        return RedirectToPage("./Pending");
    }

    public async Task<IActionResult> OnPostRejectAsync(int id, string? reviewRemark)
    {
        var request = await _context.ProductRequests
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null) return NotFound();
        if (request.Status != RequestStatus.Submitted) return RedirectToPage("./Pending");

        request.Status = RequestStatus.Rejected;
        request.ReviewRemark = reviewRemark;
        request.UpdatedAt = DateTime.UtcNow;

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "ProductRequest",
            EntityId = request.Id.ToString(),
            Action = AuditType.Rejected,
            ChangedBy = User.Identity!.Name!,
            ChangedAt = DateTime.UtcNow,
            Remarks = reviewRemark
        });

        await _context.SaveChangesAsync();

        await _emailSender.SendAsync(
            request.CreatedBy,
            $"Request #{request.Id} Rejected",
            $"Your quantity request #{request.Id} for {request.Product.Name} has been rejected. Remark: {reviewRemark}");

        return RedirectToPage("./Pending");
    }
}
