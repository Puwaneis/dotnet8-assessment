using HLIBAssessment.Common;
using HLIBAssessment.Data;
using HLIBAssessment.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HLIBAssessment.Pages.Customers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Customer NewCustomer { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var (isValid, error, field) = CustomerRules.ValidateCreate(NewCustomer);
            if (!isValid)
            {
                ModelState.AddModelError($"{nameof(NewCustomer)}.{field}", error!);
                return Page();
            }

            NewCustomer.Id = Guid.NewGuid();
            NewCustomer.CreatedAt = DateTime.UtcNow;
            NewCustomer.UpdatedAt = DateTime.UtcNow;

            _context.Customers.Add(NewCustomer);
            await _context.SaveChangesAsync();

            _context.AuditLogs.Add(new AuditLog
            {
                EntityName = "Customer",
                EntityId = NewCustomer.Id.ToString(),
                Action = AuditType.Created,
                ChangedBy = User.Identity!.Name!,
                ChangedAt = DateTime.UtcNow,
                Remarks = $"Created: {NewCustomer.Username}"
            });

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }

    }
}
