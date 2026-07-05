namespace TabulateAI.Models;

public class ReceiptGroup : List<ReceiptDisplayItem>
{
    public string MonthTitle { get; }

    public string GroupName => MonthTitle;

    public ReceiptGroup(string monthTitle, IEnumerable<ReceiptDisplayItem> items) : base(items)
    {
        MonthTitle = monthTitle;
    }
}
