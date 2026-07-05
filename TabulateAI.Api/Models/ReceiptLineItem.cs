namespace TabulateAI.Api.Models;

public sealed class ReceiptLineItem
{
    public string Name { get; set; } = string.Empty;

    public string Quantity { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public bool IsDiscount { get; set; }
}
