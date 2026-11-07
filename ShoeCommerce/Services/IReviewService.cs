using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public record ReviewViewDto(
    int ReviewId,
    int ProductId,
    string ProductName,
    string? ProductThumbnail,
    int CustomerId,
    string CustomerName,
    byte Rating,
    string Comment,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public interface IReviewService
{
    Task<bool> CanCustomerReviewProductAsync(int customerId, int productId);
    Task<List<ProductReview>> GetProductReviewsAsync(int productId);
    Task<List<ReviewViewDto>> GetCustomerReviewsAsync(int customerId);
    Task<(List<ReviewViewDto> Items, int TotalCount)> GetAdminReviewsAsync(
        int? productId = null,
        byte? rating = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 10);

    Task<(bool Success, string Message)> AddReviewAsync(int customerId, int productId, byte rating, string comment);
    Task<(bool Success, string Message)> UpdateReviewAsync(int reviewId, int customerId, byte rating, string comment);
    Task<(bool Success, string Message)> DeleteReviewByCustomerAsync(int reviewId, int customerId);
    Task<(bool Success, string Message)> DeleteReviewByAdminAsync(int reviewId);
}

