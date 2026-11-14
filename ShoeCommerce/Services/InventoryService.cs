using System.Data;
using Microsoft.EntityFrameworkCore;
using ShoeCommerce.Data;
using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public class InventoryService : IInventoryService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;
    private readonly AccessGuard _access;

    public InventoryService(IDbContextFactory<ShoeStoreDbContext> contextFactory, AccessGuard access)
    {
        _contextFactory = contextFactory;
        _access = access;
    }

    public async Task<List<StockVariantView>> GetStockVariantsAsync(
        string? searchTerm = null,
        bool? lowStockOnly = false,
        int? categoryId = null,
        int? brandId = null)
    {
        await _access.StaffAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.ProductVariants
            .AsNoTracking()
            .Include(v => v.Product)
                .ThenInclude(p => p!.Category)
            .Include(v => v.Product)
                .ThenInclude(p => p!.Brand)
            .AsQueryable();

        if (categoryId.HasValue && categoryId.Value > 0)
            query = query.Where(v => v.Product != null && v.Product.CategoryId == categoryId.Value);

        if (brandId.HasValue && brandId.Value > 0)
            query = query.Where(v => v.Product != null && v.Product.BrandId == brandId.Value);

        if (lowStockOnly == true)
            query = query.Where(v => v.StockQuantity <= v.LowStockThreshold);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(v => v.Sku.ToLower().Contains(term) ||
                                     (v.Product != null && (v.Product.ProductName.ToLower().Contains(term) || v.Product.ProductCode.ToLower().Contains(term))) ||
                                     v.Size.ToLower().Contains(term) ||
                                     v.Color.ToLower().Contains(term));
        }

        var list = await query
            .OrderBy(v => v.Product!.ProductName)
            .ThenBy(v => v.Size)
            .ToListAsync();

        return list.Select(v => new StockVariantView(
            v.VariantId,
            v.ProductId,
            v.Product?.ProductCode ?? "",
            v.Product?.ProductName ?? "",
            v.Product?.Category?.CategoryName,
            v.Product?.Brand?.BrandName,
            v.Sku,
            v.Size,
            v.Color,
            v.Price,
            v.StockQuantity,
            v.LowStockThreshold,
            v.StockQuantity <= v.LowStockThreshold,
            v.Status
        )).ToList();
    }

    public async Task<(List<StockDocument> Items, int TotalCount)> GetStockDocumentsAsync(
        StockDocumentType? type = null,
        StockDocumentStatus? status = null,
        int page = 1,
        int pageSize = 10)
    {
        await _access.AdminAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.StockDocuments
            .AsNoTracking()
            .Include(d => d.Supplier)
            .Include(d => d.CreatedByUser)
            .Include(d => d.ConfirmedByUser)
            .Include(d => d.Items)
            .AsQueryable();

        if (type.HasValue)
            query = query.Where(d => d.DocumentType == type.Value);

        if (status.HasValue)
            query = query.Where(d => d.DocumentStatus == status.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<StockDocument?> GetStockDocumentDetailAsync(int documentId)
    {
        await _access.AdminAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.StockDocuments
            .AsNoTracking()
            .Include(d => d.Supplier)
            .Include(d => d.CreatedByUser)
            .Include(d => d.ConfirmedByUser)
            .Include(d => d.Order)
            .Include(d => d.Items)
                .ThenInclude(i => i.Variant)
                    .ThenInclude(v => v!.Product)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);
    }

    public async Task<(bool Success, string Message, int? DocumentId)> CreateManualStockDocumentAsync(
        int createdByUserId,
        StockDocumentType type,
        int? supplierId,
        string reason,
        List<CreateStockDocumentItemModel> items,
        bool confirmImmediately = false)
    {
        await _access.AdminAsync(createdByUserId);
        if (!Enum.IsDefined(type) || type == StockDocumentType.OrderIssue || type == StockDocumentType.OrderCancellationReceipt)
            return (false, "Không được tự lập phiếu tự động liên quan đến đơn hàng.", null);

        if (string.IsNullOrWhiteSpace(reason))
            return (false, "Vui lòng nhập lý do lập phiếu kho.", null);

        if (items == null || !items.Any())
            return (false, "Phiếu kho phải có ít nhất một sản phẩm / biến thể.", null);

        if (type == StockDocumentType.PurchaseReceipt && (!supplierId.HasValue || supplierId.Value <= 0))
            return (false, "Phiếu nhập hàng yêu cầu phải chọn nhà cung cấp.", null);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var validation = await ValidateItemsAsync(context, type, supplierId, items);
        if (validation != null) return (false, validation, null);
        using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        try
        {
            var prefix = type switch
            {
                StockDocumentType.PurchaseReceipt => "PN",
                StockDocumentType.AdjustmentIn => "NDCK",
                StockDocumentType.AdjustmentOut => "XDCK",
                _ => "PK"
            };

            var docCode = $"{prefix}-{DateTime.Now:yyMMddHHmmss}{Guid.NewGuid().ToString("N")[..12]}";

            var doc = new StockDocument
            {
                DocumentCode = docCode,
                DocumentType = type,
                DocumentStatus = StockDocumentStatus.Draft,
                SupplierId = supplierId,
                Reason = reason.Trim(),
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.Now
            };

            context.StockDocuments.Add(doc);
            await context.SaveChangesAsync();

            var docItems = new List<StockDocumentItem>();
            foreach (var item in items)
            {
                if (item.Quantity <= 0)
                    throw new Exception("Số lượng của từng sản phẩm trong phiếu kho phải lớn hơn 0.");

                docItems.Add(new StockDocumentItem
                {
                    DocumentId = doc.DocumentId,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    Note = item.Note?.Trim()
                });
            }

            context.StockDocumentItems.AddRange(docItems);
            await context.SaveChangesAsync();

            if (confirmImmediately)
            {
                // Gọi xác nhận phiếu ngay
                var confirmRes = await ConfirmDocumentInternalAsync(context, doc.DocumentId, createdByUserId);
                if (!confirmRes.Success)
                {
                    await transaction.RollbackAsync();
                    return (false, confirmRes.Message, null);
                }
            }

            await transaction.CommitAsync();
            return (true, confirmImmediately ? "Đã lập và xác nhận phiếu kho thành công!" : "Đã lưu phiếu kho ở trạng thái Nháp.", doc.DocumentId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Lỗi tạo phiếu kho: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message)> UpdateStockDocumentDraftAsync(
        int documentId,
        int? supplierId,
        string reason,
        List<CreateStockDocumentItemModel> items)
    {
        await _access.AdminAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await DatabaseLocks.StockDocumentAsync(context, documentId);
        var doc = await context.StockDocuments
            .Include(d => d.Items)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (doc == null)
            return (false, "Không tìm thấy phiếu kho.");

        if (doc.DocumentStatus != StockDocumentStatus.Draft)
            return (false, "Không thể chỉnh sửa phiếu kho đã được xác nhận.");

        if (doc.OrderId != null)
            return (false, "Không được sửa các phiếu kho tự động tạo theo đơn hàng.");

        if (string.IsNullOrWhiteSpace(reason))
            return (false, "Vui lòng nhập lý do.");

        if (items == null || !items.Any())
            return (false, "Phiếu kho phải có ít nhất 1 sản phẩm.");

        var validation = await ValidateItemsAsync(context, doc.DocumentType, supplierId, items);
        if (validation != null) return (false, validation);
        doc.SupplierId = supplierId;
        doc.Reason = reason.Trim();

        // Xóa items cũ, thêm items mới
        context.StockDocumentItems.RemoveRange(doc.Items);
        await context.SaveChangesAsync();
        foreach (var item in items)
        {
            context.StockDocumentItems.Add(new StockDocumentItem
            {
                DocumentId = doc.DocumentId,
                VariantId = item.VariantId,
                Quantity = item.Quantity,
                UnitCost = item.UnitCost,
                Note = item.Note?.Trim()
            });
        }

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return (true, "Đã cập nhật phiếu kho nháp thành công!");
    }

    public async Task<(bool Success, string Message)> DeleteStockDocumentDraftAsync(int documentId)
    {
        await _access.AdminAsync();
        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await DatabaseLocks.StockDocumentAsync(context, documentId);
        var doc = await context.StockDocuments
            .Include(d => d.Items)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (doc == null)
            return (false, "Không tìm thấy phiếu kho.");

        if (doc.DocumentStatus != StockDocumentStatus.Draft)
            return (false, "Không thể xóa phiếu kho đã được xác nhận.");

        if (doc.OrderId != null)
            return (false, "Không được xóa phiếu kho tự động theo đơn hàng.");

        context.StockDocumentItems.RemoveRange(doc.Items);
        context.StockDocuments.Remove(doc);
        await context.SaveChangesAsync();

        await transaction.CommitAsync();
        return (true, "Đã xóa phiếu kho nháp thành công!");
    }

    public async Task<(bool Success, string Message)> ConfirmStockDocumentAsync(int documentId, int confirmedByUserId)
    {
        await _access.AdminAsync(confirmedByUserId);
        await using var context = await _contextFactory.CreateDbContextAsync();
        using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        await DatabaseLocks.StockDocumentAsync(context, documentId);
        var res = await ConfirmDocumentInternalAsync(context, documentId, confirmedByUserId);
        if (!res.Success)
        {
            await transaction.RollbackAsync();
            return res;
        }

        await transaction.CommitAsync();
        return res;
    }

    private static async Task<(bool Success, string Message)> ConfirmDocumentInternalAsync(
        ShoeStoreDbContext context,
        int documentId,
        int confirmedByUserId)
    {
        var doc = await context.StockDocuments
            .Include(d => d.Items)
                .ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (doc == null)
            return (false, "Không tìm thấy phiếu kho.");

        if (doc.DocumentStatus != StockDocumentStatus.Draft)
            return (false, "Phiếu này đã được xác nhận trước đó, không thể xác nhận lại.");

        if (doc.OrderId != null || doc.DocumentType is StockDocumentType.OrderIssue or StockDocumentType.OrderCancellationReceipt)
            return (false, "Không được xác nhận thủ công phiếu tự động theo đơn hàng.");
        var validation = await ValidateItemsAsync(context, doc.DocumentType, doc.SupplierId,
            doc.Items.Select(i => new CreateStockDocumentItemModel(i.VariantId, i.Quantity, i.UnitCost, i.Note)).ToList());
        if (validation != null) return (false, validation);

        foreach (var item in doc.Items.OrderBy(i => i.VariantId))
        {
            var delta = doc.DocumentType == StockDocumentType.AdjustmentOut ? -item.Quantity : item.Quantity;
            var changed = await context.ProductVariants
                .Where(v => v.VariantId == item.VariantId && (long)v.StockQuantity + delta >= 0 && (long)v.StockQuantity + delta <= int.MaxValue)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(v => v.StockQuantity, v => v.StockQuantity + delta)
                    .SetProperty(v => v.UpdatedAt, DateTime.Now));
            if (changed != 1)
                return (false, "Tồn kho không đủ hoặc vượt giới hạn. Phiếu chưa được xác nhận.");
        }

        doc.DocumentStatus = StockDocumentStatus.Confirmed;
        doc.ConfirmedByUserId = confirmedByUserId;
        doc.ConfirmedAt = DateTime.Now;

        await context.SaveChangesAsync();
        return (true, "Xác nhận phiếu kho thành công và đã cập nhật tồn kho.");
    }
    private static async Task<string?> ValidateItemsAsync(ShoeStoreDbContext context, StockDocumentType type,
        int? supplierId, List<CreateStockDocumentItemModel> items)
    {
        if (type is not (StockDocumentType.PurchaseReceipt or StockDocumentType.AdjustmentIn or StockDocumentType.AdjustmentOut))
            return "Loại phiếu không hợp lệ.";
        if (items.Count == 0 || items.Any(i => i.Quantity <= 0 || i.UnitCost < 0))
            return "Phiếu phải có sản phẩm, số lượng dương và giá vốn không âm.";
        var ids = items.Select(i => i.VariantId).Distinct().ToList();
        if (ids.Count != items.Count) return "Không được lặp lại biến thể trong một phiếu.";
        if (await context.ProductVariants.CountAsync(v => ids.Contains(v.VariantId)) != ids.Count)
            return "Có biến thể không tồn tại.";
        if (type == StockDocumentType.PurchaseReceipt && (!supplierId.HasValue ||
            !await context.Suppliers.AnyAsync(s => s.SupplierId == supplierId && s.IsCooperating)))
            return "Vui lòng chọn nhà cung cấp đang hợp tác.";
        if (supplierId.HasValue && !await context.Suppliers.AnyAsync(s => s.SupplierId == supplierId))
            return "Nhà cung cấp không tồn tại.";
        return null;
    }

}

