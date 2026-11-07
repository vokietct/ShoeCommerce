using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public record StockVariantView(
    int VariantId,
    int ProductId,
    string ProductCode,
    string ProductName,
    string? CategoryName,
    string? BrandName,
    string Sku,
    string Size,
    string Color,
    decimal Price,
    int StockQuantity,
    int LowStockThreshold,
    bool IsLowStock,
    ProductStatus Status
);

public class CreateStockDocumentItemModel
{
    public int VariantId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal? UnitCost { get; set; }
    public string? Note { get; set; }

    public CreateStockDocumentItemModel() { }

    public CreateStockDocumentItemModel(int variantId, int quantity, decimal? unitCost, string? note)
    {
        VariantId = variantId;
        Quantity = quantity;
        UnitCost = unitCost;
        Note = note;
    }
}

public interface IInventoryService
{
    Task<List<StockVariantView>> GetStockVariantsAsync(
        string? searchTerm = null,
        bool? lowStockOnly = false,
        int? categoryId = null,
        int? brandId = null);

    Task<(List<StockDocument> Items, int TotalCount)> GetStockDocumentsAsync(
        StockDocumentType? type = null,
        StockDocumentStatus? status = null,
        int page = 1,
        int pageSize = 10);

    Task<StockDocument?> GetStockDocumentDetailAsync(int documentId);

    Task<(bool Success, string Message, int? DocumentId)> CreateManualStockDocumentAsync(
        int createdByUserId,
        StockDocumentType type,
        int? supplierId,
        string reason,
        List<CreateStockDocumentItemModel> items,
        bool confirmImmediately = false);

    Task<(bool Success, string Message)> UpdateStockDocumentDraftAsync(
        int documentId,
        int? supplierId,
        string reason,
        List<CreateStockDocumentItemModel> items);

    Task<(bool Success, string Message)> DeleteStockDocumentDraftAsync(int documentId);

    Task<(bool Success, string Message)> ConfirmStockDocumentAsync(int documentId, int confirmedByUserId);
}

