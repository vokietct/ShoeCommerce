using System.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ShoeCommerce.Data;
using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public class UserService : IUserService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;
    private readonly AccessGuard _access;

    public UserService(IDbContextFactory<ShoeStoreDbContext> contextFactory, AccessGuard access)
    {
        _contextFactory = contextFactory;
        _access = access;
    }

    public async Task<List<User>> GetEmployeesAsync(string? searchTerm = null)
    {
        await _access.AdminAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.Employee)
            .Where(u => u.RoleId == 2) // Nhân viên
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(term) || u.Email.ToLower().Contains(term));
        }

        return await query.OrderBy(u => u.FullName).ToListAsync();
    }

    public async Task<(bool Success, string Message, int? UserId)> CreateEmployeeAsync(
        string fullName,
        string email,
        string phone,
        string password,
        DateOnly? hireDate)
    {
        await _access.AdminAsync();
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(password))
            return (false, "Vui lòng điền đầy đủ họ tên, email, số điện thoại và mật khẩu.", null);

        if (password.Length < 6 || password.Length > 72 || !new EmailAddressAttribute().IsValid(email.Trim()))
            return (false, "Email không hợp lệ hoặc mật khẩu không đủ 6–72 ký tự.", null);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var exists = await context.Users.AnyAsync(u => u.Email.ToLower() == email.Trim().ToLower());
        if (exists)
            return (false, "Email này đã tồn tại trong hệ thống.", null);

        await using var transaction = await context.Database.BeginTransactionAsync();
        var user = new User
        {
            FullName = fullName.Trim(),
            Email = email.Trim().ToLower(),
            Phone = phone.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            RoleId = 2, // Employee
            IsActive = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        context.Employees.Add(new Employee
        {
            EmployeeId = user.UserId,
            HireDate = hireDate ?? DateOnly.FromDateTime(DateTime.Today)
        });

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return (true, "Tạo tài khoản nhân viên thành công!", user.UserId);
    }

    public async Task<(bool Success, string Message)> UpdateEmployeeAsync(int userId, string fullName, string phone, DateOnly? hireDate)
    {
        await _access.AdminAsync();
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone))
            return (false, "Họ tên và số điện thoại không được để trống.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.UserId == userId && u.RoleId == 2);

        if (user == null)
            return (false, "Không tìm thấy nhân viên.");

        user.FullName = fullName.Trim();
        user.Phone = phone.Trim();
        user.UpdatedAt = DateTime.Now;

        if (user.Employee != null)
        {
            user.Employee.HireDate = hireDate;
        }

        await context.SaveChangesAsync();
        return (true, "Cập nhật thông tin nhân viên thành công!");
    }

    public async Task<(bool Success, string Message)> ToggleUserActiveAsync(int userId)
    {
        await _access.AdminAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.Users.FindAsync(userId);
        if (user == null)
            return (false, "Không tìm thấy người dùng.");

        if (user.RoleId == 1) return (false, "Không thể khóa tài khoản quản lý tại chức năng này.");
        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.Now;
        await context.SaveChangesAsync();

        var statusText = user.IsActive ? "Mở khóa" : "Khóa";
        return (true, $"Đã {statusText} tài khoản thành công!");
    }

    public async Task<(bool Success, string Message)> DeleteEmployeeAsync(int userId)
    {
        await _access.AdminAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.UserId == userId && u.RoleId == 2);

        if (user == null)
            return (false, "Không tìm thấy nhân viên.");

        // Kiểm tra xem nhân viên đã xử lý đơn hàng hoặc phiếu kho chưa
        var hasHandledOrders = await context.OrderStatusHistories.AnyAsync(h => h.ChangedByUserId == userId);
        var hasHandledPayments = await context.Payments.AnyAsync(p => p.ConfirmedByUserId == userId);
        var hasHandledStock = await context.StockDocuments.AnyAsync(s => s.CreatedByUserId == userId || s.ConfirmedByUserId == userId);

        if (hasHandledOrders || hasHandledPayments || hasHandledStock)
        {
            user.IsActive = false;
            await context.SaveChangesAsync();
            return (false, "Nhân viên đã từng xử lý đơn hàng, thanh toán hoặc giao dịch kho. Không thể xóa để bảo toàn dữ liệu lịch sử; hệ thống đã khóa tài khoản này.");
        }

        if (user.Employee != null) context.Employees.Remove(user.Employee);
        context.Users.Remove(user);
        await context.SaveChangesAsync();

        return (true, "Xóa nhân viên thành công!");
    }

    public async Task<List<User>> GetCustomersAsync(string? searchTerm = null)
    {
        await _access.StaffAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Users
            .AsNoTracking()
            .Include(u => u.Customer)
                .ThenInclude(c => c!.Orders)
            .Where(u => u.RoleId == 3) // Khách hàng
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(term) ||
                                     u.Email.ToLower().Contains(term) ||
                                     u.Phone.Contains(term));
        }

        return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
    }

    public async Task<CustomerDetailDto?> GetCustomerDetailAsync(int customerId)
    {
        await _access.StaffAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        var customer = await context.Customers
            .AsNoTracking()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (customer == null) return null;

        var addresses = await context.CustomerAddresses
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();

        var orders = await context.Orders
            .AsNoTracking()
            .Include(o => o.Payment)
            .Include(o => o.OrderDetails)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return new CustomerDetailDto(customer, addresses, orders);
    }

    public async Task<(bool Success, string Message, int? CustomerId)> CreateCustomerByAdminAsync(
        string fullName,
        string email,
        string phone,
        string password)
    {
        await _access.AdminAsync();
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(password))
            return (false, "Vui lòng nhập đầy đủ thông tin.", null);

        if (password.Length < 6 || password.Length > 72 || !new EmailAddressAttribute().IsValid(email.Trim()))
            return (false, "Email không hợp lệ hoặc mật khẩu không đủ 6–72 ký tự.", null);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var exists = await context.Users.AnyAsync(u => u.Email.ToLower() == email.Trim().ToLower());
        if (exists)
            return (false, "Email này đã tồn tại trong hệ thống.", null);

        await using var transaction = await context.Database.BeginTransactionAsync();
        var user = new User
        {
            FullName = fullName.Trim(),
            Email = email.Trim().ToLower(),
            Phone = phone.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            RoleId = 3, // Customer
            IsActive = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var customer = new Customer { CustomerId = user.UserId };
        context.Customers.Add(customer);

        context.Carts.Add(new Cart
        {
            CustomerId = customer.CustomerId,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return (true, "Thêm khách hàng thành công!", customer.CustomerId);
    }

    public async Task<(bool Success, string Message)> UpdateCustomerByAdminAsync(int customerId, string fullName, string phone)
    {
        await _access.AdminAsync();
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone))
            return (false, "Họ tên và số điện thoại không được để trống.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.Users.FindAsync(customerId);
        if (user == null || user.RoleId != 3)
            return (false, "Không tìm thấy khách hàng.");

        user.FullName = fullName.Trim();
        user.Phone = phone.Trim();
        user.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();
        return (true, "Cập nhật thông tin khách hàng thành công!");
    }

    public async Task<(bool Success, string Message)> DeleteCustomerAsync(int customerId)
    {
        await _access.AdminAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        var customer = await context.Customers
            .Include(c => c.User)
            .Include(c => c.Cart)
                .ThenInclude(cart => cart!.Items)
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (customer == null)
            return (false, "Không tìm thấy khách hàng.");

        // Không xóa khách hàng đã có đơn hàng
        var hasOrders = await context.Orders.AnyAsync(o => o.CustomerId == customerId);
        if (hasOrders)
        {
            if (customer.User != null)
            {
                customer.User.IsActive = false;
                await context.SaveChangesAsync();
            }
            return (false, "Khách hàng đã có lịch sử đơn hàng, không thể xóa để bảo toàn dữ liệu; hệ thống đã khóa tài khoản này.");
        }

        // Xóa giỏ hàng & địa chỉ trước
        if (customer.Cart != null)
        {
            if (customer.Cart.Items.Any()) context.CartItems.RemoveRange(customer.Cart.Items);
            context.Carts.Remove(customer.Cart);
        }

        if (customer.Addresses.Any())
        {
            context.CustomerAddresses.RemoveRange(customer.Addresses);
        }

        context.Customers.Remove(customer);
        if (customer.User != null) context.Users.Remove(customer.User);

        await context.SaveChangesAsync();
        return (true, "Xóa khách hàng thành công!");
    }

    public async Task<List<CustomerAddress>> GetCustomerAddressesAsync(int customerId)
    {
        await _access.CustomerAsync(customerId, allowStaff: true);
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.CustomerAddresses
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, int? AddressId)> AddAddressAsync(
        int customerId,
        string receiverName,
        string phone,
        string addressLine,
        bool isDefault)
    {
        await _access.CustomerAsync(customerId, allowAdmin: true);
        if (string.IsNullOrWhiteSpace(receiverName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(addressLine))
            return (false, "Vui lòng điền đầy đủ người nhận, số điện thoại và địa chỉ.", null);

        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await DatabaseLocks.CustomerAsync(context, customerId);

        // Nếu đây là địa chỉ đầu tiên của khách, tự động đặt làm mặc định
        var count = await context.CustomerAddresses.CountAsync(a => a.CustomerId == customerId);
        if (count == 0) isDefault = true;

        if (isDefault)
        {
            var existingDefaults = await context.CustomerAddresses.Where(a => a.CustomerId == customerId && a.IsDefault).ToListAsync();
            foreach (var d in existingDefaults) d.IsDefault = false;
            await context.SaveChangesAsync();
        }

        var addr = new CustomerAddress
        {
            CustomerId = customerId,
            ReceiverName = receiverName.Trim(),
            Phone = phone.Trim(),
            AddressLine = addressLine.Trim(),
            IsDefault = isDefault,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        context.CustomerAddresses.Add(addr);
        await context.SaveChangesAsync();

        await transaction.CommitAsync();
        return (true, "Thêm địa chỉ thành công!", addr.AddressId);
    }

    public async Task<(bool Success, string Message)> UpdateAddressAsync(
        int customerId,
        int addressId,
        string receiverName,
        string phone,
        string addressLine,
        bool isDefault)
    {
        await _access.CustomerAsync(customerId, allowAdmin: true);
        if (string.IsNullOrWhiteSpace(receiverName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(addressLine))
            return (false, "Vui lòng điền đầy đủ người nhận, số điện thoại và địa chỉ.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await DatabaseLocks.CustomerAsync(context, customerId);
        var addr = await context.CustomerAddresses.FirstOrDefaultAsync(a => a.AddressId == addressId && a.CustomerId == customerId);
        if (addr == null)
            return (false, "Không tìm thấy địa chỉ.");

        if (isDefault)
        {
            var otherDefaults = await context.CustomerAddresses.Where(a => a.CustomerId == customerId && a.AddressId != addressId && a.IsDefault).ToListAsync();
            foreach (var d in otherDefaults) d.IsDefault = false;
            await context.SaveChangesAsync();
        }

        addr.ReceiverName = receiverName.Trim();
        addr.Phone = phone.Trim();
        addr.AddressLine = addressLine.Trim();
        addr.IsDefault = isDefault || addr.IsDefault;
        addr.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return (true, "Cập nhật địa chỉ thành công!");
    }

    public async Task<(bool Success, string Message)> DeleteAddressAsync(int customerId, int addressId)
    {
        await _access.CustomerAsync(customerId, allowAdmin: true);
        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await DatabaseLocks.CustomerAsync(context, customerId);
        var addr = await context.CustomerAddresses.FirstOrDefaultAsync(a => a.AddressId == addressId && a.CustomerId == customerId);
        if (addr == null)
            return (false, "Không tìm thấy địa chỉ.");

        var wasDefault = addr.IsDefault;
        context.CustomerAddresses.Remove(addr);
        await context.SaveChangesAsync();

        // Nếu vừa xóa địa chỉ mặc định, set địa chỉ còn lại làm mặc định nếu có
        if (wasDefault)
        {
            var nextAddr = await context.CustomerAddresses.FirstOrDefaultAsync(a => a.CustomerId == customerId);
            if (nextAddr != null)
            {
                nextAddr.IsDefault = true;
                await context.SaveChangesAsync();
            }
        }

        await transaction.CommitAsync();
        return (true, "Xóa địa chỉ thành công!");
    }

    public async Task<(bool Success, string Message)> SetDefaultAddressAsync(int customerId, int addressId)
    {
        await _access.CustomerAsync(customerId, allowAdmin: true);
        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await DatabaseLocks.CustomerAsync(context, customerId);
        var addresses = await context.CustomerAddresses.Where(a => a.CustomerId == customerId).ToListAsync();
        var target = addresses.FirstOrDefault(a => a.AddressId == addressId);
        if (target == null)
            return (false, "Không tìm thấy địa chỉ.");

        foreach (var a in addresses) a.IsDefault = false;
        await context.SaveChangesAsync();
        target.IsDefault = true;
        target.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return (true, "Đã đặt làm địa chỉ giao hàng mặc định!");
    }
}

