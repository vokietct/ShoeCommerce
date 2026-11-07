using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public record UserSession(int UserId, string Email, string FullName, string Phone, int RoleId, string RoleName);

public interface IAuthService
{
    UserSession? CurrentUser { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsEmployee { get; }
    bool IsCustomer { get; }
    bool IsAdminOrEmployee { get; }

    event Action? OnAuthStateChanged;

    Task<(bool Success, string Message)> CustomerLoginAsync(string email, string password);
    Task<(bool Success, string Message)> AdminLoginAsync(string email, string password);
    Task<(bool Success, string Message)> RegisterCustomerAsync(string fullName, string email, string phone, string password);
    Task LogoutAsync();
    Task<(bool Success, string Message)> UpdateProfileAsync(string fullName, string phone);
    Task<(bool Success, string Message)> ChangePasswordAsync(string oldPassword, string newPassword);
}

