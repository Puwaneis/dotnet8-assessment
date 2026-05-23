using HLIBAssessment.Data;
using HLIBAssessment.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HLIBAssessment.Pages.Customers
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public List<Customer> Customers { get; set; } = [];

        public async Task OnGetAsync()
        {
            Customers = await _context.Customers
                .AsNoTracking()
                .OrderBy(c => c.Username)
                .ToListAsync();
        }
    }
}
