using HLIBAssessment.Common;
using HLIBAssessment.Data;
using HLIBAssessment.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HLIBAssessment.Pages.Transactions
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Guid CustomerId { get; set; }

        [BindProperty]
        public int ProductId { get; set; }

        [BindProperty]
        public int Quantity { get; set; }

        public SelectList CustomerSelectList { get; set; } = null!;
        public SelectList ProductSelectList { get; set; } = null!;

        public async Task OnGetAsync()
        {
            await LoadSelectListsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (CustomerId == Guid.Empty)
                ModelState.AddModelError(nameof(CustomerId), "Please select a customer.");

            if (ProductId <= 0)
                ModelState.AddModelError(nameof(ProductId), "Please select a product.");

            if (Quantity <= 0)
                ModelState.AddModelError(nameof(Quantity), "Quantity must be at least 1.");

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            var customer = await _context.Customers.FindAsync(CustomerId);
            var product = await _context.Products.FindAsync(ProductId);

            if (customer == null || product == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid customer or product.");
                await LoadSelectListsAsync();
                return Page();
            }

            var (customerIsValid, customerError) = CustomerRules.ValidateCanPurchase(customer);
            if (!customerIsValid)
            {
                ModelState.AddModelError(string.Empty, customerError!);
                await LoadSelectListsAsync();
                return Page();
            }

            var (isQuantityValid, quantityError) = TransactionRules.ValidateQuantity(product, Quantity);
            if (!isQuantityValid)
            {
                ModelState.AddModelError(nameof(Quantity), quantityError!);
                await LoadSelectListsAsync();
                return Page();
            }

            var transaction = new Transaction
            {
                CustomerId = customer.Id,
                ProductId = product.Id,
                CustomerUsername = customer.Username,
                ProductName = product.Name,
                Quantity = Quantity,
                TotalPrice = TransactionRules.CalculateTotalPrice(product, Quantity),
                TransactionReferenceNumber = TransactionRules.GenerateReferenceNumber(),
                DateOfPurchase = DateTime.UtcNow
            };

            TransactionRules.ApplyPurchase(product, Quantity);

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            _context.AuditLogs.Add(new AuditLog
            {
                EntityName = "Transaction",
                EntityId = transaction.Id.ToString(),
                Action = AuditType.Created,
                ChangedBy = User.Identity!.Name!,
                ChangedAt = DateTime.UtcNow,
                Remarks = $"Ref: {transaction.TransactionReferenceNumber}, Customer: {transaction.CustomerUsername}, Product: {transaction.ProductName}, Qty: {transaction.Quantity}, Total: {transaction.TotalPrice:C}"
            });
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private async Task LoadSelectListsAsync()
        {
            var customers = await _context.Customers
                .AsNoTracking()
                .Where(c => c.Status)
                .OrderBy(c => c.Username)
                .ToListAsync();

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.Quantity > 0)
                .OrderBy(p => p.Name)
                .ToListAsync();

            CustomerSelectList = new SelectList(customers, "Id", "Username", CustomerId);
            ProductSelectList = new SelectList(products, "Id", "Name", ProductId);
        }
    }
}
