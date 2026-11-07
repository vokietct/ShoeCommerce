using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public record PaymentViewDto(
    int PaymentId,
    int OrderId,
    string OrderCode,
    string CustomerName,
    PaymentMethod PaymentMethod,
    decimal Amount,
    PaymentStatus PaymentStatus,
    string? TransferReference,
    string? ConfirmedByUserName,
    DateTime? ConfirmedAt,
    DateTime CreatedAt
);

public interface IPaymentService
{
    Task<(List<PaymentViewDto> Items, int TotalCount)> GetPaymentsAsync(
        PaymentStatus? status = null,
        PaymentMethod? method = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 10);

    Task<(bool Success, string Message)> ConfirmBankTransferPaymentAsync(
        int paymentId,
        string? transferReference,
        int confirmedByUserId);

    Task<(bool Success, string Message)> ChangePaymentMethodAsync(
        int orderId,
        PaymentMethod newMethod);
}

