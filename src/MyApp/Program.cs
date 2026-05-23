using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Helpers;
using MyApp.Middleware;
using MyApp.Model;
using MyApp.Services;
using NLog;
using NLog.Web;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var baseDir = AppContext.BaseDirectory;
var nlogConfigFile = Path.Combine(baseDir, $"nlog.{environment}.config");
var nlogFallback = Path.Combine(baseDir, "nlog.config");
var logger = LogManager.Setup()
    .LoadConfigurationFromFile(File.Exists(nlogConfigFile) ? nlogConfigFile : nlogFallback)
    .GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // NLog
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Database
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    // Identity
    int emailConfirmationLinkExpiryDays =
        builder.Configuration.GetValue<int?>("EmailConfirmationLinkExpiryDays") ?? 7;

    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
    {
        opt.SignIn.RequireConfirmedAccount = true;
        opt.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddTokenProvider<EmailConfirmationTokenProvider<ApplicationUser>>("emailconfirmation");

    builder.Services.ConfigureApplicationCookie(opt =>
    {
        opt.LoginPath = "/Identity/Account/Login";
        opt.LogoutPath = "/Identity/Account/Logout";
        opt.AccessDeniedPath = "/Identity/Account/AccessDenied";
    });

    builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
        opt.TokenLifespan = TimeSpan.FromHours(2));
    builder.Services.Configure<EmailConfirmationTokenProviderOptions>(opt =>
        opt.TokenLifespan = TimeSpan.FromDays(emailConfirmationLinkExpiryDays));

    builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomClaimsPrincipalFactory>();

    // App services
    builder.Services.AddSingleton<ILoggerManager, LoggerManager>();
    builder.Services.AddScoped<IEmailSender, NoOpEmailSender>();

    builder.Services.AddRazorPages();

    var app = builder.Build();

    // Seed database
    await DataSeeder.SeedAsync(app.Services);

    // Middleware pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseCustomExceptionMiddleware();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapRazorPages();

    app.Run();
}
catch (HostAbortedException)
{
    // Intentionally thrown by EF Core CLI tools after host introspection — not a real error
    throw;
}
catch (Exception ex)
{
    logger.Error(ex, "Application stopped due to exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}
