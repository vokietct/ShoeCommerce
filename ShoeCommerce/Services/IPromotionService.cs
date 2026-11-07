using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public interface IPromotionService
{
    Task<List<Promotion>> GetAllPromotionsAsync(string? searchTerm = null);
    Task<Promotion?> GetPromotionByIdAsync(int promotionId);
    Task<(bool Success, string Message, int? PromotionId)> CreatePromotionAsync(Promotion promotion);
    Task<(bool Success, string Message)> UpdatePromotionAsync(Promotion promotion);
    Task<(bool Success, string Message)> TogglePromotionStatusAsync(int promotionId);
    Task<(bool Success, string Message)> DeletePromotionAsync(int promotionId);
}

