using Microsoft.EntityFrameworkCore;
using ShoeCommerce.Data;
using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;
    private readonly IInventoryService _inventoryService;

    public StatisticsService(IDbContextFactory<ShoeStoreDbContext> contextFactory, IInventoryService inventoryService)
    {
        _contextFactory = contextFactory;
        _inventoryService = inventoryService;
    }

    public async Task<DashboardKpiDto> GetDashboardKpisAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Doanh thu: chỉ tính đơn Hoàn tất và Đã thanh toán
        var totalRevenue = await context.Orders
            .Where(o => o.OrderStatus == OrderStatus.Completed && o.Payment != null && o.Payment.PaymentStatus == PaymentStatus.Paid)
            .SumAsync(o => (decimal)(o.TotalAmount - o.DiscountAmount));

        var today = DateTime.Today;
        var newOrdersCount = await context.Orders
            .Where(o => o.CreatedAt >= today)
            .CountAsync();

        var pendingOrdersCount = await context.Orders
            .Where(o => o.OrderStatus == OrderStatus.Pending)
            .CountAsync();

        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        var newCustomersCount = await context.Users
            .Where(u => u.RoleId == 3 && u.CreatedAt >= firstDayOfMonth)
            .CountAsync();

        var lowStockVariants = await context.ProductVariants
            .Where(v => v.StockQuantity <= v.LowStockThreshold)
            .CountAsync();

        return new DashboardKpiDto(
            totalRevenue,
            newOrdersCount,
            pendingOrdersCount,
            newCustomersCount,
            lowStockVariants
        );
    }

    public async Task<List<RevenueByDateDto>> GetRevenueByDateAsync(DateTime fromDate, DateTime toDate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var endOfDay = toDate.Date.AddDays(1).AddTicks(-1);

        var completedOrders = await context.Orders
            .AsNoTracking()
            .Include(o => o.Payment)
            .Where(o => o.OrderStatus == OrderStatus.Completed &&
                        o.Payment != null && o.Payment.PaymentStatus == PaymentStatus.Paid &&
                        (o.CompletedAt ?? o.CreatedAt) >= fromDate.Date &&
                        (o.CompletedAt ?? o.CreatedAt) <= endOfDay)
            .ToListAsync();

        var days = (toDate.Date - fromDate.Date).Days;
        var result = new List<RevenueByDateDto>();

        if (days <= 31)
        {
            // Nhóm theo ngày
            var groups = completedOrders
                .GroupBy(o => (o.CompletedAt ?? o.CreatedAt).ToString("dd/MM"))
                .ToDictionary(g => g.Key, g => new { Rev = g.Sum(x => x.TotalAmount - x.DiscountAmount), Count = g.Count() });

            for (var d = fromDate.Date; d <= toDate.Date; d = d.AddDays(1))
            {
                var label = d.ToString("dd/MM");
                if (groups.TryGetValue(label, out var val))
                {
                    result.Add(new RevenueByDateDto(label, val.Rev, val.Count));
                }
                else
                {
                    result.Add(new RevenueByDateDto(label, 0, 0));
                }
            }
        }
        else
        {
            // Nhóm theo tháng
            var groups = completedOrders
                .GroupBy(o => (o.CompletedAt ?? o.CreatedAt).ToString("MM/yyyy"))
                .ToDictionary(g => g.Key, g => new { Rev = g.Sum(x => x.TotalAmount - x.DiscountAmount), Count = g.Count() });

            var cur = new DateTime(fromDate.Year, fromDate.Month, 1);
            var endMonth = new DateTime(toDate.Year, toDate.Month, 1);

            while (cur <= endMonth)
            {
                var label = cur.ToString("MM/yyyy");
                if (groups.TryGetValue(label, out var val))
                {
                    result.Add(new RevenueByDateDto(label, val.Rev, val.Count));
                }
                else
                {
                    result.Add(new RevenueByDateDto(label, 0, 0));
                }
                cur = cur.AddMonths(1);
            }
        }

        return result;
    }

    public async Task<List<StatusCountDto>> GetOrderStatusBreakdownAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var orders = await context.Orders
            .GroupBy(o => o.OrderStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var statusDict = orders.ToDictionary(x => x.Status, x => x.Count);

        return new List<StatusCountDto>
        {
            new StatusCountDto(OrderStatus.Pending, "Chờ xác nhận", statusDict.GetValueOrDefault(OrderStatus.Pending, 0), "warning"),
            new StatusCountDto(OrderStatus.Confirmed, "Đã xác nhận", statusDict.GetValueOrDefault(OrderStatus.Confirmed, 0), "info"),
            new StatusCountDto(OrderStatus.Shipping, "Đang giao", statusDict.GetValueOrDefault(OrderStatus.Shipping, 0), "primary"),
            new StatusCountDto(OrderStatus.Completed, "Hoàn tất", statusDict.GetValueOrDefault(OrderStatus.Completed, 0), "success"),
            new StatusCountDto(OrderStatus.Cancelled, "Đã hủy", statusDict.GetValueOrDefault(OrderStatus.Cancelled, 0), "danger")
        };
    }

    public async Task<List<TopProductDto>> GetTopSellingProductsAsync(int top = 5)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Thống kê sản phẩm bán chạy theo số lượng bán từ các đơn Hoàn tất
        var completedOrderIds = await context.Orders
            .Where(o => o.OrderStatus == OrderStatus.Completed)
            .Select(o => o.OrderId)
            .ToListAsync();

        var details = await context.OrderDetails
            .AsNoTracking()
            .Include(od => od.Variant)
                .ThenInclude(v => v!.Product)
            .Where(od => completedOrderIds.Contains(od.OrderId))
            .ToListAsync();

        var topProducts = details
            .Where(d => d.Variant?.Product != null)
            .GroupBy(d => d.Variant!.ProductId)
            .Select(g =>
            {
                var first = g.First();
                return new TopProductDto(
                    g.Key,
                    first.Variant!.Product!.ProductName,
                    first.Variant.Product.ThumbnailUrl,
                    g.Sum(x => x.Quantity),
                    g.Sum(x => x.Quantity * x.UnitPrice)
                );
            })
            .OrderByDescending(p => p.TotalQuantitySold)
            .Take(top)
            .ToList();

        return topProducts;
    }

    public async Task<List<StockVariantView>> GetLowAndOutOfStockVariantsAsync()
    {
        return await _inventoryService.GetStockVariantsAsync(lowStockOnly: true);
    }
}

