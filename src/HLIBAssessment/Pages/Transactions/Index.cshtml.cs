using HLIBAssessment.Data;
using HLIBAssessment.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HLIBAssessment.Pages.Transactions;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context) => _context = context;

    public IList<Transaction> Transactions { get; set; } = [];

    public async Task OnGetAsync()
    {
        Transactions = await _context.Transactions
            .AsNoTracking()
            .OrderByDescending(t => t.DateOfPurchase)
            .ToListAsync();
    }
}
