using Microsoft.EntityFrameworkCore;
using ShoeCommerce.Data;
using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public class SupplierService : ISupplierService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;

    public SupplierService(IDbContextFactory<ShoeStoreDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Supplier>> GetSuppliersAsync(string? searchTerm = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Suppliers
            .AsNoTracking()
            .Include(s => s.Products)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(s => s.SupplierName.ToLower().Contains(term) || (s.Phone != null && s.Phone.Contains(term)));
        }

        return await query.OrderBy(s => s.SupplierName).ToListAsync();
    }

    public async Task<Supplier?> GetSupplierDetailAsync(int supplierId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Suppliers
            .AsNoTracking()
            .Include(s => s.Products)
                .ThenInclude(p => p.Category)
            .Include(s => s.Products)
                .ThenInclude(p => p.Brand)
            .FirstOrDefaultAsync(s => s.SupplierId == supplierId);
    }

    public async Task<(bool Success, string Message, int? SupplierId)> CreateSupplierAsync(Supplier supplier)
    {
        if (string.IsNullOrWhiteSpace(supplier.SupplierName))
            return (false, "Tên nhà cung cấp không được để trống.", null);

        await using var context = await _contextFactory.CreateDbContextAsync();
        supplier.SupplierName = supplier.SupplierName.Trim();
        supplier.CreatedAt = DateTime.Now;
        supplier.UpdatedAt = DateTime.Now;

        context.Suppliers.Add(supplier);
        await context.SaveChangesAsync();
        return (true, "Thêm nhà cung cấp thành công!", supplier.SupplierId);
    }

    public async Task<(bool Success, string Message)> UpdateSupplierAsync(Supplier supplier)
    {
        if (string.IsNullOrWhiteSpace(supplier.SupplierName))
            return (false, "Tên nhà cung cấp không được để trống.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Suppliers.FindAsync(supplier.SupplierId);
        if (existing == null)
            return (false, "Không tìm thấy nhà cung cấp.");

        existing.SupplierName = supplier.SupplierName.Trim();
        existing.Phone = supplier.Phone?.Trim();
        existing.Email = supplier.Email?.Trim();
        existing.Address = supplier.Address?.Trim();
        existing.IsCooperating = supplier.IsCooperating;
        existing.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();
        return (true, "Cập nhật nhà cung cấp thành công!");
    }

    public async Task<(bool Success, string Message)> ToggleCooperatingAsync(int supplierId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Suppliers.FindAsync(supplierId);
        if (existing == null)
            return (false, "Không tìm thấy nhà cung cấp.");

        existing.IsCooperating = !existing.IsCooperating;
        existing.UpdatedAt = DateTime.Now;
        await context.SaveChangesAsync();

        return (true, existing.IsCooperating ? "Đã mở lại hợp tác với nhà cung cấp." : "Đã chuyển sang Ngừng hợp tác.");
    }

    public async Task<(bool Success, string Message)> DeleteSupplierAsync(int supplierId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Suppliers
            .Include(s => s.Products)
            .Include(s => s.StockDocuments)
            .FirstOrDefaultAsync(s => s.SupplierId == supplierId);

        if (existing == null)
            return (false, "Không tìm thấy nhà cung cấp.");

        if (existing.Products.Any() || existing.StockDocuments.Any())
        {
            existing.IsCooperating = false;
            await context.SaveChangesAsync();
            return (false, "Nhà cung cấp đã có sản phẩm hoặc phiếu nhập kho liên quan, hệ thống đã chuyển sang 'Ngừng hợp tác' để bảo toàn dữ liệu lịch sử.");
        }

        context.Suppliers.Remove(existing);
        await context.SaveChangesAsync();
        return (true, "Xóa nhà cung cấp thành công!");
    }
}

