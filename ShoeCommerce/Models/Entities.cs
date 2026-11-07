using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeCommerce.Models;

[Table("roles")]
public class Role
{
    [Key]
    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("role_name")]
    [MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}

[Table("users")]
public class User
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("email")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Column("password_hash")]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("full_name")]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Column("phone")]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey(nameof(RoleId))]
    public virtual Role? Role { get; set; }

    public virtual Employee? Employee { get; set; }
    public virtual Customer? Customer { get; set; }
}

[Table("employees")]
public class Employee
{
    [Key]
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [Column("hire_date")]
    public DateOnly? HireDate { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public virtual User? User { get; set; }
}

[Table("customers")]
public class Customer
{
    [Key]
    [Column("customer_id")]
    public int CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public virtual User? User { get; set; }

    public virtual Cart? Cart { get; set; }
    public virtual ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
}

[Table("customer_addresses")]
public class CustomerAddress
{
    [Key]
    [Column("address_id")]
    public int AddressId { get; set; }

    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("receiver_name")]
    [MaxLength(150)]
    public string ReceiverName { get; set; } = string.Empty;

    [Column("phone")]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Column("address_line")]
    [MaxLength(1000)]
    public string AddressLine { get; set; } = string.Empty;

    [Column("is_default")]
    public bool IsDefault { get; set; } = false;

    [Column("default_customer_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public int? DefaultCustomerId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }
}

[Table("categories")]
public class Category
{
    [Key]
    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("category_name")]
    [MaxLength(150)]
    public string CategoryName { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

[Table("brands")]
public class Brand
{
    [Key]
    [Column("brand_id")]
    public int BrandId { get; set; }

    [Column("brand_name")]
    [MaxLength(150)]
    public string BrandName { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

[Table("suppliers")]
public class Supplier
{
    [Key]
    [Column("supplier_id")]
    public int SupplierId { get; set; }

    [Column("supplier_name")]
    [MaxLength(200)]
    public string SupplierName { get; set; } = string.Empty;

    [Column("phone")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [Column("email")]
    [MaxLength(255)]
    public string? Email { get; set; }

    [Column("address")]
    [MaxLength(1000)]
    public string? Address { get; set; }

    [Column("is_cooperating")]
    public bool IsCooperating { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<StockDocument> StockDocuments { get; set; } = new List<StockDocument>();
}

[Table("products")]
public class Product
{
    [Key]
    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("product_code")]
    [MaxLength(50)]
    public string ProductCode { get; set; } = string.Empty;

    [Column("product_name")]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("brand_id")]
    public int BrandId { get; set; }

    [Column("supplier_id")]
    public int SupplierId { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("thumbnail_url")]
    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    [Column("status")]
    public ProductStatus Status { get; set; } = ProductStatus.Selling;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey(nameof(CategoryId))]
    public virtual Category? Category { get; set; }

    [ForeignKey(nameof(BrandId))]
    public virtual Brand? Brand { get; set; }

    [ForeignKey(nameof(SupplierId))]
    public virtual Supplier? Supplier { get; set; }

    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
}

[Table("product_variants")]
public class ProductVariant
{
    [Key]
    [Column("variant_id")]
    public int VariantId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("sku")]
    [MaxLength(100)]
    public string Sku { get; set; } = string.Empty;

    [Column("size")]
    [MaxLength(20)]
    public string Size { get; set; } = string.Empty;

    [Column("color")]
    [MaxLength(50)]
    public string Color { get; set; } = string.Empty;

    [Column("price")]
    public decimal Price { get; set; }

    [Column("stock_quantity")]
    public int StockQuantity { get; set; } = 0;

    [Column("low_stock_threshold")]
    public int LowStockThreshold { get; set; } = 5;

    [Column("status")]
    public ProductStatus Status { get; set; } = ProductStatus.Selling;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public virtual ICollection<StockDocumentItem> StockDocumentItems { get; set; } = new List<StockDocumentItem>();
}

[Table("promotions")]
public class Promotion
{
    [Key]
    [Column("promotion_id")]
    public int PromotionId { get; set; }

    [Column("promo_code")]
    [MaxLength(50)]
    public string PromoCode { get; set; } = string.Empty;

    [Column("discount_amount")]
    public decimal DiscountAmount { get; set; }

    [Column("min_order_value")]
    public decimal MinOrderValue { get; set; } = 0;

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

[Table("carts")]
public class Cart
{
    [Key]
    [Column("cart_id")]
    public int CartId { get; set; }

    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }

    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

[Table("cart_items")]
public class CartItem
{
    [Key]
    [Column("cart_item_id")]
    public int CartItemId { get; set; }

    [Column("cart_id")]
    public int CartId { get; set; }

    [Column("variant_id")]
    public int VariantId { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey(nameof(CartId))]
    public virtual Cart? Cart { get; set; }

    [ForeignKey(nameof(VariantId))]
    public virtual ProductVariant? Variant { get; set; }
}

[Table("orders")]
public class Order
{
    [Key]
    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("order_code")]
    [MaxLength(50)]
    public string OrderCode { get; set; } = string.Empty;

    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("promotion_id")]
    public int? PromotionId { get; set; }

    [Column("receiver_name")]
    [MaxLength(150)]
    public string ReceiverName { get; set; } = string.Empty;

    [Column("phone")]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Column("shipping_address")]
    [MaxLength(1000)]
    public string ShippingAddress { get; set; } = string.Empty;

    [Column("total_amount")]
    public decimal TotalAmount { get; set; }

    [Column("discount_amount")]
    public decimal DiscountAmount { get; set; } = 0;

    [Column("final_amount")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal FinalAmount { get; set; }

    [Column("promo_code_snapshot")]
    [MaxLength(50)]
    public string? PromoCodeSnapshot { get; set; }

    [Column("order_status")]
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

    [Column("note")]
    public string? Note { get; set; }

    [Column("cancel_reason")]
    public string? CancelReason { get; set; }

    [Column("cancelled_by_user_id")]
    public int? CancelledByUserId { get; set; }

    [Column("cancelled_at")]
    public DateTime? CancelledAt { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }

    [ForeignKey(nameof(PromotionId))]
    public virtual Promotion? Promotion { get; set; }

    [ForeignKey(nameof(CancelledByUserId))]
    public virtual User? CancelledByUser { get; set; }

    public virtual Payment? Payment { get; set; }
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public virtual ICollection<OrderStatusHistory> StatusHistories { get; set; } = new List<OrderStatusHistory>();
    public virtual ICollection<StockDocument> StockDocuments { get; set; } = new List<StockDocument>();
}

[Table("order_details")]
public class OrderDetail
{
    [Key]
    [Column("order_detail_id")]
    public int OrderDetailId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("variant_id")]
    public int VariantId { get; set; }

    [Column("product_name_snapshot")]
    [MaxLength(200)]
    public string ProductNameSnapshot { get; set; } = string.Empty;

    [Column("sku_snapshot")]
    [MaxLength(100)]
    public string SkuSnapshot { get; set; } = string.Empty;

    [Column("size_snapshot")]
    [MaxLength(20)]
    public string SizeSnapshot { get; set; } = string.Empty;

    [Column("color_snapshot")]
    [MaxLength(50)]
    public string ColorSnapshot { get; set; } = string.Empty;

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("unit_price")]
    public decimal UnitPrice { get; set; }

    [Column("line_total")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal LineTotal { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }

    [ForeignKey(nameof(VariantId))]
    public virtual ProductVariant? Variant { get; set; }
}

[Table("order_status_history")]
public class OrderStatusHistory
{
    [Key]
    [Column("history_id")]
    public int HistoryId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("previous_status")]
    public OrderStatus? PreviousStatus { get; set; }

    [Column("new_status")]
    public OrderStatus NewStatus { get; set; }

    [Column("changed_by_user_id")]
    public int ChangedByUserId { get; set; }

    [Column("note")]
    public string? Note { get; set; }

    [Column("changed_at")]
    public DateTime ChangedAt { get; set; } = DateTime.Now;

    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }

    [ForeignKey(nameof(ChangedByUserId))]
    public virtual User? ChangedByUser { get; set; }
}

[Table("payments")]
public class Payment
{
    [Key]
    [Column("payment_id")]
    public int PaymentId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("payment_method")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("payment_status")]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    [Column("transfer_reference")]
    [MaxLength(100)]
    public string? TransferReference { get; set; }

    [Column("confirmed_by_user_id")]
    public int? ConfirmedByUserId { get; set; }

    [Column("confirmed_at")]
    public DateTime? ConfirmedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }

    [ForeignKey(nameof(ConfirmedByUserId))]
    public virtual User? ConfirmedByUser { get; set; }
}

[Table("product_reviews")]
public class ProductReview
{
    [Key]
    [Column("review_id")]
    public int ReviewId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("rating")]
    public byte Rating { get; set; }

    [Column("comment")]
    public string Comment { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }
}

[Table("stock_documents")]
public class StockDocument
{
    [Key]
    [Column("document_id")]
    public int DocumentId { get; set; }

    [Column("document_code")]
    [MaxLength(50)]
    public string DocumentCode { get; set; } = string.Empty;

    [Column("document_type")]
    public StockDocumentType DocumentType { get; set; }

    [Column("document_status")]
    public StockDocumentStatus DocumentStatus { get; set; } = StockDocumentStatus.Draft;

    [Column("supplier_id")]
    public int? SupplierId { get; set; }

    [Column("order_id")]
    public int? OrderId { get; set; }

    [Column("reason")]
    public string Reason { get; set; } = string.Empty;

    [Column("created_by_user_id")]
    public int CreatedByUserId { get; set; }

    [Column("confirmed_by_user_id")]
    public int? ConfirmedByUserId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("confirmed_at")]
    public DateTime? ConfirmedAt { get; set; }

    [ForeignKey(nameof(SupplierId))]
    public virtual Supplier? Supplier { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }

    [ForeignKey(nameof(CreatedByUserId))]
    public virtual User? CreatedByUser { get; set; }

    [ForeignKey(nameof(ConfirmedByUserId))]
    public virtual User? ConfirmedByUser { get; set; }

    public virtual ICollection<StockDocumentItem> Items { get; set; } = new List<StockDocumentItem>();
}

[Table("stock_document_items")]
public class StockDocumentItem
{
    [Key]
    [Column("document_item_id")]
    public int DocumentItemId { get; set; }

    [Column("document_id")]
    public int DocumentId { get; set; }

    [Column("variant_id")]
    public int VariantId { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("unit_cost")]
    public decimal? UnitCost { get; set; }

    [Column("note")]
    public string? Note { get; set; }

    [ForeignKey(nameof(DocumentId))]
    public virtual StockDocument? Document { get; set; }

    [ForeignKey(nameof(VariantId))]
    public virtual ProductVariant? Variant { get; set; }
}

