using CommunityToolkit.Mvvm.ComponentModel;

namespace TabulateAI.Models;

public partial class BudgetEditorItem : ObservableObject
{
    public string Category { get; set; } = string.Empty;

    [ObservableProperty]
    private string _budgetText = string.Empty;
}
