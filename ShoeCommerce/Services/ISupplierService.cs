using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public interface ISupplierService
{
    Task<List<Supplier>> GetSuppliersAsync(string? searchTerm = null);
    Task<Supplier?> GetSupplierDetailAsync(int supplierId);
    Task<(bool Success, string Message, int? SupplierId)> CreateSupplierAsync(Supplier supplier);
    Task<(bool Success, string Message)> UpdateSupplierAsync(Supplier supplier);
    Task<(bool Success, string Message)> ToggleCooperatingAsync(int supplierId);
    Task<(bool Success, string Message)> DeleteSupplierAsync(int supplierId);
}

