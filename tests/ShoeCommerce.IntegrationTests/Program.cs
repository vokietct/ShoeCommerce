using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using ShoeCommerce.Data;
using ShoeCommerce.Models;
using ShoeCommerce.Services;

// Uses the real MySQL schema, including generated columns, checks and unique indexes.
// A fresh random database is created and only that database is removed afterwards.
var server = new MySqlConnectionStringBuilder(Environment.GetEnvironmentVariable("SHOE_TEST_MYSQL")
    ?? "Server=127.0.0.1;Port=3307;User ID=root;SSL Mode=Disabled;") { Database = "" };
var database = "shoestore_test_" + Guid.NewGuid().ToString("N");
await using var connection = new MySqlConnection(server.ConnectionString);
await connection.OpenAsync();
var schema = (await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "schema.sql")))
    .Replace("shoestore_db", database);
server.Database = database;
var factory = new TestContextFactory(server.ConnectionString);
var passed = 0;
try
{
    await new MySqlCommand(schema, connection).ExecuteNonQueryAsync();
    await using (var db = factory.CreateDbContext()) await DbSeeder.SeedAsync(db);
    var admin = NewAuth();
    Check((await admin.AdminLoginAsync("admin@shoestore.vn", "admin123")).Success, "Admin login");
    var employee = NewAuth();
    Check((await employee.AdminLoginAsync("linh@shoestore.com", "employee123")).Success, "Employee login");
    var customer = NewAuth();
    Check((await customer.RegisterCustomerAsync("Test Customer", "test@example.com", "0900000001", "test1234")).Success,
        "Registration creates user, customer and cart");
    var second = NewAuth();
    Check((await second.RegisterCustomerAsync("Second Customer", "second@example.com", "0900000002", "test1234")).Success, "Second customer registration");
    var customerId = customer.CurrentUser!.UserId;
    var secondId = second.CurrentUser!.UserId;
    var adminId = admin.CurrentUser!.UserId;
    var adminAccess = new AccessGuard(admin);
    var customerAccess = new AccessGuard(customer);
    var employeeAccess = new AccessGuard(employee);
    var products = new ProductService(factory, adminAccess);
    var inventory = new InventoryService(factory, adminAccess);
    var users = new UserService(factory, adminAccess);
    var addresses = new UserService(factory, customerAccess);
    var cart = new CartService(factory, customerAccess);
    var orders = new OrderService(factory, customerAccess);
    var staffOrders = new OrderService(factory, employeeAccess);
    var payments = new PaymentService(factory, employeeAccess);
    var secondOrders = new OrderService(factory, new AccessGuard(second));

    Check(!(await NewAuth().CustomerLoginAsync("admin@shoestore.vn", "admin123")).Success, "Customer portal rejects internal accounts");
    Check(!(await NewAuth().AdminLoginAsync("test@example.com", "test1234")).Success, "Admin portal rejects customers");
    await Denied(() => new ProductService(factory, employeeAccess).CreateProductAsync(new()), "Employee cannot create products");
    await Denied(() => new InventoryService(factory, employeeAccess).GetStockDocumentsAsync(), "Employee cannot access warehouse documents");
    await Denied(() => orders.GetCustomerOrdersAsync(secondId), "Customer cannot read another customer's orders");
    await Denied(() => cart.GetCartAsync(secondId), "Customer cannot read another customer's cart");
    await Denied(() => addresses.AddAddressAsync(secondId, "Other", "0900000000", "Private", false), "Customer cannot edit another address book");
    await Denied(() => new UserService(factory, employeeAccess).AddAddressAsync(customerId, "Other", "0900000000", "Private", false), "Employee cannot edit customer addresses");
    await Denied(() => new ProductService(factory, new AccessGuard(NewAuth())).GetAdminProductsAsync(), "Anonymous service access denied");

    var a1 = await addresses.AddAddressAsync(customerId, "First", "0900000000", "Address A", false);
    var a2 = await addresses.AddAddressAsync(customerId, "Second", "0900000000", "Address B", true);
    Check(a1.Success && a2.Success, "Add default addresses without violating generated unique index");
    Check((await addresses.SetDefaultAddressAsync(customerId, a1.AddressId!.Value)).Success, "Switch default to earlier address");
    Check((await addresses.SetDefaultAddressAsync(customerId, a2.AddressId!.Value)).Success, "Switch default to later address");
    Check((await addresses.UpdateAddressAsync(customerId, a1.AddressId.Value, "Updated", "0900000000", "New address", true)).Success, "Edit and switch default atomically");
    Check((await addresses.DeleteAddressAsync(customerId, a1.AddressId.Value)).Success &&
        (await addresses.GetCustomerAddressesAsync(customerId)).Single().IsDefault, "Delete default selects a remaining address");

    int categoryId, brandId, supplierId;
    await using (var db = factory.CreateDbContext())
    {
        categoryId = await db.Categories.Select(c => c.CategoryId).FirstAsync();
        brandId = await db.Brands.Select(b => b.BrandId).FirstAsync();
        supplierId = await db.Suppliers.Select(s => s.SupplierId).FirstAsync();
    }
    var created = await products.CreateProductAsync(new Product
    { ProductCode = "TEST-RANGE", ProductName = "Range Test", CategoryId = categoryId, BrandId = brandId, SupplierId = supplierId });
    Check(created.Success, "Create product");
    var low = await products.AddVariantAsync(new ProductVariant
    { ProductId = created.ProductId!.Value, Sku = "TEST-LOW", Size = "40", Color = "White", Price = 100, StockQuantity = 999 });
    var high = await products.AddVariantAsync(new ProductVariant
    { ProductId = created.ProductId.Value, Sku = "TEST-HIGH", Size = "42", Color = "White", Price = 300 });
    var lowId = low.VariantId!.Value;
    var highId = high.VariantId!.Value;
    Check(await Stock(lowId) == 0, "Variant creation cannot bypass warehouse stock tracking");
    Check((await products.GetCatalogProductsAsync(minPrice: 150, maxPrice: 250, searchTerm: "TEST-RANGE")).TotalItems == 0,
        "Price range must match one variant, not two different variants");
    Check((await products.GetCatalogProductsAsync(minPrice: 100, maxPrice: 100, searchTerm: "TEST-RANGE")).TotalItems == 1, "Price range includes boundary");

    var draft = await inventory.CreateManualStockDocumentAsync(adminId, StockDocumentType.AdjustmentIn, null, "Test receipt", [new(lowId, 5, 50, null)]);
    Check(draft.Success && await Stock(lowId) == 0, "Draft does not change stock");
    Check(!(await inventory.UpdateStockDocumentDraftAsync(draft.DocumentId!.Value, null, "Invalid quantity", [new(lowId, -2, 50, null)])).Success, "Negative draft quantity rejected");
    Check((await inventory.UpdateStockDocumentDraftAsync(draft.DocumentId.Value, null, "Updated quantity", [new(lowId, 6, 50, null)])).Success, "Replace draft line with same variant");
    var confirmations = await Task.WhenAll(
        inventory.ConfirmStockDocumentAsync(draft.DocumentId.Value, adminId),
        inventory.ConfirmStockDocumentAsync(draft.DocumentId.Value, adminId));
    Check(confirmations.Count(r => r.Success) == 1 && await Stock(lowId) == 6, "Concurrent document confirmations update stock exactly once");
    Check(!(await inventory.DeleteStockDocumentDraftAsync(draft.DocumentId.Value)).Success, "Confirmed document cannot be deleted");
    Check(!(await inventory.UpdateStockDocumentDraftAsync(draft.DocumentId.Value, null, "Invalid edit", [new(lowId, 8, 50, null)])).Success, "Confirmed document cannot be edited");
    Check(!(await inventory.CreateManualStockDocumentAsync(adminId, StockDocumentType.AdjustmentIn, null, "Duplicate", [new(lowId, 1, null, null), new(lowId, 1, null, null)])).Success, "Duplicate warehouse variants rejected");
    Check(!(await inventory.CreateManualStockDocumentAsync(adminId, StockDocumentType.AdjustmentOut, null, "Too much", [new(lowId, 2, null, null), new(highId, 1, null, null)], true)).Success && await Stock(lowId) == 6,
        "Failed multi-line warehouse confirmation rolls back all stock changes");

    Check((await cart.AddToCartAsync(customerId, lowId, 2)).Success, "Add product to cart");
    Check((await cart.AddToCartAsync(customerId, lowId, 1)).Success && (await cart.GetCartAsync(customerId)).TotalQuantity == 3, "Same variant quantities merge");
    Check(!(await Place(orders, customerId, PaymentMethod.COD, "INVALID")).Success && await Stock(lowId) == 6 && (await cart.GetCartAsync(customerId)).TotalQuantity == 3,
        "Invalid promotion preserves cart and stock without creating order");
    var promotion = new PromotionService(factory, adminAccess);
    Check((await promotion.CreatePromotionAsync(new Promotion { PromoCode = "TEST500", DiscountAmount = 500, MinOrderValue = 0, StartDate = DateTime.Now.AddDays(-1), EndDate = DateTime.Now.AddDays(1) })).Success, "Create fixed discount promotion");
    var placed = await Place(orders, customerId, PaymentMethod.COD, "TEST500");
    Check(placed.Success, "Checkout succeeds");
    var order = (await orders.GetCustomerOrdersAsync(customerId)).Single(o => o.OrderCode == placed.OrderCode);
    Check(order.DiscountAmount == 300 && order.Payment!.Amount == 0 && order.OrderDetails.Single().UnitPrice == 100 && await Stock(lowId) == 3 && (await cart.GetCartAsync(customerId)).TotalQuantity == 0,
        "Checkout saves capped discount, price snapshot, payment and stock atomically");
    Check(!(await payments.ConfirmBankTransferPaymentAsync(order.Payment!.PaymentId, null, employee.CurrentUser!.UserId)).Success, "Transfer confirmation cannot mark COD as paid");
    Check(!(await staffOrders.UpdateOrderStatusAsync(order.OrderId, OrderStatus.Completed, employee.CurrentUser.UserId)).Success, "Order status cannot skip delivery stages");
    foreach (var state in new[] { OrderStatus.Confirmed, OrderStatus.Shipping, OrderStatus.Completed })
        Check((await staffOrders.UpdateOrderStatusAsync(order.OrderId, state, employee.CurrentUser.UserId)).Success, $"COD transition to {state}");
    order = (await orders.GetCustomerOrderDetailAsync(customerId, order.OrderId))!;
    Check(order.Payment!.PaymentStatus == PaymentStatus.Paid && order.StatusHistories.Count == 4, "COD completion records payment and history");
    Check(!(await orders.CancelOrderCustomerAsync(customerId, order.OrderId, "Too late")).Success, "Completed paid order cannot be cancelled");
    var reviews = new ReviewService(factory, customerAccess);
    Check((await reviews.AddReviewAsync(customerId, created.ProductId.Value, 5, "Good shoes")).Success, "Buyer of completed order can review");
    Check(!(await reviews.AddReviewAsync(customerId, created.ProductId.Value, 4, "Duplicate")).Success, "Duplicate review rejected");

    await cart.AddToCartAsync(customerId, lowId, 1);
    placed = await Place(orders, customerId, PaymentMethod.BankTransfer);
    order = (await orders.GetCustomerOrdersAsync(customerId)).Single(o => o.OrderCode == placed.OrderCode);
    Check(!(await orders.CancelOrderCustomerAsync(customerId, order.OrderId, "Cancel transfer")).Success, "Customer cannot cancel transfer order directly");
    Check((await staffOrders.UpdateOrderStatusAsync(order.OrderId, OrderStatus.Confirmed, employee.CurrentUser.UserId)).Success &&
        !(await staffOrders.UpdateOrderStatusAsync(order.OrderId, OrderStatus.Shipping, employee.CurrentUser.UserId)).Success, "Unpaid transfer cannot ship");
    Check((await payments.ConfirmBankTransferPaymentAsync(order.Payment!.PaymentId, "TEST-TRANSFER", employee.CurrentUser.UserId)).Success, "Staff confirms transfer");
    Check(!(await staffOrders.CancelOrderAdminAsync(order.OrderId, "Paid", employee.CurrentUser.UserId, true)).Success, "Staff cannot cancel received payment");
    Check((await staffOrders.UpdateOrderStatusAsync(order.OrderId, OrderStatus.Shipping, employee.CurrentUser.UserId)).Success, "Paid transfer can ship");

    await cart.AddToCartAsync(customerId, lowId, 1);
    placed = await Place(orders, customerId, PaymentMethod.COD);
    order = (await orders.GetCustomerOrdersAsync(customerId)).Single(o => o.OrderCode == placed.OrderCode);
    var beforeCancel = await Stock(lowId);
    var cancellations = await Task.WhenAll(orders.CancelOrderCustomerAsync(customerId, order.OrderId, "Test cancellation"),
        staffOrders.CancelOrderAdminAsync(order.OrderId, "Same cancellation", employee.CurrentUser.UserId));
    Check(cancellations.Count(r => r.Success) == 1 && await Stock(lowId) == beforeCancel + 1, "Concurrent customer/staff cancellation restores stock exactly once");
    await using (var db = factory.CreateDbContext())
        Check(await db.StockDocuments.CountAsync(d => d.OrderId == order.OrderId && d.DocumentType == StockDocumentType.OrderCancellationReceipt) == 1, "One automatic return document per cancelled order");

    // Two customers compete for the final unit; use separate service scopes and transactions.
    await inventory.CreateManualStockDocumentAsync(adminId, StockDocumentType.AdjustmentIn, null, "Last item", [new(highId, 1, null, null)], true);
    await cart.AddToCartAsync(customerId, highId, 1);
    var secondCart = new CartService(factory, new AccessGuard(second));
    await secondCart.AddToCartAsync(secondId, highId, 1);
    var race = await Task.WhenAll(Place(orders, customerId, PaymentMethod.COD), Place(secondOrders, secondId, PaymentMethod.COD));
    Check(race.Count(r => r.Success) == 1 && await Stock(highId) == 0, "Competing checkouts cannot oversell the final unit");
    var loserCart = race[0].Success ? await secondCart.GetCartAsync(secondId) : await cart.GetCartAsync(customerId);
    Check(loserCart.TotalQuantity == 1, "Failed competing checkout retains cart");

    await using (var db = factory.CreateDbContext())
    {
        var account = await db.Users.FindAsync(customerId);
        var principal = SessionSecurity.Principal(account!);
        var restored = new AuthService(factory, new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = principal } });
        Check(restored.CurrentUser?.UserId == customerId && await restored.ValidateSessionAsync(), "Authenticated identity restores in a fresh request scope");
        account!.IsActive = false;
        await db.SaveChangesAsync();
        Check(!await restored.ValidateSessionAsync(), "Locked account invalidates an existing session");
    }
    Check(SessionSecurity.LocalReturnUrl("https://example.com") == "/" && SessionSecurity.LocalReturnUrl("//example.com") == "/" &&
        SessionSecurity.LocalReturnUrl("/\\example.com") == "/" && SessionSecurity.LocalReturnUrl("/orders") == "/orders", "Return URL only permits local paths");
    Console.WriteLine($"PASS: {passed} integration checks against MySQL {connection.ServerVersion}.");
}
finally
{
    MySqlConnection.ClearAllPools();
    // database is generated above and never obtained from configuration or the user.
    await new MySqlCommand($"DROP DATABASE IF EXISTS `{database}`", connection).ExecuteNonQueryAsync();
}

AuthService NewAuth() => new(factory, new HttpContextAccessor());
void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    passed++;
    Console.WriteLine("PASS: " + name);
}
async Task Denied(Func<Task> action, string name)
{
    try { await action(); }
    catch (UnauthorizedAccessException) { Check(true, name); return; }
    throw new Exception("FAIL (access was allowed): " + name);
}
async Task<int> Stock(int id)
{
    await using var db = factory.CreateDbContext();
    return await db.ProductVariants.Where(v => v.VariantId == id).Select(v => v.StockQuantity).SingleAsync();
}
static Task<(bool Success, string Message, string? OrderCode)> Place(OrderService service, int id, PaymentMethod method, string? promo = null) =>
    service.PlaceOrderAsync(id, "Test Receiver", "0900000000", "Test Address", null, method, promo);

sealed class TestContextFactory(string connectionString) : IDbContextFactory<ShoeStoreDbContext>
{
    public ShoeStoreDbContext CreateDbContext() => new(new DbContextOptionsBuilder<ShoeStoreDbContext>()
        .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 16))).Options);
    public Task<ShoeStoreDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(CreateDbContext());
}
