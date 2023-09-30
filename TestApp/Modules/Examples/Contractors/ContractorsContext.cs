using System.Collections.ObjectModel;

namespace TestApp;

public class ContractorsContext : StswObservableObject
{
    #region Properties
    /// Tabs
    private static ObservableCollection<StswTabItem> tabs = new()
    {
        new()
        {
            Header = new StswHeader()
            {
                IconData = StswIcons.AccountGroup,
                Content = "Contractors list"
            },
            Content = new ContractorsListContext()
        }
    };
    public ObservableCollection<StswTabItem> Tabs
    {
        get => tabs;
        set => SetProperty(ref tabs, value);
    }
    public static ObservableCollection<StswTabItem> Tabs_ => tabs;
    #endregion
}
