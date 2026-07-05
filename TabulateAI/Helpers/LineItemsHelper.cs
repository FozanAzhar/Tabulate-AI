using System.Text.Json;
using TabulateAI.Models;

namespace TabulateAI.Helpers;

public static class LineItemsHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string Serialize(IEnumerable<LineItem> items)
    {
        var payload = items
            .Select(item => new LineItemDto
            {
                Name = item.Name,
                Quantity = item.Quantity,
                Price = item.Price,
                IsDiscount = item.IsDiscount
            })
            .ToList();

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    public static List<LineItem> Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            var payload = JsonSerializer.Deserialize<List<LineItemDto>>(json, JsonOptions);
            if (payload is null || payload.Count == 0)
            {
                return [];
            }

            return payload
                .Where(item => !string.IsNullOrWhiteSpace(item.Name))
                .Select(item => new LineItem
                {
                    Name = item.Name.Trim(),
                    Quantity = item.Quantity?.Trim() ?? string.Empty,
                    Price = item.Price,
                    IsDiscount = item.IsDiscount
                })
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    public static void ApplyDividers(IList<LineItem> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            items[i].ShowDivider = i < items.Count - 1;
        }
    }

    private sealed class LineItemDto
    {
        public string Name { get; set; } = string.Empty;

        public string Quantity { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public bool IsDiscount { get; set; }
    }
}
