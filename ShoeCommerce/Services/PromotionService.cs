using Microsoft.EntityFrameworkCore;
using ShoeCommerce.Data;
using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public class PromotionService : IPromotionService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;
    private readonly AccessGuard _access;

    public PromotionService(IDbContextFactory<ShoeStoreDbContext> contextFactory, AccessGuard access)
    {
        _contextFactory = contextFactory;
        _access = access;
    }

    public async Task<List<Promotion>> GetAllPromotionsAsync(string? searchTerm = null)
    {
        await _access.AdminAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Promotions
            .AsNoTracking()
            .Include(p => p.Orders)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p => p.PromoCode.ToLower().Contains(term));
        }

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<Promotion?> GetPromotionByIdAsync(int promotionId)
    {
        await _access.AdminAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Promotions
            .AsNoTracking()
            .Include(p => p.Orders)
            .FirstOrDefaultAsync(p => p.PromotionId == promotionId);
    }

    public async Task<(bool Success, string Message, int? PromotionId)> CreatePromotionAsync(Promotion promotion)
    {
        await _access.AdminAsync();
        if (string.IsNullOrWhiteSpace(promotion.PromoCode))
            return (false, "Mã khuyến mãi không được để trống.", null);

        if (promotion.DiscountAmount <= 0)
            return (false, "Số tiền giảm phải lớn hơn 0.", null);

        if (promotion.MinOrderValue < 0)
            return (false, "Giá trị đơn hàng tối thiểu không được âm.", null);

        if (promotion.EndDate <= promotion.StartDate)
            return (false, "Ngày kết thúc phải sau ngày bắt đầu.", null);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var code = promotion.PromoCode.Trim().ToUpper();
        var exists = await context.Promotions.AnyAsync(p => p.PromoCode == code);
        if (exists)
            return (false, "Mã khuyến mãi đã tồn tại.", null);

        promotion.PromoCode = code;
        promotion.CreatedAt = DateTime.Now;
        promotion.UpdatedAt = DateTime.Now;

        context.Promotions.Add(promotion);
        await context.SaveChangesAsync();

        return (true, "Thêm mã khuyến mãi thành công!", promotion.PromotionId);
    }

    public async Task<(bool Success, string Message)> UpdatePromotionAsync(Promotion promotion)
    {
        await _access.AdminAsync();
        if (string.IsNullOrWhiteSpace(promotion.PromoCode))
            return (false, "Mã khuyến mãi không được để trống.");

        if (promotion.DiscountAmount <= 0)
            return (false, "Số tiền giảm phải lớn hơn 0.");

        if (promotion.MinOrderValue < 0)
            return (false, "Giá trị đơn hàng tối thiểu không được âm.");

        if (promotion.EndDate <= promotion.StartDate)
            return (false, "Ngày kết thúc phải sau ngày bắt đầu.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Promotions.FindAsync(promotion.PromotionId);
        if (existing == null)
            return (false, "Không tìm thấy khuyến mãi.");

        var code = promotion.PromoCode.Trim().ToUpper();
        var exists = await context.Promotions.AnyAsync(p => p.PromotionId != promotion.PromotionId && p.PromoCode == code);
        if (exists)
            return (false, "Mã khuyến mãi đã bị trùng.");

        existing.PromoCode = code;
        existing.DiscountAmount = promotion.DiscountAmount;
        existing.MinOrderValue = promotion.MinOrderValue;
        existing.StartDate = promotion.StartDate;
        existing.EndDate = promotion.EndDate;
        existing.IsActive = promotion.IsActive;
        existing.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();
        return (true, "Cập nhật khuyến mãi thành công!");
    }

    public async Task<(bool Success, string Message)> TogglePromotionStatusAsync(int promotionId)
    {
        await _access.AdminAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Promotions.FindAsync(promotionId);
        if (existing == null)
            return (false, "Không tìm thấy khuyến mãi.");

        existing.IsActive = !existing.IsActive;
        existing.UpdatedAt = DateTime.Now;
        await context.SaveChangesAsync();

        return (true, existing.IsActive ? "Đã kích hoạt mã khuyến mãi." : "Đã tạm dừng mã khuyến mãi.");
    }

    public async Task<(bool Success, string Message)> DeletePromotionAsync(int promotionId)
    {
        await _access.AdminAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Promotions
            .Include(p => p.Orders)
            .FirstOrDefaultAsync(p => p.PromotionId == promotionId);

        if (existing == null)
            return (false, "Không tìm thấy khuyến mãi.");

        // Không xóa mã đã được sử dụng trong đơn hàng; cho phép ngừng áp dụng mã
        if (existing.Orders.Any())
        {
            existing.IsActive = false;
            await context.SaveChangesAsync();
            return (false, "Mã khuyến mãi đã từng được sử dụng trong đơn hàng nên không thể xóa, hệ thống đã chuyển sang 'Ngừng áp dụng'.");
        }

        context.Promotions.Remove(existing);
        await context.SaveChangesAsync();
        return (true, "Xóa mã khuyến mãi thành công!");
    }
}

