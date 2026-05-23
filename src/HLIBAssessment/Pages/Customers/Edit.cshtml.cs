using HLIBAssessment.Common;
using HLIBAssessment.Data;
using HLIBAssessment.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HLIBAssessment.Pages.Customers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Customer Customer { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            Customer = customer;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var existing = await _context.Customers.FindAsync(Customer.Id);
            if (existing == null) return NotFound();

            var (isValid, error, field) = CustomerRules.ValidateCreate(Customer);
            if (!isValid)
            {
                ModelState.AddModelError($"{nameof(Customer)}.{field}", error!);
                return Page();
            }

            const int openOrderCount = 0;
            if (existing.Status && !Customer.Status)
            {
                var (canDeactivate, deactivationError) = CustomerRules.ValidateDeactivation(existing, openOrderCount);
                if (!canDeactivate)
                {
                    ModelState.AddModelError(string.Empty, deactivationError!);
                    return Page();
                }
            }

            existing.Username = Customer.Username;
            existing.Email = Customer.Email;
            existing.PhoneNumber = Customer.PhoneNumber;
            existing.Status = Customer.Status;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.AuditLogs.Add(new AuditLog
            {
                EntityName = "Customer",
                EntityId = existing.Id.ToString(),
                Action = AuditType.Updated,
                ChangedBy = User.Identity!.Name!,
                ChangedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
