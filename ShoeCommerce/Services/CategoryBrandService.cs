using Microsoft.EntityFrameworkCore;
using ShoeCommerce.Data;
using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public class CategoryBrandService : ICategoryBrandService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;

    public CategoryBrandService(IDbContextFactory<ShoeStoreDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Categories
            .AsNoTracking()
            .Include(c => c.Products)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
    }

    public async Task<List<Brand>> GetAllBrandsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Brands
            .AsNoTracking()
            .Include(b => b.Products)
            .OrderBy(b => b.BrandName)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, int? CategoryId)> CreateCategoryAsync(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Tên danh mục không được để trống.", null);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var exists = await context.Categories.AnyAsync(c => c.CategoryName.ToLower() == name.Trim().ToLower());
        if (exists)
            return (false, "Tên danh mục đã tồn tại.", null);

        var cat = new Category
        {
            CategoryName = name.Trim(),
            Description = description?.Trim()
        };

        context.Categories.Add(cat);
        await context.SaveChangesAsync();
        return (true, "Thêm danh mục thành công!", cat.CategoryId);
    }

    public async Task<(bool Success, string Message)> UpdateCategoryAsync(int id, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Tên danh mục không được để trống.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var cat = await context.Categories.FindAsync(id);
        if (cat == null)
            return (false, "Không tìm thấy danh mục.");

        var exists = await context.Categories.AnyAsync(c => c.CategoryId != id && c.CategoryName.ToLower() == name.Trim().ToLower());
        if (exists)
            return (false, "Tên danh mục đã tồn tại.");

        cat.CategoryName = name.Trim();
        cat.Description = description?.Trim();
        await context.SaveChangesAsync();

        return (true, "Cập nhật danh mục thành công!");
    }

    public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var cat = await context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.CategoryId == id);
        if (cat == null)
            return (false, "Không tìm thấy danh mục.");

        // Không xóa danh mục đang được sử dụng cho sản phẩm
        if (cat.Products.Any())
            return (false, $"Không thể xóa danh mục '{cat.CategoryName}' vì đang có {cat.Products.Count} sản phẩm thuộc danh mục này.");

        context.Categories.Remove(cat);
        await context.SaveChangesAsync();
        return (true, "Xóa danh mục thành công!");
    }

    public async Task<(bool Success, string Message, int? BrandId)> CreateBrandAsync(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Tên thương hiệu không được để trống.", null);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var exists = await context.Brands.AnyAsync(b => b.BrandName.ToLower() == name.Trim().ToLower());
        if (exists)
            return (false, "Tên thương hiệu đã tồn tại.", null);

        var brand = new Brand
        {
            BrandName = name.Trim(),
            Description = description?.Trim()
        };

        context.Brands.Add(brand);
        await context.SaveChangesAsync();
        return (true, "Thêm thương hiệu thành công!", brand.BrandId);
    }

    public async Task<(bool Success, string Message)> UpdateBrandAsync(int id, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Tên thương hiệu không được để trống.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var brand = await context.Brands.FindAsync(id);
        if (brand == null)
            return (false, "Không tìm thấy thương hiệu.");

        var exists = await context.Brands.AnyAsync(b => b.BrandId != id && b.BrandName.ToLower() == name.Trim().ToLower());
        if (exists)
            return (false, "Tên thương hiệu đã tồn tại.");

        brand.BrandName = name.Trim();
        brand.Description = description?.Trim();
        await context.SaveChangesAsync();

        return (true, "Cập nhật thương hiệu thành công!");
    }

    public async Task<(bool Success, string Message)> DeleteBrandAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var brand = await context.Brands.Include(b => b.Products).FirstOrDefaultAsync(b => b.BrandId == id);
        if (brand == null)
            return (false, "Không tìm thấy thương hiệu.");

        // Không xóa thương hiệu đang được sử dụng cho sản phẩm
        if (brand.Products.Any())
            return (false, $"Không thể xóa thương hiệu '{brand.BrandName}' vì đang có {brand.Products.Count} sản phẩm thuộc thương hiệu này.");

        context.Brands.Remove(brand);
        await context.SaveChangesAsync();
        return (true, "Xóa thương hiệu thành công!");
    }
}

