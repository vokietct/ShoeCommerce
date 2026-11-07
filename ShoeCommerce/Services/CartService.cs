using Microsoft.EntityFrameworkCore;
using ShoeCommerce.Data;
using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public class CartService : ICartService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;

    public CartService(IDbContextFactory<ShoeStoreDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    private async Task<Cart> GetOrCreateCartAsync(ShoeStoreDbContext context, int customerId)
    {
        var cart = await context.Carts.FirstOrDefaultAsync(c => c.CustomerId == customerId);
        if (cart == null)
        {
            cart = new Cart
            {
                CustomerId = customerId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            context.Carts.Add(cart);
            await context.SaveChangesAsync();
        }
        return cart;
    }

    public async Task<CartSummaryDto> GetCartAsync(int customerId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var cart = await GetOrCreateCartAsync(context, customerId);

        var items = await context.CartItems
            .AsNoTracking()
            .Include(ci => ci.Variant)
                .ThenInclude(v => v!.Product)
            .Where(ci => ci.CartId == cart.CartId)
            .ToListAsync();

        var dtoList = items
            .Where(i => i.Variant != null && i.Variant.Product != null)
            .Select(i => new CartItemDto(
                i.CartItemId,
                i.VariantId,
                i.Variant!.ProductId,
                i.Variant.Product!.ProductName,
                i.Variant.Sku,
                i.Variant.Size,
                i.Variant.Color,
                i.Variant.Product.ThumbnailUrl,
                i.Variant.Price,
                i.Quantity,
                i.Variant.StockQuantity,
                i.Quantity * i.Variant.Price
            ))
            .ToList();

        var totalQty = dtoList.Sum(i => i.Quantity);
        var totalAmount = dtoList.Sum(i => i.LineTotal);

        return new CartSummaryDto(cart.CartId, dtoList, totalQty, totalAmount);
    }

    public async Task<(bool Success, string Message)> AddToCartAsync(int customerId, int variantId, int quantity)
    {
        if (quantity <= 0)
            return (false, "Số lượng thêm vào giỏ phải lớn hơn 0.");

        await using var context = await _contextFactory.CreateDbContextAsync();

        var variant = await context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.VariantId == variantId);

        if (variant == null)
            return (false, "Biến thể sản phẩm không tồn tại.");

        if (variant.Status != ProductStatus.Selling || variant.Product?.Status != ProductStatus.Selling)
            return (false, "Sản phẩm hoặc biến thể này hiện đang ngừng kinh doanh.");

        var cart = await GetOrCreateCartAsync(context, customerId);

        var existingItem = await context.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.VariantId == variantId);

        var newQty = (existingItem?.Quantity ?? 0) + quantity;

        if (newQty > variant.StockQuantity)
        {
            return (false, $"Kho chỉ còn {variant.StockQuantity} sản phẩm cho size và màu này. Bạn đã có {existingItem?.Quantity ?? 0} trong giỏ.");
        }

        if (existingItem != null)
        {
            existingItem.Quantity = newQty;
            existingItem.UpdatedAt = DateTime.Now;
        }
        else
        {
            context.CartItems.Add(new CartItem
            {
                CartId = cart.CartId,
                VariantId = variantId,
                Quantity = quantity,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }

        cart.UpdatedAt = DateTime.Now;
        await context.SaveChangesAsync();
        return (true, "Đã thêm sản phẩm vào giỏ hàng thành công!");
    }

    public async Task<(bool Success, string Message)> UpdateQuantityAsync(int customerId, int cartItemId, int quantity)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var item = await context.CartItems
            .Include(ci => ci.Cart)
            .Include(ci => ci.Variant)
            .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

        if (item == null || item.Cart?.CustomerId != customerId)
            return (false, "Không tìm thấy sản phẩm trong giỏ hàng.");

        if (quantity <= 0)
        {
            context.CartItems.Remove(item);
            await context.SaveChangesAsync();
            return (true, "Đã xóa sản phẩm khỏi giỏ hàng.");
        }

        if (item.Variant != null && quantity > item.Variant.StockQuantity)
        {
            return (false, $"Số lượng vượt quá tồn kho (hiện còn {item.Variant.StockQuantity} sản phẩm).");
        }

        item.Quantity = quantity;
        item.UpdatedAt = DateTime.Now;
        await context.SaveChangesAsync();

        return (true, "Đã cập nhật số lượng.");
    }

    public async Task<(bool Success, string Message)> RemoveItemAsync(int customerId, int cartItemId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var item = await context.CartItems
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

        if (item == null || item.Cart?.CustomerId != customerId)
            return (false, "Không tìm thấy sản phẩm trong giỏ hàng.");

        context.CartItems.Remove(item);
        await context.SaveChangesAsync();
        return (true, "Đã xóa sản phẩm khỏi giỏ hàng.");
    }

    public async Task<(bool Success, string Message)> ClearCartAsync(int customerId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var cart = await context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (cart != null && cart.Items.Any())
        {
            context.CartItems.RemoveRange(cart.Items);
            cart.UpdatedAt = DateTime.Now;
            await context.SaveChangesAsync();
        }

        return (true, "Đã làm trống giỏ hàng.");
    }

    public async Task<int> GetItemCountAsync(int customerId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var cart = await context.Carts.FirstOrDefaultAsync(c => c.CustomerId == customerId);
        if (cart == null) return 0;

        return await context.CartItems
            .Where(ci => ci.CartId == cart.CartId)
            .SumAsync(ci => ci.Quantity);
    }
}

