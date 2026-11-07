using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public record CustomerDetailDto(
    Customer Customer,
    List<CustomerAddress> Addresses,
    List<Order> Orders
);

public interface IUserService
{
    // Employees (Admin only)
    Task<List<User>> GetEmployeesAsync(string? searchTerm = null);
    Task<(bool Success, string Message, int? UserId)> CreateEmployeeAsync(string fullName, string email, string phone, string password, DateOnly? hireDate);
    Task<(bool Success, string Message)> UpdateEmployeeAsync(int userId, string fullName, string phone, DateOnly? hireDate);
    Task<(bool Success, string Message)> ToggleUserActiveAsync(int userId);
    Task<(bool Success, string Message)> DeleteEmployeeAsync(int userId);

    // Customers (Admin & Employee view, Admin manage)
    Task<List<User>> GetCustomersAsync(string? searchTerm = null);
    Task<CustomerDetailDto?> GetCustomerDetailAsync(int customerId);
    Task<(bool Success, string Message, int? CustomerId)> CreateCustomerByAdminAsync(string fullName, string email, string phone, string password);
    Task<(bool Success, string Message)> UpdateCustomerByAdminAsync(int customerId, string fullName, string phone);
    Task<(bool Success, string Message)> DeleteCustomerAsync(int customerId);

    // Customer Address Book
    Task<List<CustomerAddress>> GetCustomerAddressesAsync(int customerId);
    Task<(bool Success, string Message, int? AddressId)> AddAddressAsync(int customerId, string receiverName, string phone, string addressLine, bool isDefault);
    Task<(bool Success, string Message)> UpdateAddressAsync(int customerId, int addressId, string receiverName, string phone, string addressLine, bool isDefault);
    Task<(bool Success, string Message)> DeleteAddressAsync(int customerId, int addressId);
    Task<(bool Success, string Message)> SetDefaultAddressAsync(int customerId, int addressId);
}

