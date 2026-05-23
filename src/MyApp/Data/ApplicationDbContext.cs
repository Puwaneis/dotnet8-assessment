using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyApp.Model;

namespace MyApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductRequest> ProductRequests => Set<ProductRequest>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
}
