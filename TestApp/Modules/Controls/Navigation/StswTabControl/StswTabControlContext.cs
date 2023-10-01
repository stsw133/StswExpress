using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace TestApp;

public class StswTabControlContext : ControlsContext
{
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
    private Visibility newTabButtonVisibility = Visibility.Visible;
    public Visibility NewTabButtonVisibility
    {
        get => newTabButtonVisibility;
        set => SetProperty(ref newTabButtonVisibility, value);
    }

    /// TabStripPlacement
    private Dock tabStripPlacement = Dock.Top;
    public Dock TabStripPlacement
    {
        get => tabStripPlacement;
        set => SetProperty(ref tabStripPlacement, value);
    }
    #endregion
}
