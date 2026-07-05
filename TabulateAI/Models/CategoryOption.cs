using CommunityToolkit.Mvvm.ComponentModel;

namespace TabulateAI.Models;

public partial class CategoryOption : ObservableObject
{
    public string Name { get; init; } = string.Empty;

    [ObservableProperty]
    private bool _isSelected;
}
