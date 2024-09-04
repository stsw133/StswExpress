using System.Collections.ObjectModel;

namespace TestApp;

public class ContractorsContext : StswObservableObject
{
    /// Tabs
    public static ObservableCollection<StswTabItem> Tabs_ => _tabs;
    public ObservableCollection<StswTabItem> Tabs
    {
        get => _tabs;
        set => SetProperty(ref _tabs, value);
    }
    private static ObservableCollection<StswTabItem> _tabs = new()
    {
        new()
        {
            Header = new StswLabel()
            {
                IconData = StswIcons.AccountGroup,
                Content = "Contractors list"
            },
            Content = new ContractorsListContext()
        }
    };
}
