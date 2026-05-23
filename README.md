# dotnet8Boilerplate

A .NET 8 Razor Pages boilerplate with ASP.NET Core Identity, Entity Framework Core, role-based authorization, audit logging, and NLog — ready to rename and build on.

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Git](https://git-scm.com/)
- A code editor — Visual Studio 2022, VS Code, or Rider
- SQLite (default, zero config) or SQL Server (production)

---

## Quick Start

### 1. Clone the repository

```bash
git clone https://github.com/juanyeanhlib/dotnet8Boilerplate-assessment.git
cd dotnet8Boilerplate
```

### 2. Restore and build

```powershell
dotnet build src/MyApp/MyApp.csproj
```

Expected output:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 3. Apply database migrations

The default database is **SQLite** (`myapp.db`) — no installation required.

```powershell
dotnet ef database update --project src/MyApp/MyApp.csproj --startup-project src/MyApp/MyApp.csproj
```

> **First time running `dotnet ef`?** Install the EF Core CLI tool first:
> ```powershell
> dotnet tool install --global dotnet-ef
> ```

This creates `src/MyApp/myapp.db` and applies all migrations including seeded data:

| Account | Password | Role |
|---------|----------|------|
| admin@app.com | Admin@123 | Admin |

Two sample products (Widget A, Widget B) are also seeded.

### 4. Run the application

```powershell
dotnet dev-certs https --trust
dotnet run --project src/MyApp/MyApp.csproj --launch-profile https
```

> Run `dotnet dev-certs https --trust` once per machine — it installs the local dev certificate so HTTPS works without browser warnings.

Open **https://localhost:7227** — you will be redirected to the login page automatically.

| Login URL | https://localhost:7227/Identity/Account/Login |
|-----------|----------------------------------------------|
| Email | `admin@app.com` |
| Password | `Admin@123` |

### 5. Run the tests

```powershell
dotnet test tests/MyApp.Tests/MyApp.Tests.csproj
```

Expected output:
```
Passed!  - Failed: 0, Passed: 8, Skipped: 0
```

---

## Next Steps

Refer [Assignment.md](Assignment.md) for the assessment
<!--
- Renaming the project
- Adding entities with full CRUD
- Database migrations
- Writing unit tests
- Best practices (authorization, audit logging, EF Core patterns)
-->