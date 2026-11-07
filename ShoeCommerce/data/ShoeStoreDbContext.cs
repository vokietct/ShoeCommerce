using Microsoft.EntityFrameworkCore;
using ShoeCommerce.Models;

namespace ShoeCommerce.Data;

public class ShoeStoreDbContext : DbContext
{
    public ShoeStoreDbContext(DbContextOptions<ShoeStoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<StockDocument> StockDocuments => Set<StockDocument>();
    public DbSet<StockDocumentItem> StockDocumentItems => Set<StockDocumentItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Roles
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.RoleName).IsUnique();
        });

        // 2. Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasOne(e => e.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 3. Employees (1-to-1 with User)
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId);
            entity.HasOne(e => e.User)
                  .WithOne(u => u.Employee)
                  .HasForeignKey<Employee>(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 4. Customers (1-to-1 with User)
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.CustomerId);
            entity.HasOne(c => c.User)
                  .WithOne(u => u.Customer)
                  .HasForeignKey<Customer>(c => c.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 5. Customer Addresses
        modelBuilder.Entity<CustomerAddress>(entity =>
        {
            entity.Property(e => e.DefaultCustomerId)
                  .ValueGeneratedOnAddOrUpdate();
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Addresses)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 6. Categories
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.CategoryName).IsUnique();
        });

        // 7. Brands
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasIndex(e => e.BrandName).IsUnique();
        });

        // 8. Suppliers
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasIndex(e => e.SupplierName);
        });

        // 9. Products
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.ProductCode).IsUnique();
            entity.Property(e => e.Status)
                  .HasConversion<string>();

            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Brand)
                  .WithMany(b => b.Products)
                  .HasForeignKey(e => e.BrandId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Supplier)
                  .WithMany(s => s.Products)
                  .HasForeignKey(e => e.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 10. Product Variants
        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.HasIndex(e => new { e.ProductId, e.Size, e.Color }).IsUnique();
            entity.Property(e => e.Price).HasPrecision(15, 2);
            entity.Property(e => e.Status)
                  .HasConversion<string>();

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.Variants)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 11. Promotions
        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasIndex(e => e.PromoCode).IsUnique();
            entity.Property(e => e.DiscountAmount).HasPrecision(15, 2);
            entity.Property(e => e.MinOrderValue).HasPrecision(15, 2);
        });

        // 12. Carts (1-to-1 with Customer)
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasIndex(e => e.CustomerId).IsUnique();
            entity.HasOne(e => e.Customer)
                  .WithOne(c => c.Cart)
                  .HasForeignKey<Cart>(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 13. Cart Items
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(e => new { e.CartId, e.VariantId }).IsUnique();

            entity.HasOne(e => e.Cart)
                  .WithMany(c => c.Items)
                  .HasForeignKey(e => e.CartId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Variant)
                  .WithMany(v => v.CartItems)
                  .HasForeignKey(e => e.VariantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 14. Orders
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.OrderCode).IsUnique();
            entity.Property(e => e.TotalAmount).HasPrecision(15, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(15, 2);
            entity.Property(e => e.FinalAmount)
                  .HasPrecision(15, 2)
                  .ValueGeneratedOnAddOrUpdate();
            entity.Property(e => e.OrderStatus)
                  .HasConversion<string>();

            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Orders)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Promotion)
                  .WithMany(p => p.Orders)
                  .HasForeignKey(e => e.PromotionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CancelledByUser)
                  .WithMany()
                  .HasForeignKey(e => e.CancelledByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 15. Order Details
        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasIndex(e => new { e.OrderId, e.VariantId }).IsUnique();
            entity.Property(e => e.UnitPrice).HasPrecision(15, 2);
            entity.Property(e => e.LineTotal)
                  .HasPrecision(25, 2)
                  .ValueGeneratedOnAddOrUpdate();

            entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderDetails)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Variant)
                  .WithMany(v => v.OrderDetails)
                  .HasForeignKey(e => e.VariantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 16. Order Status History
        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.Property(e => e.PreviousStatus)
                  .HasConversion<string?>();
            entity.Property(e => e.NewStatus)
                  .HasConversion<string>();

            entity.HasOne(e => e.Order)
                  .WithMany(o => o.StatusHistories)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ChangedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.ChangedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 17. Payments (1-to-1 with Order)
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(e => e.OrderId).IsUnique();
            entity.Property(e => e.Amount).HasPrecision(15, 2);
            entity.Property(e => e.PaymentMethod)
                  .HasConversion<string>();
            entity.Property(e => e.PaymentStatus)
                  .HasConversion<string>();

            entity.HasOne(e => e.Order)
                  .WithOne(o => o.Payment)
                  .HasForeignKey<Payment>(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ConfirmedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.ConfirmedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 18. Product Reviews
        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasIndex(e => new { e.ProductId, e.CustomerId }).IsUnique();

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.Reviews)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Reviews)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 19. Stock Documents
        modelBuilder.Entity<StockDocument>(entity =>
        {
            entity.HasIndex(e => e.DocumentCode).IsUnique();
            entity.HasIndex(e => new { e.OrderId, e.DocumentType }).IsUnique();
            entity.Property(e => e.DocumentType)
                  .HasConversion<string>();
            entity.Property(e => e.DocumentStatus)
                  .HasConversion<string>();

            entity.HasOne(e => e.Supplier)
                  .WithMany(s => s.StockDocuments)
                  .HasForeignKey(e => e.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Order)
                  .WithMany(o => o.StockDocuments)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ConfirmedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.ConfirmedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 20. Stock Document Items
        modelBuilder.Entity<StockDocumentItem>(entity =>
        {
            entity.HasIndex(e => new { e.DocumentId, e.VariantId }).IsUnique();
            entity.Property(e => e.UnitCost).HasPrecision(15, 2);

            entity.HasOne(e => e.Document)
                  .WithMany(d => d.Items)
                  .HasForeignKey(e => e.DocumentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Variant)
                  .WithMany(v => v.StockDocumentItems)
                  .HasForeignKey(e => e.VariantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

