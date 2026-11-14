using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public record OrderSummaryView(
    int OrderId,
    string OrderCode,
    int CustomerId,
    string CustomerName,
    string ReceiverName,
    string Phone,
    string ShippingAddress,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal FinalAmount,
    OrderStatus OrderStatus,
    PaymentMethod PaymentMethod,
    PaymentStatus PaymentStatus,
    DateTime CreatedAt,
    int ItemCount
);

public interface IOrderService
{
    Task<(bool Success, string Message, decimal DiscountAmount, Promotion? Promo)> ValidatePromotionAsync(string promoCode, decimal totalAmount);

    Task<(bool Success, string Message, string? OrderCode)> PlaceOrderAsync(
        int customerId,
        string receiverName,
        string phone,
        string shippingAddress,
        string? note,
        PaymentMethod paymentMethod,
        string? promoCode);

    Task<List<Order>> GetCustomerOrdersAsync(int customerId);
    Task<Order?> GetCustomerOrderDetailAsync(int customerId, int orderId);
    Task<(bool Success, string Message)> CancelOrderCustomerAsync(int customerId, int orderId, string reason);

    // Admin / Employee
    Task<(List<OrderSummaryView> Items, int TotalCount)> GetAdminOrdersAsync(
        OrderStatus? status = null,
        string? searchCodeOrCustomer = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 10);

    Task<Order?> GetAdminOrderDetailAsync(int orderId);

    Task<(bool Success, string Message)> UpdateOrderStatusAsync(
        int orderId,
        OrderStatus newStatus,
        int changedByUserId,
        string? note = null);

    Task<(bool Success, string Message)> CancelOrderAdminAsync(
        int orderId,
        string cancelReason,
        int changedByUserId,
        bool bankTransferChecked = false);

    Task<(bool Success, string Message)> UpdateOrderShippingInfoAsync(
        int orderId,
        string receiverName,
        string phone,
        string address,
        string? note);
}

