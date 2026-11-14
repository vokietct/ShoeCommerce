using System.Data;
using Microsoft.EntityFrameworkCore;
using ShoeCommerce.Data;
using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public class PaymentService : IPaymentService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;
    private readonly AccessGuard _access;

    public PaymentService(IDbContextFactory<ShoeStoreDbContext> contextFactory, AccessGuard access)
    {
        _contextFactory = contextFactory;
        _access = access;
    }

    public async Task<(List<PaymentViewDto> Items, int TotalCount)> GetPaymentsAsync(
        PaymentStatus? status = null,
        PaymentMethod? method = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 10)
    {
        await _access.StaffAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Payments
            .AsNoTracking()
            .Include(p => p.Order)
                .ThenInclude(o => o!.Customer)
                    .ThenInclude(c => c!.User)
            .Include(p => p.ConfirmedByUser)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(p => p.PaymentStatus == status.Value);

        if (method.HasValue)
            query = query.Where(p => p.PaymentMethod == method.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p => (p.Order != null && p.Order.OrderCode.ToLower().Contains(term)) ||
                                     (p.TransferReference != null && p.TransferReference.ToLower().Contains(term)) ||
                                     (p.Order != null && p.Order.Customer != null && p.Order.Customer.User != null && p.Order.Customer.User.FullName.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var paged = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtoList = paged.Select(p => new PaymentViewDto(
            p.PaymentId,
            p.OrderId,
            p.Order?.OrderCode ?? "",
            p.Order?.Customer?.User?.FullName ?? p.Order?.ReceiverName ?? "",
            p.PaymentMethod,
            p.Amount,
            p.PaymentStatus,
            p.TransferReference,
            p.ConfirmedByUser?.FullName,
            p.ConfirmedAt,
            p.CreatedAt
        )).ToList();

        return (dtoList, totalCount);
    }

    public async Task<(bool Success, string Message)> ConfirmBankTransferPaymentAsync(
        int paymentId,
        string? transferReference,
        int confirmedByUserId)
    {
        await _access.StaffAsync(confirmedByUserId);
        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        var orderId = await context.Payments.Where(p => p.PaymentId == paymentId).Select(p => p.OrderId).FirstOrDefaultAsync();
        await DatabaseLocks.OrderAsync(context, orderId);
        var payment = await context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

        if (payment == null)
            return (false, "Không tìm thấy bản ghi thanh toán.");

        if (payment.PaymentMethod != PaymentMethod.BankTransfer)
            return (false, "COD chỉ được xác nhận đã thu tiền khi hoàn tất giao hàng.");
        if (payment.Order == null || payment.Order.OrderStatus == OrderStatus.Cancelled)
            return (false, "Đơn hàng đã hủy hoặc không tồn tại.");
        if (payment.PaymentStatus == PaymentStatus.Paid)
            return (false, "Khoản thanh toán này đã được xác nhận trước đó.");

        if (payment.PaymentStatus == PaymentStatus.Cancelled)
            return (false, "Đơn hàng và thanh toán đã bị hủy, không thể xác nhận thanh toán.");

        payment.PaymentStatus = PaymentStatus.Paid;
        payment.TransferReference = transferReference?.Trim();
        payment.ConfirmedByUserId = confirmedByUserId;
        payment.ConfirmedAt = DateTime.Now;
        payment.UpdatedAt = DateTime.Now;

        // Lưu log lịch sử đơn hàng
        context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = payment.OrderId,
            PreviousStatus = payment.Order?.OrderStatus,
            NewStatus = payment.Order?.OrderStatus ?? OrderStatus.Pending,
            ChangedByUserId = confirmedByUserId,
            Note = $"Xác nhận thanh toán chuyển khoản: {payment.Amount:N0} ₫. Mã đối soát: {transferReference}",
            ChangedAt = DateTime.Now
        });

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return (true, "Xác nhận thanh toán chuyển khoản thành công!");
    }

    public async Task<(bool Success, string Message)> ChangePaymentMethodAsync(
        int orderId,
        PaymentMethod newMethod)
    {
        await _access.StaffAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        if (!Enum.IsDefined(newMethod)) return (false, "Phương thức thanh toán không hợp lệ.");
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await DatabaseLocks.OrderAsync(context, orderId);
        var order = await context.Orders
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null || order.Payment == null)
            return (false, "Không tìm thấy đơn hàng hoặc thông tin thanh toán.");

        // Cho phép thay đổi phương thức khi đơn còn Chờ xác nhận và chưa thanh toán
        if (order.OrderStatus != OrderStatus.Pending)
            return (false, "Chỉ được thay đổi phương thức thanh toán khi đơn đang Chờ xác nhận.");

        if (order.Payment.PaymentStatus != PaymentStatus.Unpaid)
            return (false, "Không thể đổi phương thức thanh toán khi đơn đã thanh toán hoặc đã hủy.");

        order.Payment.PaymentMethod = newMethod;
        order.Payment.UpdatedAt = DateTime.Now;
        await context.SaveChangesAsync();

        return (true, $"Đã chuyển phương thức thanh toán sang {(newMethod == PaymentMethod.COD ? "COD" : "Chuyển khoản")}.");
    }
}

