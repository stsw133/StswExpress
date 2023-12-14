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

    #region Properties
    /// AreTabsVisible
    private bool areTabsVisible = true;
    public bool AreTabsVisible
    {
        get => areTabsVisible;
        set => SetProperty(ref areTabsVisible, value);
    }

    /// Items
    private ObservableCollection<StswTabItem> items = new()
    {
        new()
        {
            Header = new StswHeader()
            {
                IconData = StswIcons.Dice1,
                Content = "Tab 1"
            },
            Content = new HomeContext()
        },
        new()
        {
            Header = new StswHeader()
            {
                IconData = StswIcons.Dice2,
                Content = "Tab 2"
            },
            Content = new HomeContext(),
            IsClosable = true
        },
        new()
        {
            Header = new StswHeader()
            {
                IconData = StswIcons.Dice3,
                Content = "Tab 3"
            },
            Content = new HomeContext(),
            IsClosable = true
        }
    };
    public ObservableCollection<StswTabItem> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }

    /// NewTabButtonVisibility
    private Visibility newTabButtonVisibility;
    public Visibility NewTabButtonVisibility
    {
        get => newTabButtonVisibility;
        set => SetProperty(ref newTabButtonVisibility, value);
    }

    /// TabStripPlacement
    private Dock tabStripPlacement;
    public Dock TabStripPlacement
    {
        get => tabStripPlacement;
        set => SetProperty(ref tabStripPlacement, value);
    }
    #endregion
}
