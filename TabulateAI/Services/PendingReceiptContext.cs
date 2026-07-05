using TabulateAI.Models;

namespace TabulateAI.Services;

/// <summary>
/// Holds large extraction payloads between Processing and Review screens
/// (line items and raw OCR text are too large for Shell query strings).
/// </summary>
public sealed class PendingReceiptContext
{
    private List<LineItem> _lineItems = [];
    private string _rawOcrText = string.Empty;

    public void SetExtras(IEnumerable<LineItem> lineItems, string rawOcrText)
    {
        _lineItems = lineItems?.ToList() ?? [];
        _rawOcrText = rawOcrText ?? string.Empty;
    }

    public (List<LineItem> LineItems, string RawOcrText) ConsumeExtras()
    {
        var items = _lineItems;
        var raw = _rawOcrText;
        _lineItems = [];
        _rawOcrText = string.Empty;
        return (items, raw);
    }
}
