using SpendSmart.Models;

namespace SpendSmart.Services;

public interface IReceiptRepository
{
    Task InitializeAsync();

    Task<List<Receipt>> GetAllAsync();

    Task<List<Receipt>> SearchAsync(string query);

    Task<Receipt?> GetByIdAsync(int id);

    Task<int> SaveAsync(Receipt receipt);

    Task DeleteAsync(int id);

    Task<decimal> GetMonthlyTotalAsync(int year, int month);

    Task<List<CategorySummary>> GetCategorySummariesAsync(int year, int month);
}
