# .NET 8 Technical Assessment

You are given a .NET 8 Razor Pages boilerplate application built with ASP.NET Core Identity, Entity Framework Core (SQLite), role-based authorization, audit logging, and NLog.

Complete all tasks below. Your submission will be reviewed and followed up with a technical discussion.

> **Before you start:** Read [README.md](README.md) for prerequisites and how to run the application.

---

## Submission Checklist

First Section (Eta 1 hours)
- [ ] Project renamed (Task 1)
- [ ] New entity with full CRUD implemented (Task 2)
- [ ] Migration created and applied (Task 3)
- [ ] Unit tests written and passing (Task 4)
- [ ] Repository pushed to your own GitHub account
Second Section (Eta 2 hours)
- [ ] Solutioning based on use case. (Task 5)
- [ ] Repository pushed to your own GitHub account
- [ ] Share the repository link upon submission

---

## Task 1 — Rename the Project

Rename all occurrences of `MyApp` to your chosen project name (e.g. `AcmePortal`).

### 1.1 — Rename files and folders

```
dotnet7BoilerPlate/
  src/
    MyApp/           ← rename folder to AcmePortal
      MyApp.csproj   ← rename file to AcmePortal.csproj
  tests/
    MyApp.Tests/     ← rename folder to AcmePortal.Tests
      MyApp.Tests.csproj ← rename file to AcmePortal.Tests.csproj
  MyApp.slnx         ← rename file to AcmePortal.slnx (or keep it)
```

### 1.2 — Update the solution file

Open `AcmePortal.slnx` and replace all `MyApp` references with `AcmePortal`.

### 1.3 — Find and replace in code

Perform a **case-sensitive find & replace** across the entire project:

| Find | Replace |
|------|---------|
| `MyApp` | `AcmePortal` |
| `myapp.db` | `acmeportal.db` |

Files that will be affected:
- All `.cs` files (namespaces: `namespace MyApp.*`, `using MyApp.*`)
- `appsettings.json` (connection string)
- `_ViewImports.cshtml`
- `Program.cs`
- `MyApp.csproj` → `UserSecretsId` attribute

### 1.4 — Verify the build

```powershell
dotnet build src/AcmePortal/AcmePortal.csproj
```

Expected: `0 errors, 0 warnings`

---

## Task 2 — Create a New Entity (Full CRUD)

Add a new entity of your choice to the application. Use `Customer` as a reference example below. Your entity must have at least 3 meaningful fields beyond `Id`, `CreatedAt`, and `UpdatedAt`.

### 2.1 — Define the model

Create `src/MyApp/Model/Customer.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace MyApp.Model;

public class Customer
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### 2.2 — Register in ApplicationDbContext

Edit `src/MyApp/Data/ApplicationDbContext.cs`:

```csharp
public DbSet<Customer> Customers { get; set; }
```

### 2.3 — Create and apply the migration

```powershell
dotnet ef migrations add AddCustomer --project src/MyApp/MyApp.csproj --startup-project src/MyApp/MyApp.csproj
dotnet ef database update --project src/MyApp/MyApp.csproj --startup-project src/MyApp/MyApp.csproj
```

### 2.4 — Index page

Create `src/MyApp/Pages/Customers/Index.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Model;

namespace MyApp.Pages.Customers;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context) => _context = context;

    public IList<Customer> Customers { get; set; } = [];

    public async Task OnGetAsync()
    {
        Customers = await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
```

Create `src/MyApp/Pages/Customers/Index.cshtml`:

```html
@page
@model MyApp.Pages.Customers.IndexModel
@{
    ViewData["Title"] = "Customers";
}

<div class="d-flex justify-content-between align-items-center mb-3">
    <h4>Customers</h4>
    @if (User.IsInRole(AppRoles.Admin))
    {
        <a asp-page="./Create" class="btn btn-primary">Add Customer</a>
    }
</div>

<table id="customersTable" class="table table-striped table-hover">
    <thead>
        <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Phone</th>
            <th>Status</th>
            <th></th>
        </tr>
    </thead>
    <tbody>
        @foreach (var c in Model.Customers)
        {
            <tr>
                <td>@c.Name</td>
                <td>@c.Email</td>
                <td>@c.Phone</td>
                <td>
                    <span class="badge @(c.IsActive ? "bg-success" : "bg-secondary")">
                        @(c.IsActive ? "Active" : "Inactive")
                    </span>
                </td>
                <td>
                    @if (User.IsInRole(AppRoles.Admin))
                    {
                        <a asp-page="./Edit" asp-route-id="@c.Id" class="btn btn-sm btn-primary">Edit</a>
                        <a asp-page="./Delete" asp-route-id="@c.Id" class="btn btn-sm btn-danger ms-1">Delete</a>
                    }
                </td>
            </tr>
        }
    </tbody>
</table>

@section Scripts {
    <script>$('#customersTable').DataTable();</script>
}
```

### 2.5 — Create page

Create `src/MyApp/Pages/Customers/Create.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyApp.Common;
using MyApp.Data;
using MyApp.Model;

namespace MyApp.Pages.Customers;

[Authorize(Roles = AppRoles.Admin)]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context) => _context = context;

    [BindProperty]
    public Customer NewCustomer { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        NewCustomer.CreatedAt = DateTime.UtcNow;
        NewCustomer.UpdatedAt = DateTime.UtcNow;

        _context.Customers.Add(NewCustomer);

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "Customer",
            EntityId = "0",
            Action = AuditType.Created,
            ChangedBy = User.Identity!.Name!,
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }
}
```

Create `src/MyApp/Pages/Customers/Create.cshtml`:

```html
@page
@model MyApp.Pages.Customers.CreateModel
@{
    ViewData["Title"] = "Add Customer";
}

<h4>Add Customer</h4>

<div class="card" style="max-width:500px;">
    <div class="card-body">
        <form method="post">
            <div class="mb-3">
                <label asp-for="NewCustomer.Name" class="form-label">Name</label>
                <input asp-for="NewCustomer.Name" class="form-control" />
                <span asp-validation-for="NewCustomer.Name" class="text-danger"></span>
            </div>
            <div class="mb-3">
                <label asp-for="NewCustomer.Email" class="form-label">Email</label>
                <input asp-for="NewCustomer.Email" class="form-control" type="email" />
            </div>
            <div class="mb-3">
                <label asp-for="NewCustomer.Phone" class="form-label">Phone</label>
                <input asp-for="NewCustomer.Phone" class="form-control" />
            </div>
            <button type="submit" class="btn btn-success">Save</button>
            <a asp-page="./Index" class="btn btn-secondary ms-2">Cancel</a>
        </form>
    </div>
</div>

@section Scripts {
    @await Html.PartialAsync("_ValidationScriptsPartial")
}
```

### 2.6 — Edit page

Create `src/MyApp/Pages/Customers/Edit.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyApp.Common;
using MyApp.Data;
using MyApp.Model;

namespace MyApp.Pages.Customers;

[Authorize(Roles = AppRoles.Admin)]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context) => _context = context;

    [BindProperty]
    public Customer Customer { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
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

        existing.Name = Customer.Name;
        existing.Email = Customer.Email;
        existing.Phone = Customer.Phone;
        existing.IsActive = Customer.IsActive;
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
```

Create `src/MyApp/Pages/Customers/Edit.cshtml`:

```html
@page "{id:int}"
@model MyApp.Pages.Customers.EditModel
@{
    ViewData["Title"] = "Edit Customer";
}

<h4>Edit Customer</h4>

<div class="card" style="max-width:500px;">
    <div class="card-body">
        <form method="post">
            <input type="hidden" asp-for="Customer.Id" />
            <div class="mb-3">
                <label asp-for="Customer.Name" class="form-label">Name</label>
                <input asp-for="Customer.Name" class="form-control" />
                <span asp-validation-for="Customer.Name" class="text-danger"></span>
            </div>
            <div class="mb-3">
                <label asp-for="Customer.Email" class="form-label">Email</label>
                <input asp-for="Customer.Email" class="form-control" type="email" />
            </div>
            <div class="mb-3">
                <label asp-for="Customer.Phone" class="form-label">Phone</label>
                <input asp-for="Customer.Phone" class="form-control" />
            </div>
            <div class="mb-3">
                <div class="form-check">
                    <input asp-for="Customer.IsActive" class="form-check-input" type="checkbox" />
                    <label asp-for="Customer.IsActive" class="form-check-label">Active</label>
                </div>
            </div>
            <button type="submit" class="btn btn-primary">Update</button>
            <a asp-page="./Index" class="btn btn-secondary ms-2">Cancel</a>
        </form>
    </div>
</div>

@section Scripts {
    @await Html.PartialAsync("_ValidationScriptsPartial")
}
```

### 2.7 — Delete page

Create `src/MyApp/Pages/Customers/Delete.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyApp.Common;
using MyApp.Data;
using MyApp.Model;

namespace MyApp.Pages.Customers;

[Authorize(Roles = AppRoles.Admin)]
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context) => _context = context;

    public Customer Customer { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        Customer = customer;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "Customer",
            EntityId = customer.Id.ToString(),
            Action = AuditType.Deleted,
            ChangedBy = User.Identity!.Name!,
            ChangedAt = DateTime.UtcNow,
            Remarks = $"Deleted: {customer.Name}"
        });

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }
}
```

Create `src/MyApp/Pages/Customers/Delete.cshtml`:

```html
@page "{id:int}"
@model MyApp.Pages.Customers.DeleteModel
@{
    ViewData["Title"] = "Delete Customer";
}

<h4>Delete Customer</h4>
<div class="alert alert-danger">Are you sure you want to delete <strong>@Model.Customer.Name</strong>?</div>

<form method="post">
    <button type="submit" class="btn btn-danger">Yes, Delete</button>
    <a asp-page="./Index" class="btn btn-secondary ms-2">Cancel</a>
</form>
```

### 2.8 — Add to sidebar navigation

Edit `src/MyApp/Pages/Shared/_Layout.cshtml` — add inside the sidebar section:

```html
<div class="sidebar-heading">Customers</div>
<a asp-page="/Customers/Index">All Customers</a>
@if (User.IsInRole(AppRoles.Admin))
{
    <a asp-page="/Customers/Create">Add Customer</a>
}
```

---

## Task 3 — Database Migrations

### 3.1 — Add a migration

After modifying a model or `ApplicationDbContext`:

```powershell
dotnet ef migrations add <MigrationName> --project src/MyApp/MyApp.csproj --startup-project src/MyApp/MyApp.csproj
```

Use descriptive names: `AddCustomer`, `AddCustomerEmailIndex`.

### 3.2 — Apply the migration

```powershell
dotnet ef database update --project src/MyApp/MyApp.csproj --startup-project src/MyApp/MyApp.csproj
```

### 3.3 — Save manual SQL scripts

For changes that cannot be expressed as EF migrations (e.g. seed data, index hints), create a file in `src/MyApp/sql/` using the naming convention:

```
YYYY-MM-DD-description.sql
```

Write idempotent SQL — safe to run multiple times. Commit the file to git.

---

## Task 4 — Unit Tests

Write unit tests for the business logic in your new entity. Tests must live in `tests/MyApp.Tests/`.

### 4.1 — Extract business logic

If your entity has validation rules or state transitions, extract them into a static class under `src/MyApp/Common/`. This makes the logic testable without a database or web server.

Example — `src/MyApp/Common/CustomerRules.cs`:

```csharp
namespace MyApp.Common;

public static class CustomerRules
{
    public static (bool IsValid, string? ErrorMessage) ValidateDeactivation(Customer customer, int openOrderCount)
    {
        if (openOrderCount > 0)
            return (false, $"Cannot deactivate customer with {openOrderCount} open order(s).");

        return (true, null);
    }
}
```

### 4.2 — Write the tests

Create `tests/MyApp.Tests/Workflow/CustomerRulesTests.cs`:

```csharp
using MyApp.Common;
using MyApp.Model;

namespace MyApp.Tests.Workflow;

public class CustomerRulesTests
{
    [Fact]
    public void ValidateDeactivation_NoOpenOrders_IsValid()
    {
        var customer = new Customer { Id = 1, Name = "Acme" };
        var (isValid, error) = CustomerRules.ValidateDeactivation(customer, 0);
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void ValidateDeactivation_HasOpenOrders_IsInvalid()
    {
        var customer = new Customer { Id = 1, Name = "Acme" };
        var (isValid, error) = CustomerRules.ValidateDeactivation(customer, 3);
        Assert.False(isValid);
        Assert.NotNull(error);
    }
}
```

Each rule must have tests for: happy path, edge case, and failure path.

### 4.3 — Run the tests

```powershell
dotnet test tests/MyApp.Tests/MyApp.Tests.csproj
```

All tests must pass before submission.

---
## Task 5 — Solutioning (Based on problem statement)

We want to record transactions between a Customer and a Product. Each transaction must capture the following fields: Date of Purchase, Customer Name, Quantity, Total Price, Product Name, and a unique Transaction Reference Number.

Requirements:

- [Validation] A transaction cannot be submitted if the requested quantity exceeds the current available Product quantity.
- The total price must be auto-calculated based on the Product price and the requested quantity.
- The Product quantity must be deducted upon successful transaction submission.
- An audit entry must be created in the audit log upon successful transaction submission.

Please ensure the quantity validation is covered by a unit test.

---
## Task 6 — Technical Discussion (Post-Submission)

After reviewing your submission, you will be asked to walk through and explain the following topics based on what you have implemented:

- Authorization and role-based access control
- Audit logging
- Business logic placement and separation of concerns
- Logging and error handling
- Entity Framework Core query patterns
- `[BindProperty]` usage and form binding

There are no right or wrong ways to answer — explain based on your own understanding of the code you wrote.
