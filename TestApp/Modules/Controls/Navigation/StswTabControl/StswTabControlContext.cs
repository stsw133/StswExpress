using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TestApp;

public class StswTabControlContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        NewTabButtonVisibility = (Visibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(NewTabButtonVisibility)))?.Value ?? default;
        TabStripPlacement = (Dock?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(TabStripPlacement)))?.Value ?? default;
    }

    /// AreTabsVisible
    public bool AreTabsVisible
    {
        get => _areTabsVisible;
        set => SetProperty(ref _areTabsVisible, value);
    }
    private bool _areTabsVisible = true;

    /// Items
    public ObservableCollection<StswTabItem> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }
    private ObservableCollection<StswTabItem> _items =
    [
        new()
        {
            Header = new StswLabel()
            {
                IconData = StswIcons.Dice1,
                Content = nameof(StswButton)
            },
            Content = new StswButtonContext()
        },
        new()
        {
            Header = new StswLabel()
            {
                IconData = StswIcons.Dice2,
                Content = nameof(StswCheckBox)
            },
            Content = new StswCheckBoxContext(),
            IsClosable = true
        },
        new()
        {
            Header = new StswLabel()
            {
                IconData = StswIcons.Dice3,
                Content = nameof(StswGroupBox)
            },
            Content = new StswGroupBoxContext(),
            IsClosable = true
        }
    ];

    /// NewTabButtonVisibility
    public Visibility NewTabButtonVisibility
    {
        get => _newTabButtonVisibility;
        set => SetProperty(ref _newTabButtonVisibility, value);
    }
    private Visibility _newTabButtonVisibility;

    /// TabStripPlacement
    public Dock TabStripPlacement
    {
        get => _tabStripPlacement;
        set => SetProperty(ref _tabStripPlacement, value);
    }
    private Dock _tabStripPlacement;
}
