using System.Data;
using Microsoft.EntityFrameworkCore;
using ShoeCommerce.Data;
using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public class OrderService : IOrderService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;
    private readonly AccessGuard _access;

    public OrderService(IDbContextFactory<ShoeStoreDbContext> contextFactory, AccessGuard access)
    {
        _contextFactory = contextFactory;
        _access = access;
    }

    public async Task<(bool Success, string Message, decimal DiscountAmount, Promotion? Promo)> ValidatePromotionAsync(string promoCode, decimal totalAmount)
    {
        if (string.IsNullOrWhiteSpace(promoCode))
            return (false, "Vui lòng nhập mã khuyến mãi.", 0, null);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var code = promoCode.Trim().ToUpper();
        var promo = await context.Promotions.FirstOrDefaultAsync(p => p.PromoCode == code);

        if (promo == null)
            return (false, "Mã khuyến mãi không tồn tại.", 0, null);

        if (!promo.IsActive)
            return (false, "Mã khuyến mãi đã tạm ngừng áp dụng.", 0, null);

        var now = DateTime.Now;
        if (now < promo.StartDate)
            return (false, $"Mã khuyến mãi chỉ có hiệu lực từ ngày {promo.StartDate:dd/MM/yyyy}.", 0, null);

        if (now > promo.EndDate)
            return (false, "Mã khuyến mãi đã hết hạn sử dụng.", 0, null);

        if (totalAmount < promo.MinOrderValue)
            return (false, $"Đơn hàng chưa đạt giá trị tối thiểu {promo.MinOrderValue:N0} ₫ để áp dụng mã này.", 0, null);

        // Số tiền giảm không vượt quá tiền hàng
        var discount = Math.Min(promo.DiscountAmount, totalAmount);
        return (true, $"Áp dụng mã thành công! Giảm {discount:N0} ₫.", discount, promo);
    }

    public async Task<(bool Success, string Message, string? OrderCode)> PlaceOrderAsync(
        int customerId,
        string receiverName,
        string phone,
        string shippingAddress,
        string? note,
        PaymentMethod paymentMethod,
        string? promoCode)
    {
        await _access.CustomerAsync(customerId);
        if (!Enum.IsDefined(paymentMethod)) return (false, "Phương thức thanh toán không hợp lệ.", null);
        if (string.IsNullOrWhiteSpace(receiverName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(shippingAddress))
            return (false, "Vui lòng nhập đầy đủ họ tên người nhận, số điện thoại và địa chỉ nhận hàng.", null);

        await using var context = await _contextFactory.CreateDbContextAsync();
        using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        try
        {
            await DatabaseLocks.CustomerAsync(context, customerId);
            var cart = await context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Variant)
                        .ThenInclude(v => v!.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null || !cart.Items.Any())
                return (false, "Giỏ hàng của bạn đang trống.", null);

            // Kiểm tra tính hợp lệ và tồn kho của từng biến thể
            decimal totalAmount = 0;
            foreach (var item in cart.Items.OrderBy(i => i.VariantId))
            {
                if (item.Variant == null || item.Variant.Product == null)
                    return (false, "Sản phẩm trong giỏ không còn tồn tại.", null);

                if (item.Variant.Status != ProductStatus.Selling || item.Variant.Product.Status != ProductStatus.Selling)
                    return (false, $"Sản phẩm '{item.Variant.Product.ProductName}' ({item.Variant.Size}/{item.Variant.Color}) đã ngừng kinh doanh.", null);

                if (item.Quantity <= 0 || item.Quantity > item.Variant.StockQuantity)
                    return (false, $"Sản phẩm '{item.Variant.Product.ProductName}' ({item.Variant.Size}/{item.Variant.Color}) chỉ còn {item.Variant.StockQuantity} sản phẩm trong kho. Vui lòng cập nhật lại giỏ hàng.", null);

                totalAmount += item.Quantity * item.Variant.Price;
            }

            // Áp dụng khuyến mãi nếu có
            decimal discountAmount = 0;
            int? promoId = null;
            string? promoSnapshot = null;

            if (!string.IsNullOrWhiteSpace(promoCode))
            {
                var promoRes = await ValidatePromotionAsync(promoCode, totalAmount);
                if (!promoRes.Success) return (false, promoRes.Message, null);
                if (promoRes.Promo != null)
                {
                    discountAmount = promoRes.DiscountAmount;
                    promoId = promoRes.Promo.PromotionId;
                    promoSnapshot = promoRes.Promo.PromoCode;
                }
            }

            // Tạo mã đơn hàng duy nhất: SS + yyMMddHHmmss + 2 số ngẫu nhiên
            var orderCode = $"SS{DateTime.Now:yyMMddHHmmss}{Guid.NewGuid().ToString("N")[..12]}";

            var order = new Order
            {
                OrderCode = orderCode,
                CustomerId = customerId,
                PromotionId = promoId,
                ReceiverName = receiverName.Trim(),
                Phone = phone.Trim(),
                ShippingAddress = shippingAddress.Trim(),
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                PromoCodeSnapshot = promoSnapshot,
                OrderStatus = OrderStatus.Pending,
                Note = note?.Trim(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // Tạo chi tiết đơn hàng (snapshots) và trừ kho
            var stockItems = new List<StockDocumentItem>();

            foreach (var item in cart.Items.OrderBy(i => i.VariantId))
            {
                var detail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    VariantId = item.VariantId,
                    ProductNameSnapshot = item.Variant!.Product!.ProductName,
                    SkuSnapshot = item.Variant.Sku,
                    SizeSnapshot = item.Variant.Size,
                    ColorSnapshot = item.Variant.Color,
                    Quantity = item.Quantity,
                    UnitPrice = item.Variant.Price
                };
                context.OrderDetails.Add(detail);

                // Trừ tồn kho
                var updated = await context.ProductVariants
                    .Where(v => v.VariantId == item.VariantId && v.StockQuantity >= item.Quantity &&
                        v.Status == ProductStatus.Selling && v.Product!.Status == ProductStatus.Selling && v.Price == item.Variant.Price)
                    .ExecuteUpdateAsync(set => set
                        .SetProperty(v => v.StockQuantity, v => v.StockQuantity - item.Quantity)
                        .SetProperty(v => v.UpdatedAt, DateTime.Now));
                if (updated != 1)
                    return (false, "Tồn kho hoặc giá bán vừa thay đổi. Vui lòng kiểm tra lại giỏ hàng.", null);

                stockItems.Add(new StockDocumentItem
                {
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    UnitCost = item.Variant.Price,
                    Note = $"Xuất cho đơn {orderCode}"
                });
            }

            // Tạo bản ghi thanh toán
            var payment = new Payment
            {
                OrderId = order.OrderId,
                PaymentMethod = paymentMethod,
                Amount = totalAmount - discountAmount,
                PaymentStatus = PaymentStatus.Unpaid,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            context.Payments.Add(payment);

            // Tự động tạo phiếu xuất kho liên kết với đơn hàng
            var stockDoc = new StockDocument
            {
                DocumentCode = $"XK-{orderCode}",
                DocumentType = StockDocumentType.OrderIssue,
                DocumentStatus = StockDocumentStatus.Confirmed,
                OrderId = order.OrderId,
                Reason = $"Tự động xuất kho cho đơn hàng {orderCode}",
                CreatedByUserId = customerId,
                ConfirmedByUserId = customerId,
                CreatedAt = DateTime.Now,
                ConfirmedAt = DateTime.Now,
                Items = stockItems
            };
            context.StockDocuments.Add(stockDoc);

            // Lịch sử trạng thái đơn hàng
            context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.OrderId,
                PreviousStatus = null,
                NewStatus = OrderStatus.Pending,
                ChangedByUserId = customerId,
                Note = "Khách hàng đặt hàng",
                ChangedAt = DateTime.Now
            });

            // Làm trống giỏ hàng
            context.CartItems.RemoveRange(cart.Items);
            cart.UpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Đặt hàng thành công!", orderCode);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return (false, "Không thể cập nhật đơn hàng lúc này. Vui lòng tải lại và thử lại.", null);
        }
    }

    public async Task<List<Order>> GetCustomerOrdersAsync(int customerId)
    {
        await _access.CustomerAsync(customerId);
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Orders
            .AsNoTracking()
            .Include(o => o.Payment)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Variant)
                    .ThenInclude(v => v!.Product)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetCustomerOrderDetailAsync(int customerId, int orderId)
    {
        await _access.CustomerAsync(customerId);
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Orders
            .AsNoTracking()
            .Include(o => o.Payment)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Variant)
                    .ThenInclude(v => v!.Product)
            .Include(o => o.StatusHistories)
                .ThenInclude(h => h.ChangedByUser)
            .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.OrderId == orderId);
    }

    public async Task<(bool Success, string Message)> CancelOrderCustomerAsync(int customerId, int orderId, string reason)
    {
        await _access.CustomerAsync(customerId);
        if (string.IsNullOrWhiteSpace(reason))
            return (false, "Vui lòng nhập lý do hủy đơn.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        try
        {
            await DatabaseLocks.OrderAsync(context, orderId);
            var order = await context.Orders
                .Include(o => o.Payment)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Variant)
                .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.OrderId == orderId);

            if (order == null)
                return (false, "Không tìm thấy đơn hàng.");

            if (order.OrderStatus != OrderStatus.Pending)
                return (false, "Chỉ được phép hủy đơn hàng khi đơn còn ở trạng thái Chờ xác nhận.");

            if (order.Payment?.PaymentMethod != PaymentMethod.COD)
                return (false, "Đối với đơn hàng chuyển khoản, quý khách vui lòng liên hệ trực tiếp với cửa hàng để được kiểm tra và hỗ trợ hủy khi chưa nhận tiền.");

            if (order.Payment == null || order.Payment.PaymentStatus != PaymentStatus.Unpaid)
                return (false, "Đơn hàng đã thanh toán, không thể hủy.");

            // Kiểm tra đã có phiếu hoàn kho cho đơn này chưa để tránh hoàn kho nhiều lần
            var alreadyReturned = await context.StockDocuments.AnyAsync(sd =>
                sd.OrderId == order.OrderId && sd.DocumentType == StockDocumentType.OrderCancellationReceipt);

            if (!alreadyReturned)
            {
                // Hoàn lại tồn kho cho từng biến thể
                var returnItems = new List<StockDocumentItem>();
                foreach (var detail in order.OrderDetails.OrderBy(d => d.VariantId))
                {
                    if (detail.Variant != null)
                    {
                        await context.ProductVariants.Where(v => v.VariantId == detail.VariantId)
                            .ExecuteUpdateAsync(set => set
                                .SetProperty(v => v.StockQuantity, v => v.StockQuantity + detail.Quantity)
                                .SetProperty(v => v.UpdatedAt, DateTime.Now));
                    }

                    returnItems.Add(new StockDocumentItem
                    {
                        VariantId = detail.VariantId,
                        Quantity = detail.Quantity,
                        UnitCost = detail.UnitPrice,
                        Note = $"Hoàn kho do hủy đơn {order.OrderCode}"
                    });
                }

                // Tự động tạo phiếu nhập kho do hủy đơn
                var stockDoc = new StockDocument
                {
                    DocumentCode = $"NK-HUY-{order.OrderCode}",
                    DocumentType = StockDocumentType.OrderCancellationReceipt,
                    DocumentStatus = StockDocumentStatus.Confirmed,
                    OrderId = order.OrderId,
                    Reason = $"Tự động nhập kho do khách hủy đơn {order.OrderCode}. Lý do: {reason.Trim()}",
                    CreatedByUserId = customerId,
                    ConfirmedByUserId = customerId,
                    CreatedAt = DateTime.Now,
                    ConfirmedAt = DateTime.Now,
                    Items = returnItems
                };
                context.StockDocuments.Add(stockDoc);
            }

            // Cập nhật trạng thái đơn
            order.OrderStatus = OrderStatus.Cancelled;
            order.CancelReason = reason.Trim();
            order.CancelledByUserId = customerId;
            order.CancelledAt = DateTime.Now;
            order.UpdatedAt = DateTime.Now;

            // Cập nhật thanh toán thành Đã hủy
            if (order.Payment != null)
            {
                order.Payment.PaymentStatus = PaymentStatus.Cancelled;
                order.Payment.UpdatedAt = DateTime.Now;
            }

            // Ghi lịch sử
            context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.OrderId,
                PreviousStatus = OrderStatus.Pending,
                NewStatus = OrderStatus.Cancelled,
                ChangedByUserId = customerId,
                Note = $"Khách hàng hủy đơn. Lý do: {reason.Trim()}",
                ChangedAt = DateTime.Now
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Hủy đơn hàng thành công và hệ thống đã hoàn trả tồn kho.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return (false, "Không thể cập nhật đơn hàng lúc này. Vui lòng tải lại và thử lại.");
        }
    }

    public async Task<(List<OrderSummaryView> Items, int TotalCount)> GetAdminOrdersAsync(
        OrderStatus? status = null,
        string? searchCodeOrCustomer = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 10)
    {
        await _access.StaffAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
                .ThenInclude(c => c!.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderDetails)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.OrderStatus == status.Value);

        if (!string.IsNullOrWhiteSpace(searchCodeOrCustomer))
        {
            var term = searchCodeOrCustomer.Trim().ToLower();
            query = query.Where(o => o.OrderCode.ToLower().Contains(term) ||
                                     o.ReceiverName.ToLower().Contains(term) ||
                                     o.Phone.Contains(term) ||
                                     (o.Customer != null && o.Customer.User != null && o.Customer.User.FullName.ToLower().Contains(term)));
        }

        if (fromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= fromDate.Value.Date);

        if (toDate.HasValue)
        {
            var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(o => o.CreatedAt <= endOfDay);
        }

        var totalCount = await query.CountAsync();
        var paged = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtoList = paged.Select(o => new OrderSummaryView(
            o.OrderId,
            o.OrderCode,
            o.CustomerId,
            o.Customer?.User?.FullName ?? o.ReceiverName,
            o.ReceiverName,
            o.Phone,
            o.ShippingAddress,
            o.TotalAmount,
            o.DiscountAmount,
            o.TotalAmount - o.DiscountAmount,
            o.OrderStatus,
            o.Payment?.PaymentMethod ?? PaymentMethod.COD,
            o.Payment?.PaymentStatus ?? PaymentStatus.Unpaid,
            o.CreatedAt,
            o.OrderDetails.Sum(d => d.Quantity)
        )).ToList();

        return (dtoList, totalCount);
    }

    public async Task<Order?> GetAdminOrderDetailAsync(int orderId)
    {
        await _access.StaffAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
                .ThenInclude(c => c!.User)
            .Include(o => o.Promotion)
            .Include(o => o.Payment)
                .ThenInclude(p => p!.ConfirmedByUser)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Variant)
                    .ThenInclude(v => v!.Product)
            .Include(o => o.StatusHistories)
                .ThenInclude(h => h.ChangedByUser)
            .Include(o => o.StockDocuments)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    public async Task<(bool Success, string Message)> UpdateOrderStatusAsync(
        int orderId,
        OrderStatus newStatus,
        int changedByUserId,
        string? note = null)
    {
        await _access.StaffAsync(changedByUserId);
        await using var context = await _contextFactory.CreateDbContextAsync();
        using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        try
        {
            await DatabaseLocks.OrderAsync(context, orderId);
            var order = await context.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return (false, "Không tìm thấy đơn hàng.");

            if (order.Payment == null || order.Payment.PaymentStatus == PaymentStatus.Cancelled)
                return (false, "Đơn hàng không có thanh toán hợp lệ.");
            var current = order.OrderStatus;

            // Kiểm tra trình tự hợp lệ: Chờ xác nhận -> Đã xác nhận -> Đang giao -> Hoàn tất
            bool isValidTransition = (current, newStatus) switch
            {
                (OrderStatus.Pending, OrderStatus.Confirmed) => true,
                (OrderStatus.Confirmed, OrderStatus.Shipping) => true,
                (OrderStatus.Shipping, OrderStatus.Completed) => true,
                _ => false
            };

            if (!isValidTransition)
                return (false, $"Không thể chuyển trạng thái từ '{current}' sang '{newStatus}'. Trình tự xử lý phải là: Chờ xác nhận → Đã xác nhận → Đang giao → Hoàn tất.");

            // Với chuyển khoản, đơn phải được xác nhận Đã thanh toán trước khi chuyển sang Đang giao
            if (newStatus == OrderStatus.Shipping && order.Payment?.PaymentMethod == PaymentMethod.BankTransfer)
            {
                if (order.Payment.PaymentStatus != PaymentStatus.Paid)
                    return (false, "Đơn hàng thanh toán chuyển khoản phải được xác nhận 'Đã thanh toán' trước khi chuyển sang 'Đang giao'.");
            }

            // Với COD, khi nhân viên chuyển sang Hoàn tất thì tự động chuyển thanh toán sang Đã thanh toán
            if (newStatus == OrderStatus.Completed)
            {
                order.CompletedAt = DateTime.Now;

                if (order.Payment != null && order.Payment.PaymentMethod == PaymentMethod.COD)
                {
                    order.Payment.PaymentStatus = PaymentStatus.Paid;
                    order.Payment.ConfirmedByUserId = changedByUserId;
                    order.Payment.ConfirmedAt = DateTime.Now;
                    order.Payment.UpdatedAt = DateTime.Now;
                }
            }

            order.OrderStatus = newStatus;
            order.UpdatedAt = DateTime.Now;

            context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.OrderId,
                PreviousStatus = current,
                NewStatus = newStatus,
                ChangedByUserId = changedByUserId,
                Note = note ?? $"Cập nhật trạng thái thành {newStatus}",
                ChangedAt = DateTime.Now
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, $"Đã cập nhật đơn hàng sang trạng thái '{newStatus}'.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return (false, "Không thể cập nhật đơn hàng lúc này. Vui lòng tải lại và thử lại.");
        }
    }

    public async Task<(bool Success, string Message)> CancelOrderAdminAsync(
        int orderId,
        string cancelReason,
        int changedByUserId,
        bool bankTransferChecked = false)
    {
        await _access.StaffAsync(changedByUserId);
        if (string.IsNullOrWhiteSpace(cancelReason))
            return (false, "Vui lòng nhập lý do hủy đơn.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        try
        {
            await DatabaseLocks.OrderAsync(context, orderId);
            var order = await context.Orders
                .Include(o => o.Payment)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Variant)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return (false, "Không tìm thấy đơn hàng.");

            // Hủy đơn khi đơn ở trạng thái Chờ xác nhận hoặc Đã xác nhận, đồng thời chưa thanh toán.
            if (order.Payment?.PaymentMethod == PaymentMethod.BankTransfer && !bankTransferChecked)
                return (false, "Phải kiểm tra tài khoản ngân hàng và xác nhận chưa nhận tiền trước khi hủy.");
            if (order.OrderStatus != OrderStatus.Pending && order.OrderStatus != OrderStatus.Confirmed)
                return (false, "Chỉ được hủy đơn khi đơn đang ở trạng thái 'Chờ xác nhận' hoặc 'Đã xác nhận'.");

            if (order.Payment == null || order.Payment.PaymentStatus != PaymentStatus.Unpaid)
                return (false, "Đơn hàng đã được thanh toán. Trong phạm vi hệ thống không xử lý hủy đơn đã nhận tiền.");

            var alreadyReturned = await context.StockDocuments.AnyAsync(sd =>
                sd.OrderId == order.OrderId && sd.DocumentType == StockDocumentType.OrderCancellationReceipt);

            if (!alreadyReturned)
            {
                var returnItems = new List<StockDocumentItem>();
                foreach (var detail in order.OrderDetails.OrderBy(d => d.VariantId))
                {
                    if (detail.Variant != null)
                    {
                        await context.ProductVariants.Where(v => v.VariantId == detail.VariantId)
                            .ExecuteUpdateAsync(set => set
                                .SetProperty(v => v.StockQuantity, v => v.StockQuantity + detail.Quantity)
                                .SetProperty(v => v.UpdatedAt, DateTime.Now));
                    }

                    returnItems.Add(new StockDocumentItem
                    {
                        VariantId = detail.VariantId,
                        Quantity = detail.Quantity,
                        UnitCost = detail.UnitPrice,
                        Note = $"Hoàn kho do quản trị viên hủy đơn {order.OrderCode}"
                    });
                }

                var stockDoc = new StockDocument
                {
                    DocumentCode = $"NK-HUY-{order.OrderCode}",
                    DocumentType = StockDocumentType.OrderCancellationReceipt,
                    DocumentStatus = StockDocumentStatus.Confirmed,
                    OrderId = order.OrderId,
                    Reason = $"Quản trị hủy đơn {order.OrderCode}. Lý do: {cancelReason.Trim()}",
                    CreatedByUserId = changedByUserId,
                    ConfirmedByUserId = changedByUserId,
                    CreatedAt = DateTime.Now,
                    ConfirmedAt = DateTime.Now,
                    Items = returnItems
                };
                context.StockDocuments.Add(stockDoc);
            }

            var prev = order.OrderStatus;
            order.OrderStatus = OrderStatus.Cancelled;
            order.CancelReason = cancelReason.Trim();
            order.CancelledByUserId = changedByUserId;
            order.CancelledAt = DateTime.Now;
            order.UpdatedAt = DateTime.Now;

            if (order.Payment != null)
            {
                order.Payment.PaymentStatus = PaymentStatus.Cancelled;
                order.Payment.UpdatedAt = DateTime.Now;
            }

            context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.OrderId,
                PreviousStatus = prev,
                NewStatus = OrderStatus.Cancelled,
                ChangedByUserId = changedByUserId,
                Note = $"Quản trị hủy đơn. Lý do: {cancelReason.Trim()}",
                ChangedAt = DateTime.Now
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Đã hủy đơn hàng và hoàn trả tồn kho thành công.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return (false, "Không thể cập nhật đơn hàng lúc này. Vui lòng tải lại và thử lại.");
        }
    }

    public async Task<(bool Success, string Message)> UpdateOrderShippingInfoAsync(
        int orderId,
        string receiverName,
        string phone,
        string address,
        string? note)
    {
        await _access.StaffAsync();
        if (string.IsNullOrWhiteSpace(receiverName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(address))
            return (false, "Vui lòng nhập đầy đủ họ tên, số điện thoại và địa chỉ giao hàng.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await DatabaseLocks.OrderAsync(context, orderId);
        var order = await context.Orders.FindAsync(orderId);
        if (order == null)
            return (false, "Không tìm thấy đơn hàng.");

        // Cho phép sửa thông tin người nhận khi đơn còn Chờ xác nhận
        if (order.OrderStatus != OrderStatus.Pending)
            return (false, "Chỉ được chỉnh sửa thông tin giao hàng khi đơn đang ở trạng thái Chờ xác nhận.");

        order.ReceiverName = receiverName.Trim();
        order.Phone = phone.Trim();
        order.ShippingAddress = address.Trim();
        order.Note = note?.Trim();
        order.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return (true, "Cập nhật thông tin nhận hàng thành công!");
    }
}

