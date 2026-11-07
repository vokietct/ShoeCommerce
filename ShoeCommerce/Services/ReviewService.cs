using Microsoft.EntityFrameworkCore;
using ShoeCommerce.Data;
using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public class ReviewService : IReviewService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;

    public ReviewService(IDbContextFactory<ShoeStoreDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<bool> CanCustomerReviewProductAsync(int customerId, int productId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // 1. Khách hàng đã đánh giá sản phẩm này chưa? Mỗi khách tối đa 1 đánh giá
        var alreadyReviewed = await context.ProductReviews
            .AnyAsync(r => r.CustomerId == customerId && r.ProductId == productId);
        if (alreadyReviewed) return false;

        // 2. Khách hàng đã mua sản phẩm này trong ít nhất một đơn Hoàn tất chưa?
        var productVariantIds = await context.ProductVariants
            .Where(v => v.ProductId == productId)
            .Select(v => v.VariantId)
            .ToListAsync();

        var hasPurchasedInCompletedOrder = await context.Orders
            .Where(o => o.CustomerId == customerId && o.OrderStatus == OrderStatus.Completed)
            .AnyAsync(o => o.OrderDetails.Any(od => productVariantIds.Contains(od.VariantId)));

        return hasPurchasedInCompletedOrder;
    }

    public async Task<List<ProductReview>> GetProductReviewsAsync(int productId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ProductReviews
            .AsNoTracking()
            .Include(r => r.Customer)
                .ThenInclude(c => c!.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ReviewViewDto>> GetCustomerReviewsAsync(int customerId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var reviews = await context.ProductReviews
            .AsNoTracking()
            .Include(r => r.Product)
            .Include(r => r.Customer)
                .ThenInclude(c => c!.User)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(r => new ReviewViewDto(
            r.ReviewId,
            r.ProductId,
            r.Product?.ProductName ?? "",
            r.Product?.ThumbnailUrl,
            r.CustomerId,
            r.Customer?.User?.FullName ?? "",
            r.Rating,
            r.Comment,
            r.CreatedAt,
            r.UpdatedAt
        )).ToList();
    }

    public async Task<(List<ReviewViewDto> Items, int TotalCount)> GetAdminReviewsAsync(
        int? productId = null,
        byte? rating = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 10)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.ProductReviews
            .AsNoTracking()
            .Include(r => r.Product)
            .Include(r => r.Customer)
                .ThenInclude(c => c!.User)
            .AsQueryable();

        if (productId.HasValue && productId.Value > 0)
            query = query.Where(r => r.ProductId == productId.Value);

        if (rating.HasValue && rating.Value > 0)
            query = query.Where(r => r.Rating == rating.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(r => r.Comment.ToLower().Contains(term) ||
                                     (r.Product != null && r.Product.ProductName.ToLower().Contains(term)) ||
                                     (r.Customer != null && r.Customer.User != null && r.Customer.User.FullName.ToLower().Contains(term)));
        }

        var total = await query.CountAsync();
        var paged = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtoList = paged.Select(r => new ReviewViewDto(
            r.ReviewId,
            r.ProductId,
            r.Product?.ProductName ?? "",
            r.Product?.ThumbnailUrl,
            r.CustomerId,
            r.Customer?.User?.FullName ?? "",
            r.Rating,
            r.Comment,
            r.CreatedAt,
            r.UpdatedAt
        )).ToList();

        return (dtoList, total);
    }

    public async Task<(bool Success, string Message)> AddReviewAsync(int customerId, int productId, byte rating, string comment)
    {
        if (rating < 1 || rating > 5)
            return (false, "Số sao đánh giá phải từ 1 đến 5.");

        if (string.IsNullOrWhiteSpace(comment))
            return (false, "Vui lòng nhập nội dung nhận xét đánh giá.");

        var canReview = await CanCustomerReviewProductAsync(customerId, productId);
        if (!canReview)
            return (false, "Chỉ khách hàng đã từng mua sản phẩm này trong đơn hàng đã hoàn tất mới được quyền đánh giá.");

        await using var context = await _contextFactory.CreateDbContextAsync();

        var review = new ProductReview
        {
            ProductId = productId,
            CustomerId = customerId,
            Rating = rating,
            Comment = comment.Trim(),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        context.ProductReviews.Add(review);
        await context.SaveChangesAsync();

        return (true, "Đánh giá sản phẩm thành công! Cảm ơn bạn đã đóng góp ý kiến.");
    }

    public async Task<(bool Success, string Message)> UpdateReviewAsync(int reviewId, int customerId, byte rating, string comment)
    {
        if (rating < 1 || rating > 5)
            return (false, "Số sao đánh giá phải từ 1 đến 5.");

        if (string.IsNullOrWhiteSpace(comment))
            return (false, "Vui lòng nhập nội dung nhận xét đánh giá.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var review = await context.ProductReviews.FindAsync(reviewId);
        if (review == null || review.CustomerId != customerId)
            return (false, "Không tìm thấy đánh giá của bạn.");

        review.Rating = rating;
        review.Comment = comment.Trim();
        review.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();
        return (true, "Cập nhật đánh giá thành công!");
    }

    public async Task<(bool Success, string Message)> DeleteReviewByCustomerAsync(int reviewId, int customerId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var review = await context.ProductReviews.FindAsync(reviewId);
        if (review == null || review.CustomerId != customerId)
            return (false, "Không tìm thấy đánh giá.");

        context.ProductReviews.Remove(review);
        await context.SaveChangesAsync();
        return (true, "Đã xóa đánh giá của bạn.");
    }

    public async Task<(bool Success, string Message)> DeleteReviewByAdminAsync(int reviewId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var review = await context.ProductReviews.FindAsync(reviewId);
        if (review == null)
            return (false, "Không tìm thấy đánh giá.");

        context.ProductReviews.Remove(review);
        await context.SaveChangesAsync();
        return (true, "Đã xóa đánh giá không phù hợp thành công!");
    }
}

