using Microsoft.EntityFrameworkCore;

namespace ShoeCommerce.Data;

public static class DatabaseLocks
{
    public static async Task CustomerAsync(ShoeStoreDbContext db, int id) =>
        await db.Customers.FromSqlInterpolated($"SELECT * FROM customers WHERE customer_id = {id} FOR UPDATE")
            .AsNoTracking().ToListAsync();

    public static async Task OrderAsync(ShoeStoreDbContext db, int id) =>
        await db.Orders.FromSqlInterpolated($"SELECT * FROM orders WHERE order_id = {id} FOR UPDATE")
            .AsNoTracking().ToListAsync();

    public static async Task StockDocumentAsync(ShoeStoreDbContext db, int id) =>
        await db.StockDocuments.FromSqlInterpolated($"SELECT * FROM stock_documents WHERE document_id = {id} FOR UPDATE")
            .AsNoTracking().ToListAsync();
}
