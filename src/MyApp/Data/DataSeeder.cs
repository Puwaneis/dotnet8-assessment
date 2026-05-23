using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Common;
using MyApp.Model;

namespace MyApp.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        // Seed roles
        string[] roles = [AppRoles.Admin, AppRoles.Manager, AppRoles.User];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed default admin user
        if (await userManager.FindByEmailAsync("admin@app.com") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@app.com",
                Email = "admin@app.com",
                FullName = "System Admin",
                IsActive = true,
                IsSuspended = false,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, AppRoles.Admin);
        }

        // Seed sample products
        if (!await context.Products.AnyAsync())
        {
            context.Products.AddRange(
                new Product
                {
                    Name = "Widget A",
                    Description = "Standard widget",
                    Price = 9.99m,
                    Quantity = 100,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Widget B",
                    Description = "Premium widget",
                    Price = 24.99m,
                    Quantity = 50,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();
        }
    }
}
