using HLIBAssessment.Common;
using HLIBAssessment.Data;
using HLIBAssessment.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HLIBAssessment.Pages.Customers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context) => _context = context;

        public Customer Customer { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            Customer = customer;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            // TODO Task 5: pass real open-order count when transactions exist
            const int openOrderCount = 0;
            var (canDelete, error) = CustomerRules.ValidateDeletion(customer, openOrderCount);
            if (!canDelete)
            {
                ModelState.AddModelError(string.Empty, error!);
                Customer = customer;
                return Page();
            }

            _context.AuditLogs.Add(new AuditLog
            {
                EntityName = "Customer",
                EntityId = customer.Id.ToString(),
                Action = AuditType.Deleted,
                ChangedBy = User.Identity!.Name!,
                ChangedAt = DateTime.UtcNow,
                Remarks = $"Deleted: {customer.Username}"
            });

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
