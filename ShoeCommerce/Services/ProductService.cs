using Microsoft.EntityFrameworkCore;
using ShoeCommerce.Data;
using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public class ProductService : IProductService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;
    private readonly AccessGuard _access;

    public ProductService(IDbContextFactory<ShoeStoreDbContext> contextFactory, AccessGuard access)
    {
        _contextFactory = contextFactory;
        _access = access;
    }

    public async Task<ProductCatalogResult> GetCatalogProductsAsync(
        int? categoryId = null,
        int? brandId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? searchTerm = null,
        string? sortBy = "newest",
        int page = 1,
        int pageSize = 12)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Variants)
            .Include(p => p.Reviews)
            .Where(p => p.Status == ProductStatus.Selling && p.Variants.Any(v => v.Status == ProductStatus.Selling));

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (brandId.HasValue && brandId.Value > 0)
        {
            query = query.Where(p => p.BrandId == brandId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p => p.ProductName.ToLower().Contains(term) || p.ProductCode.ToLower().Contains(term));
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        if (minPrice.HasValue || maxPrice.HasValue)
            query = query.Where(p => p.Variants.Any(v => v.Status == ProductStatus.Selling &&
                (!minPrice.HasValue || v.Price >= minPrice.Value) &&
                (!maxPrice.HasValue || v.Price <= maxPrice.Value)));
        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        query = sortBy switch
        {
            "price_asc" => query.OrderBy(p => p.Variants.Where(v => v.Status == ProductStatus.Selling).Min(v => v.Price)).ThenBy(p => p.ProductId),
            "price_desc" => query.OrderByDescending(p => p.Variants.Where(v => v.Status == ProductStatus.Selling).Min(v => v.Price)).ThenBy(p => p.ProductId),
            "rating" => query.OrderByDescending(p => p.Reviews.Select(r => (double?)r.Rating).Average() ?? 0).ThenBy(p => p.ProductId),
            _ => query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.ProductId)
        };
        var rawList = await query.Skip((page - 1) * pageSize).Take(pageSize).AsSplitQuery().ToListAsync();

        var dtoList = rawList.Select(p =>
        {
            var sellingVariants = p.Variants.Where(v => v.Status == ProductStatus.Selling).ToList();
            var minP = sellingVariants.Any() ? sellingVariants.Min(v => v.Price) : 0;
            var maxP = sellingVariants.Any() ? sellingVariants.Max(v => v.Price) : 0;
            var avgR = p.Reviews.Any() ? p.Reviews.Average(r => (double)r.Rating) : 0.0;
            var totalStock = sellingVariants.Sum(v => v.StockQuantity);

            return new ProductCardDto(
                p.ProductId,
                p.ProductCode,
                p.ProductName,
                p.Category?.CategoryName,
                p.Brand?.BrandName,
                p.ThumbnailUrl,
                minP,
                maxP,
                avgR,
                p.Reviews.Count,
                totalStock,
                p.Status
            );
        }).ToList();

        return new ProductCatalogResult(dtoList, totalItems, page, pageSize, totalPages);
    }

    public async Task<Product?> GetProductDetailAsync(int productId, bool includeAllVariants = false)
    {
        if (includeAllVariants) await _access.StaffAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Supplier)
            .Include(p => p.Variants)
            .Include(p => p.Reviews)
                .ThenInclude(r => r.Customer)
                    .ThenInclude(c => c!.User)
            .Where(p => p.ProductId == productId);

        var product = await query.FirstOrDefaultAsync();
        if (product == null || (!includeAllVariants && product.Status != ProductStatus.Selling)) return null;

        if (!includeAllVariants)
        {
            product.Variants = product.Variants
                .Where(v => v.Status == ProductStatus.Selling)
                .OrderBy(v => v.Size)
                .ThenBy(v => v.Color)
                .ToList();
        }
        else
        {
            product.Variants = product.Variants
                .OrderBy(v => v.Size)
                .ThenBy(v => v.Color)
                .ToList();
        }

        return product;
    }

    public async Task<List<Product>> GetFeaturedProductsAsync(int count = 8)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Variants)
            .Include(p => p.Reviews)
            .Where(p => p.Status == ProductStatus.Selling && p.Variants.Any(v => v.Status == ProductStatus.Selling))
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<(List<Product> Items, int TotalCount)> GetAdminProductsAsync(
        int? categoryId = null,
        int? brandId = null,
        ProductStatus? status = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 10)
    {
        await _access.StaffAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Supplier)
            .Include(p => p.Variants)
            .AsQueryable();

        if (categoryId.HasValue && categoryId.Value > 0)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (brandId.HasValue && brandId.Value > 0)
            query = query.Where(p => p.BrandId == brandId.Value);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p => p.ProductName.ToLower().Contains(term) || p.ProductCode.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.ProductId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(bool Success, string Message, int? ProductId)> CreateProductAsync(Product product)
    {
        await _access.AdminAsync();
        if (string.IsNullOrWhiteSpace(product.ProductCode) || string.IsNullOrWhiteSpace(product.ProductName))
            return (false, "Mã sản phẩm và tên sản phẩm không được để trống.", null);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var codeExists = await context.Products.AnyAsync(p => p.ProductCode.ToLower() == product.ProductCode.Trim().ToLower());
        if (codeExists)
            return (false, "Mã sản phẩm đã tồn tại trong hệ thống.", null);

        product.ProductCode = product.ProductCode.Trim().ToUpper();
        product.ProductName = product.ProductName.Trim();
        product.CreatedAt = DateTime.Now;
        product.UpdatedAt = DateTime.Now;

        context.Products.Add(product);
        await context.SaveChangesAsync();

        return (true, "Thêm sản phẩm thành công!", product.ProductId);
    }

    public async Task<(bool Success, string Message)> UpdateProductAsync(Product product)
    {
        await _access.AdminAsync();
        if (string.IsNullOrWhiteSpace(product.ProductCode) || string.IsNullOrWhiteSpace(product.ProductName))
            return (false, "Mã sản phẩm và tên sản phẩm không được để trống.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Products.FindAsync(product.ProductId);
        if (existing == null)
            return (false, "Không tìm thấy sản phẩm.");

        var codeExists = await context.Products.AnyAsync(p => p.ProductId != product.ProductId && p.ProductCode.ToLower() == product.ProductCode.Trim().ToLower());
        if (codeExists)
            return (false, "Mã sản phẩm đã bị trùng với sản phẩm khác.");

        existing.ProductCode = product.ProductCode.Trim().ToUpper();
        existing.ProductName = product.ProductName.Trim();
        existing.CategoryId = product.CategoryId;
        existing.BrandId = product.BrandId;
        existing.SupplierId = product.SupplierId;
        existing.Description = product.Description;
        existing.ThumbnailUrl = product.ThumbnailUrl;
        existing.Status = product.Status;
        existing.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();
        return (true, "Cập nhật sản phẩm thành công!");
    }

    public async Task<(bool Success, string Message)> ToggleProductStatusAsync(int productId)
    {
        await _access.AdminAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Products.FindAsync(productId);
        if (existing == null)
            return (false, "Không tìm thấy sản phẩm.");

        existing.Status = existing.Status == ProductStatus.Selling ? ProductStatus.Discontinued : ProductStatus.Selling;
        existing.UpdatedAt = DateTime.Now;
        await context.SaveChangesAsync();

        var msg = existing.Status == ProductStatus.Selling ? "Đã chuyển sản phẩm sang Đang kinh doanh." : "Đã ngừng kinh doanh sản phẩm.";
        return (true, msg);
    }

    public async Task<(bool Success, string Message)> DeleteProductAsync(int productId)
    {
        await _access.AdminAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (existing == null)
            return (false, "Không tìm thấy sản phẩm.");

        // Kiểm tra xem đã có đơn hàng, phiếu kho hoặc review liên quan chưa
        var variantIds = existing.Variants.Select(v => v.VariantId).ToList();
        var hasOrderDetails = await context.OrderDetails.AnyAsync(od => variantIds.Contains(od.VariantId));
        var hasStockItems = await context.StockDocumentItems.AnyAsync(sd => variantIds.Contains(sd.VariantId));
        var hasReviews = await context.ProductReviews.AnyAsync(r => r.ProductId == productId);

        if (hasOrderDetails || hasStockItems || hasReviews)
        {
            // Chỉ xóa khi chưa phát sinh dữ liệu liên quan; nếu có thì chuyển sang Ngừng kinh doanh
            existing.Status = ProductStatus.Discontinued;
            await context.SaveChangesAsync();
            return (false, "Sản phẩm đã có dữ liệu giao dịch hoặc đánh giá, hệ thống đã tự động chuyển sang trạng thái 'Ngừng kinh doanh' để bảo toàn dữ liệu lịch sử.");
        }

        // Xóa giỏ hàng chứa sản phẩm này trước nếu có
        var cartItems = await context.CartItems.Where(ci => variantIds.Contains(ci.VariantId)).ToListAsync();
        if (cartItems.Any()) context.CartItems.RemoveRange(cartItems);

        context.ProductVariants.RemoveRange(existing.Variants);
        context.Products.Remove(existing);
        await context.SaveChangesAsync();

        return (true, "Xóa sản phẩm thành công!");
    }

    public async Task<(bool Success, string Message, int? VariantId)> AddVariantAsync(ProductVariant variant)
    {
        await _access.AdminAsync();
        if (string.IsNullOrWhiteSpace(variant.Sku) || string.IsNullOrWhiteSpace(variant.Size) || string.IsNullOrWhiteSpace(variant.Color))
            return (false, "SKU, Size và Màu sắc không được để trống.", null);

        if (variant.Price < 0 || variant.LowStockThreshold < 0)
            return (false, "Giá bán không được âm.", null);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // 1. Kiểm tra SKU không trùng trên toàn hệ thống
        var skuExists = await context.ProductVariants.AnyAsync(v => v.Sku.ToLower() == variant.Sku.Trim().ToLower());
        if (skuExists)
            return (false, "Mã SKU này đã tồn tại trên hệ thống.", null);

        // 2. Không tạo trùng tổ hợp size / màu trong cùng một sản phẩm
        var comboExists = await context.ProductVariants.AnyAsync(v =>
            v.ProductId == variant.ProductId &&
            v.Size.ToLower() == variant.Size.Trim().ToLower() &&
            v.Color.ToLower() == variant.Color.Trim().ToLower());
        if (comboExists)
            return (false, "Tổ hợp Size và Màu sắc này đã tồn tại cho sản phẩm này.", null);

        variant.Sku = variant.Sku.Trim().ToUpper();
        variant.Size = variant.Size.Trim();
        variant.Color = variant.Color.Trim();
        variant.StockQuantity = 0; // Tồn kho chỉ thay đổi qua phiếu kho
        variant.CreatedAt = DateTime.Now;
        variant.UpdatedAt = DateTime.Now;

        context.ProductVariants.Add(variant);
        await context.SaveChangesAsync();

        return (true, "Thêm biến thể thành công!", variant.VariantId);
    }

    public async Task<(bool Success, string Message)> UpdateVariantAsync(ProductVariant variant)
    {
        await _access.AdminAsync();
        if (string.IsNullOrWhiteSpace(variant.Sku) || string.IsNullOrWhiteSpace(variant.Size) || string.IsNullOrWhiteSpace(variant.Color))
            return (false, "SKU, Size và Màu sắc không được để trống.");

        if (variant.Price < 0 || variant.LowStockThreshold < 0)
            return (false, "Giá bán không được âm.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.ProductVariants.FindAsync(variant.VariantId);
        if (existing == null)
            return (false, "Không tìm thấy biến thể.");

        // Kiểm tra SKU không trùng biến thể khác
        var skuExists = await context.ProductVariants.AnyAsync(v => v.VariantId != variant.VariantId && v.Sku.ToLower() == variant.Sku.Trim().ToLower());
        if (skuExists)
            return (false, "Mã SKU đã bị trùng với biến thể khác.");

        // Kiểm tra combo size/màu trong cùng sản phẩm
        var comboExists = await context.ProductVariants.AnyAsync(v =>
            v.VariantId != variant.VariantId &&
            v.ProductId == existing.ProductId &&
            v.Size.ToLower() == variant.Size.Trim().ToLower() &&
            v.Color.ToLower() == variant.Color.Trim().ToLower());
        if (comboExists)
            return (false, "Tổ hợp Size và Màu sắc này đã tồn tại cho sản phẩm này.");

        existing.Sku = variant.Sku.Trim().ToUpper();
        existing.Size = variant.Size.Trim();
        existing.Color = variant.Color.Trim();
        existing.Price = variant.Price;
        existing.LowStockThreshold = variant.LowStockThreshold;
        existing.Status = variant.Status;
        existing.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();
        return (true, "Cập nhật biến thể thành công!");
    }

    public async Task<(bool Success, string Message)> DeleteVariantAsync(int variantId)
    {
        await _access.AdminAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.ProductVariants.FindAsync(variantId);
        if (existing == null)
            return (false, "Không tìm thấy biến thể.");

        var hasOrderDetails = await context.OrderDetails.AnyAsync(od => od.VariantId == variantId);
        var hasStockItems = await context.StockDocumentItems.AnyAsync(sd => sd.VariantId == variantId);

        if (hasOrderDetails || hasStockItems)
        {
            existing.Status = ProductStatus.Discontinued;
            await context.SaveChangesAsync();
            return (false, "Biến thể đã phát sinh đơn hàng hoặc giao dịch kho, đã chuyển sang trạng thái 'Ngừng kinh doanh'.");
        }

        var cartItems = await context.CartItems.Where(ci => ci.VariantId == variantId).ToListAsync();
        if (cartItems.Any()) context.CartItems.RemoveRange(cartItems);

        context.ProductVariants.Remove(existing);
        await context.SaveChangesAsync();
        return (true, "Xóa biến thể thành công!");
    }

    public async Task<(bool Success, string Message)> ToggleVariantStatusAsync(int variantId)
    {
        await _access.AdminAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.ProductVariants.FindAsync(variantId);
        if (existing == null)
            return (false, "Không tìm thấy biến thể.");

        existing.Status = existing.Status == ProductStatus.Selling ? ProductStatus.Discontinued : ProductStatus.Selling;
        existing.UpdatedAt = DateTime.Now;
        await context.SaveChangesAsync();

        return (true, existing.Status == ProductStatus.Selling ? "Đã mở kinh doanh biến thể." : "Đã ngừng kinh doanh biến thể.");
    }
}

