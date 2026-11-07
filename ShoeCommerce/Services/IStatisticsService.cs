using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public record RevenueByDateDto(string DateLabel, decimal Revenue, int OrderCount);
public record StatusCountDto(OrderStatus Status, string StatusName, int Count, string ColorClass);
public record TopProductDto(int ProductId, string ProductName, string? ThumbnailUrl, int TotalQuantitySold, decimal TotalRevenue);
public record DashboardKpiDto(
    decimal TotalRevenue,
    int NewOrdersCount,
    int PendingOrdersCount,
    int NewCustomersCount,
    int LowStockVariantCount
);

public interface IStatisticsService
{
    Task<DashboardKpiDto> GetDashboardKpisAsync();
    Task<List<RevenueByDateDto>> GetRevenueByDateAsync(DateTime fromDate, DateTime toDate);
    Task<List<StatusCountDto>> GetOrderStatusBreakdownAsync();
    Task<List<TopProductDto>> GetTopSellingProductsAsync(int top = 5);
    Task<List<StockVariantView>> GetLowAndOutOfStockVariantsAsync();
}

