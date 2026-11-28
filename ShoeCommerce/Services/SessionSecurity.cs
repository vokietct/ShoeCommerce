using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data.Common;
using ShoeCommerce.Data;
using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public static class SessionSecurity
{
    public const string StampClaim = "shoestore:stamp";
    public const string PhoneClaim = "shoestore:phone";
    public static string Stamp(string hash) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hash)));
    public static string RoleName(int id) => id switch { 1 => "Admin", 2 => "Employee", 3 => "Customer", _ => "" };

    public static ClaimsPrincipal Principal(User user) => new(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, RoleName(user.RoleId)),
        new Claim(PhoneClaim, user.Phone),
        new Claim(StampClaim, Stamp(user.PasswordHash))
    }, CookieAuthenticationDefaults.AuthenticationScheme));

    public static UserSession? Read(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true ||
            !int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return null;
        var role = principal.FindFirstValue(ClaimTypes.Role);
        var roleId = role switch { "Admin" => 1, "Employee" => 2, "Customer" => 3, _ => 0 };
        return roleId == 0 ? null : new(id, principal.FindFirstValue(ClaimTypes.Email) ?? "",
            principal.Identity.Name ?? "", principal.FindFirstValue(PhoneClaim) ?? "", roleId, role!);
    }

    public static string LocalReturnUrl(string? url, string fallback = "/") =>
        !string.IsNullOrWhiteSpace(url) && url.StartsWith('/') && !url.StartsWith("//") &&
        !url.Contains('\\') && !url.Any(char.IsControl) ? url : fallback;

    public static async Task ValidateCookieAsync(CookieValidatePrincipalContext cookie)
    {
        var session = Read(cookie.Principal);
        try
        {
            await using var db = await cookie.HttpContext.RequestServices
                .GetRequiredService<IDbContextFactory<ShoeStoreDbContext>>().CreateDbContextAsync();
            var user = session == null ? null : await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == session.UserId);
            if (user == null || !user.IsActive || user.RoleId != session!.RoleId ||
                Stamp(user.PasswordHash) != cookie.Principal?.FindFirstValue(StampClaim))
            {
                cookie.RejectPrincipal();
                await cookie.HttpContext.SignOutAsync();
            }
            else if (user.FullName != session.FullName || user.Phone != session.Phone)
            {
                cookie.ReplacePrincipal(Principal(user));
                cookie.ShouldRenew = true;
            }
        }
        catch (Exception ex) when (ex is DbException or InvalidOperationException)
        {
            cookie.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("SessionSecurity").LogWarning(ex, "Không thể kiểm tra phiên đăng nhập do lỗi cơ sở dữ liệu.");
            cookie.RejectPrincipal();
            await cookie.HttpContext.SignOutAsync();
        }
    }

    public static void MapAccountEndpoints(this WebApplication app)
    {
        app.MapPost("/account/login", async (HttpContext http, IAntiforgery antiforgery, IAuthService auth,
            IDbContextFactory<ShoeStoreDbContext> factory) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(http);
            }
            catch (AntiforgeryValidationException ex)
            {
                var invalidForm = http.Request.HasFormContentType
                    ? await http.Request.ReadFormAsync()
                    : null;
                LogAntiforgeryValidationFailure(http, invalidForm, ex);
                var isAdminLogin = invalidForm != null && invalidForm["portal"] == "admin";
                var returnUrl = invalidForm == null ? null : invalidForm["returnUrl"].ToString();
                var invalidTarget = LocalReturnUrl(returnUrl, isAdminLogin ? "/admin" : "/");
                return Results.LocalRedirect($"{(isAdminLogin ? "/admin/login" : "/login")}?formError=invalid&returnUrl={Uri.EscapeDataString(invalidTarget)}");
            }
            var form = await http.Request.ReadFormAsync();
            var admin = form["portal"] == "admin";
            var result = admin
                ? await auth.AdminLoginAsync(form["email"].ToString(), form["password"].ToString())
                : await auth.CustomerLoginAsync(form["email"].ToString(), form["password"].ToString());
            var target = LocalReturnUrl(form["returnUrl"], admin ? "/admin" : "/");
            if (!result.Success)
                return Results.LocalRedirect($"{(admin ? "/admin/login" : "/login")}?error={Uri.EscapeDataString(result.Message)}&returnUrl={Uri.EscapeDataString(target)}");
            await SignInAsync(http, factory, auth.CurrentUser!.UserId);
            return Results.LocalRedirect(target);
        }).RequireRateLimiting("account");

        app.MapPost("/account/register", async (HttpContext http, IAntiforgery antiforgery, IAuthService auth,
            IDbContextFactory<ShoeStoreDbContext> factory) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(http);
            }
            catch (AntiforgeryValidationException ex)
            {
                var invalidForm = http.Request.HasFormContentType
                    ? await http.Request.ReadFormAsync()
                    : null;
                LogAntiforgeryValidationFailure(http, invalidForm, ex);
                var returnUrl = invalidForm == null ? null : invalidForm["returnUrl"].ToString();
                var invalidTarget = LocalReturnUrl(returnUrl);
                return Results.LocalRedirect($"/register?error={Uri.EscapeDataString("Phiên biểu mẫu đã hết hạn. Vui lòng tải lại trang và thử lại.")}&returnUrl={Uri.EscapeDataString(invalidTarget)}");
            }
            var form = await http.Request.ReadFormAsync();
            var result = form["password"] != form["confirmPassword"]
                ? (Success: false, Message: "Xác nhận mật khẩu không trùng khớp.")
                : await auth.RegisterCustomerAsync(form["fullName"].ToString(), form["email"].ToString(),
                    form["phone"].ToString(), form["password"].ToString());
            var target = LocalReturnUrl(form["returnUrl"]);
            if (!result.Success)
                return Results.LocalRedirect($"/register?error={Uri.EscapeDataString(result.Message)}&returnUrl={Uri.EscapeDataString(target)}");
            await SignInAsync(http, factory, auth.CurrentUser!.UserId);
            return Results.LocalRedirect(target);
        }).RequireRateLimiting("account");

        app.MapPost("/account/logout", async (HttpContext http, IAntiforgery antiforgery) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(http);
            }
            catch (AntiforgeryValidationException ex)
            {
                var invalidForm = http.Request.HasFormContentType
                    ? await http.Request.ReadFormAsync()
                    : null;
                LogAntiforgeryValidationFailure(http, invalidForm, ex);
                await http.SignOutAsync();
                return Results.LocalRedirect("/login?error=Phiên%20biểu%20mẫu%20đã%20hết%20hạn.%20Vui%20lòng%20đăng%20nhập%20lại.");
            }
            await http.SignOutAsync();
            return Results.LocalRedirect("/");
        });
    }

    private static void LogAntiforgeryValidationFailure(
        HttpContext http,
        IFormCollection? form,
        AntiforgeryValidationException exception)
    {
        var options = http.RequestServices.GetRequiredService<IOptions<AntiforgeryOptions>>().Value;
        var requestTokenPresent = form != null &&
            form.TryGetValue(options.FormFieldName, out var requestTokens) &&
            requestTokens.Any(value => !string.IsNullOrEmpty(value));
        var cookieName = options.Cookie.Name;
        var cookieTokenPresent = cookieName != null &&
            http.Request.Cookies.TryGetValue(cookieName, out var cookieToken) &&
            !string.IsNullOrEmpty(cookieToken);
        var message = exception.Message;
        var category = !requestTokenPresent
            ? "MissingRequestToken"
            : !cookieTokenPresent
                ? "MissingCookieToken"
                : message.Contains("could not be decrypted", StringComparison.OrdinalIgnoreCase)
                    ? "TokenUnprotectFailed"
                    : message.Contains("do not match", StringComparison.OrdinalIgnoreCase) ||
                      message.Contains("were swapped", StringComparison.OrdinalIgnoreCase)
                        ? "TokenPairMismatch"
                        : message.Contains("meant for a different", StringComparison.OrdinalIgnoreCase) ||
                          message.Contains("meant for user", StringComparison.OrdinalIgnoreCase)
                            ? "TokenIdentityMismatch"
                            : message.Contains("custom data check", StringComparison.OrdinalIgnoreCase)
                                ? "TokenAdditionalDataMismatch"
                                : "TokenValidationFailed";

        http.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("SessionSecurity")
            .LogWarning(
                "Antiforgery validation failed on {Path}. Category={Category}; RequestTokenPresent={RequestTokenPresent}; CookieTokenPresent={CookieTokenPresent}; ExceptionType={ExceptionType}.",
                http.Request.Path,
                category,
                requestTokenPresent,
                cookieTokenPresent,
                exception.GetType().Name);
    }

    private static async Task SignInAsync(HttpContext http, IDbContextFactory<ShoeStoreDbContext> factory, int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var user = await db.Users.AsNoTracking().SingleAsync(u => u.UserId == id && u.IsActive);
        await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, Principal(user),
            new AuthenticationProperties { IsPersistent = false });
    }
}
