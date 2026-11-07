namespace ShoeCommerce.Models;

public enum ProductStatus
{
    Selling,
    Discontinued
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipping,
    Completed,
    Cancelled
}

public enum PaymentMethod
{
    COD,
    BankTransfer
}

public enum PaymentStatus
{
    Unpaid,
    Paid,
    Cancelled
}

public enum StockDocumentType
{
    PurchaseReceipt,
    AdjustmentIn,
    AdjustmentOut,
    OrderIssue,
    OrderCancellationReceipt
}

public enum StockDocumentStatus
{
    Draft,
    Confirmed
}

