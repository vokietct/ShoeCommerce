using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public record ProductCardDto(
    int ProductId,
    string ProductCode,
    string ProductName,
    string? CategoryName,
    string? BrandName,
    string? ThumbnailUrl,
    decimal MinPrice,
    decimal MaxPrice,
    double AverageRating,
    int ReviewCount,
    int TotalStock,
    ProductStatus Status
);

public record ProductCatalogResult(
    List<ProductCardDto> Items,
    int TotalItems,
    int Page,
    int PageSize,
    int TotalPages
);

public interface IProductService
{
    Task<ProductCatalogResult> GetCatalogProductsAsync(
        int? categoryId = null,
        int? brandId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? searchTerm = null,
        string? sortBy = "newest",
        int page = 1,
        int pageSize = 12);

    Task<Product?> GetProductDetailAsync(int productId, bool includeAllVariants = false);
    Task<List<Product>> GetFeaturedProductsAsync(int count = 8);

    // Admin
    Task<(List<Product> Items, int TotalCount)> GetAdminProductsAsync(
        int? categoryId = null,
        int? brandId = null,
        ProductStatus? status = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 10);

    Task<(bool Success, string Message, int? ProductId)> CreateProductAsync(Product product);
    Task<(bool Success, string Message)> UpdateProductAsync(Product product);
    Task<(bool Success, string Message)> ToggleProductStatusAsync(int productId);
    Task<(bool Success, string Message)> DeleteProductAsync(int productId);

    // Variants
    Task<(bool Success, string Message, int? VariantId)> AddVariantAsync(ProductVariant variant);
    Task<(bool Success, string Message)> UpdateVariantAsync(ProductVariant variant);
    Task<(bool Success, string Message)> DeleteVariantAsync(int variantId);
    Task<(bool Success, string Message)> ToggleVariantStatusAsync(int variantId);
}

