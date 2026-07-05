using SQLite;
using TabulateAI.Models;

namespace TabulateAI.Services;

public class ReceiptRepository : IReceiptRepository
{
    private SQLiteAsyncConnection? _database;

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_database is not null)
        {
            return _database;
        }

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "tabulate.db3");
        _database = new SQLiteAsyncConnection(dbPath);
        await _database.CreateTableAsync<Receipt>();
        await MigrateSchemaAsync(_database);
        return _database;
    }

    public async Task InitializeAsync()
    {
        await GetConnectionAsync();
    }

    public async Task<List<Receipt>> GetAllAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<Receipt>().OrderByDescending(r => r.Date).ToListAsync();
    }

    public async Task<List<Receipt>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllAsync();
        }

        var db = await GetConnectionAsync();
        var normalized = query.Trim().ToLowerInvariant();
        var receipts = await db.Table<Receipt>().ToListAsync();

        return receipts
            .Where(r =>
                r.Merchant.Contains(normalized, StringComparison.OrdinalIgnoreCase) ||
                r.Category.Contains(normalized, StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains(normalized, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.Date)
            .ToList();
    }

    public async Task<Receipt?> GetByIdAsync(int id)
    {
        var db = await GetConnectionAsync();
        return await db.Table<Receipt>().FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<int> SaveAsync(Receipt receipt)
    {
        var db = await GetConnectionAsync();

        if (receipt.Id == 0)
        {
            receipt.CreatedAt = DateTime.UtcNow;
            await db.InsertAsync(receipt);
            return receipt.Id;
        }

        await db.UpdateAsync(receipt);
        return receipt.Id;
    }

    public async Task DeleteAsync(int id)
    {
        var db = await GetConnectionAsync();
        await db.DeleteAsync<Receipt>(id);
    }

    public async Task<List<Receipt>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var db = await GetConnectionAsync();
        var receipts = await db.Table<Receipt>().ToListAsync();
        var start = startDate.Date;
        var end = endDate.Date;

        return receipts
            .Where(r => r.Date.Date >= start && r.Date.Date <= end)
            .OrderByDescending(r => r.Date)
            .ToList();
    }

    public async Task<decimal> GetMonthlyTotalAsync(int year, int month)
    {
        var db = await GetConnectionAsync();
        var receipts = await db.Table<Receipt>().ToListAsync();

        return receipts
            .Where(r => r.Date.Year == year && r.Date.Month == month)
            .Sum(r => r.Amount);
    }

    public async Task<List<CategorySummary>> GetCategorySummariesAsync(int year, int month)
    {
        var db = await GetConnectionAsync();
        var receipts = await db.Table<Receipt>().ToListAsync();

        return receipts
            .Where(r => r.Date.Year == year && r.Date.Month == month)
            .GroupBy(r => r.Category)
            .Select(g => new CategorySummary
            {
                Category = g.Key,
                Total = g.Sum(r => r.Amount),
                Count = g.Count()
            })
            .OrderByDescending(c => c.Total)
            .ToList();
    }

    private static async Task MigrateSchemaAsync(SQLiteAsyncConnection db)
    {
        try
        {
            await db.ExecuteAsync("ALTER TABLE Receipt ADD COLUMN Description TEXT NOT NULL DEFAULT ''");
        }
        catch
        {
            // Column already exists.
        }

        try
        {
            await db.ExecuteAsync("ALTER TABLE Receipt ADD COLUMN LocationAddress TEXT NOT NULL DEFAULT ''");
        }
        catch
        {
            // Column already exists.
        }

        try
        {
            await db.ExecuteAsync("ALTER TABLE Receipt ADD COLUMN Latitude REAL");
        }
        catch
        {
            // Column already exists.
        }

        try
        {
            await db.ExecuteAsync("ALTER TABLE Receipt ADD COLUMN Longitude REAL");
        }
        catch
        {
            // Column already exists.
        }

        try
        {
            await db.ExecuteAsync("ALTER TABLE Receipt ADD COLUMN PaymentMethod TEXT NOT NULL DEFAULT ''");
        }
        catch
        {
            // Column already exists.
        }

        try
        {
            await db.ExecuteAsync("ALTER TABLE Receipt ADD COLUMN LineItemsJson TEXT NOT NULL DEFAULT ''");
        }
        catch
        {
            // Column already exists.
        }

        try
        {
            await db.ExecuteAsync("ALTER TABLE Receipt ADD COLUMN MerchantLogoPath TEXT NOT NULL DEFAULT ''");
        }
        catch
        {
            // Column already exists.
        }
    }
}
