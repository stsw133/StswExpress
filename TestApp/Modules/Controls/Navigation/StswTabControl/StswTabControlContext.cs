using System.Collections.ObjectModel;

namespace TestApp;

public class StswTabControlContext : ControlsContext
{
    #region Properties
    /// Items
    private ObservableCollection<StswTabItem> items = new()
    {
        new()
        {
            Header = new StswHeader()
            {
                IconData = StswIcons.Dice1,
                Content = "TEST 1"
            },
            Content = new StswToggleButtonContext()
        },
        new()
        {
            Header = new StswHeader()
            {
                IconData = StswIcons.Dice2,
                Content = "TEST 2"
            },
            Content = new StswTextBoxContext(),
            IsClosable = true
        },
        new()
        {
            Header = new StswHeader()
            {
                IconData = StswIcons.Dice3,
                Content = "TEST 3"
            },
            Content = new StswCheckBoxContext(),
            IsClosable = true
        }
    };
    public ObservableCollection<StswTabItem> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }
    #endregion
}
