namespace TabulateAI.Models;

public class CategoryManageItem
{
    public string Name { get; set; } = string.Empty;

    public bool IsCustom { get; set; }

    public string Subtitle => IsCustom ? "Custom category" : "Preset category";
}
