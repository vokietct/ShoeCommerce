using Microsoft.EntityFrameworkCore;
using ShoeCommerce.Data;
using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public class AuthService : IAuthService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;
    private UserSession? _currentUser;

    public AuthService(IDbContextFactory<ShoeStoreDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public UserSession? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;
    public bool IsAdmin => _currentUser?.RoleId == 1;
    public bool IsEmployee => _currentUser?.RoleId == 2;
    public bool IsCustomer => _currentUser?.RoleId == 3;
    public bool IsAdminOrEmployee => IsAdmin || IsEmployee;

    public event Action? OnAuthStateChanged;

    public async Task<(bool Success, string Message)> CustomerLoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "Vui lòng nhập đầy đủ email và mật khẩu.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, "Email hoặc mật khẩu không chính xác.");

        if (!user.IsActive)
            return (false, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ hỗ trợ.");

        if (user.RoleId != 3) // Khách hàng
            return (false, "Tài khoản này là tài khoản nội bộ, vui lòng đăng nhập tại trang quản trị.");

        _currentUser = new UserSession(user.UserId, user.Email, user.FullName, user.Phone, user.RoleId, user.Role?.RoleName ?? "Customer");
        OnAuthStateChanged?.Invoke();
        return (true, "Đăng nhập thành công!");
    }

    public async Task<(bool Success, string Message)> AdminLoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "Vui lòng nhập đầy đủ email và mật khẩu.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, "Email hoặc mật khẩu quản trị không chính xác.");

        if (!user.IsActive)
            return (false, "Tài khoản quản trị đã bị khóa.");

        if (user.RoleId != 1 && user.RoleId != 2) // Phải là Quản lý hoặc Nhân viên
            return (false, "Tài khoản không có quyền truy cập trang quản trị.");

        _currentUser = new UserSession(user.UserId, user.Email, user.FullName, user.Phone, user.RoleId, user.Role?.RoleName ?? "Admin");
        OnAuthStateChanged?.Invoke();
        return (true, "Đăng nhập quản trị thành công!");
    }

    public async Task<(bool Success, string Message)> RegisterCustomerAsync(string fullName, string email, string phone, string password)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Vui lòng điền đầy đủ tất cả các trường.");
        }

        if (password.Length < 6)
            return (false, "Mật khẩu phải có ít nhất 6 ký tự.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var emailExists = await context.Users.AnyAsync(u => u.Email.ToLower() == email.Trim().ToLower());
        if (emailExists)
            return (false, "Email này đã được sử dụng bởi một tài khoản khác.");

        var newUser = new User
        {
            Email = email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = fullName.Trim(),
            Phone = phone.Trim(),
            RoleId = 3, // Customer
            IsActive = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        context.Users.Add(newUser);
        await context.SaveChangesAsync();

        var customer = new Customer
        {
            CustomerId = newUser.UserId
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Tự động tạo giỏ hàng khi đăng ký khách thành công
        context.Carts.Add(new Cart
        {
            CustomerId = customer.CustomerId,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });
        await context.SaveChangesAsync();

        // Tự động đăng nhập
        _currentUser = new UserSession(newUser.UserId, newUser.Email, newUser.FullName, newUser.Phone, newUser.RoleId, "Customer");
        OnAuthStateChanged?.Invoke();

        return (true, "Đăng ký tài khoản thành công!");
    }

    public Task LogoutAsync()
    {
        _currentUser = null;
        OnAuthStateChanged?.Invoke();
        return Task.CompletedTask;
    }

    public async Task<(bool Success, string Message)> UpdateProfileAsync(string fullName, string phone)
    {
        if (_currentUser == null)
            return (false, "Bạn chưa đăng nhập.");

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone))
            return (false, "Vui lòng nhập họ tên và số điện thoại.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.Users.FindAsync(_currentUser.UserId);
        if (user == null)
            return (false, "Không tìm thấy người dùng.");

        user.FullName = fullName.Trim();
        user.Phone = phone.Trim();
        user.UpdatedAt = DateTime.Now;
        await context.SaveChangesAsync();

        _currentUser = _currentUser with { FullName = user.FullName, Phone = user.Phone };
        OnAuthStateChanged?.Invoke();
        return (true, "Cập nhật thông tin thành công!");
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(string oldPassword, string newPassword)
    {
        if (_currentUser == null)
            return (false, "Bạn chưa đăng nhập.");

        if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            return (false, "Vui lòng nhập mật khẩu hiện tại và mật khẩu mới.");

        if (newPassword.Length < 6)
            return (false, "Mật khẩu mới phải có ít nhất 6 ký tự.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.Users.FindAsync(_currentUser.UserId);
        if (user == null)
            return (false, "Không tìm thấy người dùng.");

        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            return (false, "Mật khẩu hiện tại không chính xác.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.Now;
        await context.SaveChangesAsync();

        return (true, "Đổi mật khẩu thành công!");
    }
}

