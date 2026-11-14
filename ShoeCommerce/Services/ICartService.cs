using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public record CartItemDto(
    int CartItemId,
    int VariantId,
    int ProductId,
    string ProductName,
    string Sku,
    string Size,
    string Color,
    string? ThumbnailUrl,
    decimal UnitPrice,
    int Quantity,
    int StockQuantity,
    decimal LineTotal
);

public record CartSummaryDto(
    int CartId,
    List<CartItemDto> Items,
    int TotalQuantity,
    decimal TotalAmount
);

public interface ICartService
{
    event Action? OnCartChanged;
    Task<CartSummaryDto> GetCartAsync(int customerId);
    Task<(bool Success, string Message)> AddToCartAsync(int customerId, int variantId, int quantity);
    Task<(bool Success, string Message)> UpdateQuantityAsync(int customerId, int cartItemId, int quantity);
    Task<(bool Success, string Message)> RemoveItemAsync(int customerId, int cartItemId);
    Task<(bool Success, string Message)> ClearCartAsync(int customerId);
    Task<int> GetItemCountAsync(int customerId);
}

