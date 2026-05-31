using SQLite;

namespace TabulateAI.Models;

public class Receipt
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Merchant { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime Date { get; set; } = DateTime.Today;

    public string Category { get; set; } = ExpenseCategories.Other;

    public string ImagePath { get; set; } = string.Empty;

    public string RawOcrText { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
