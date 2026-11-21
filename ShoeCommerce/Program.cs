using Microsoft.EntityFrameworkCore;
using ShoeCommerce.Components;
using ShoeCommerce.Data;
using ShoeCommerce.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables().AddCommandLine(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
var dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName("ShoeCommerce")
    .PersistKeysToFileSystem(new DirectoryInfo(
        Path.Combine(builder.Environment.ContentRootPath, "data-protection-keys")));
if (OperatingSystem.IsWindows())
{
    dataProtection.ProtectKeysWithDpapi();
}
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = ".ShoeStore.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.Cookie.Name = "ShoeStore.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Events.OnValidatePrincipal = SessionSecurity.ValidateCookieAsync;
        options.Events.OnRedirectToLogin = ctx =>
        {
            var login = ctx.Request.Path.StartsWithSegments("/admin") ? "/admin/login" : "/login";
            ctx.Response.Redirect($"{login}?returnUrl={Uri.EscapeDataString(ctx.Request.Path + ctx.Request.QueryString)}");
            return Task.CompletedTask;
        };
    });
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("account", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local", _ => new FixedWindowRateLimiterOptions
        { PermitLimit = 15, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

// Configure MySQL Database with Pomelo EF Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Chưa cấu hình ConnectionStrings:DefaultConnection.");

builder.Services.AddDbContextFactory<ShoeStoreDbContext>(options =>
{
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 16)));
}, ServiceLifetime.Scoped);

// Register Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AccessGuard>();
builder.Services.Configure<ShoeCommerce.Models.BankTransferOptions>(builder.Configuration.GetSection("BankTransfer"));
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryBrandService, CategoryBrandService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();

var app = builder.Build();

// Seed demo data only in Development and only when explicitly enabled.
if (app.Environment.IsDevelopment() &&
    app.Configuration.GetValue<bool>("Database:SeedDemoData"))
{
    try
    {
        using var scope = app.Services.CreateScope();
        await using var context = await scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<ShoeStoreDbContext>>()
            .CreateDbContextAsync();
        await DbSeeder.SeedAsync(context);
    }
    catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException or System.Data.Common.DbException)
    {
        app.Logger.LogError(ex, "Không thể chuẩn bị dữ liệu demo. Kiểm tra MySQL và cấu hình ConnectionStrings:DefaultConnection.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseAntiforgery();

app.MapAccountEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
