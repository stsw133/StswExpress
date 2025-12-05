using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TestApp.Wpf;
public partial class StswTabControlContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        CanReorder = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(CanReorder)))?.Value ?? default;
        NewItemButtonVisibility = (Visibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(NewItemButtonVisibility)))?.Value ?? default;
        TabStripPlacement = (Dock?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(TabStripPlacement)))?.Value ?? default;
    }

    [StswObservableProperty] bool _areTabsVisible = true;
    [StswObservableProperty] bool _canReorder;
    [StswObservableProperty] ObservableCollection<StswTabItem> _items =
    [
        CreateTab(nameof(StswButton), StswIcons.Dice1, new StswButtonContext(), false),
        CreateTab(nameof(StswCheckBox), StswIcons.Dice2, new StswCheckBoxContext(), true),
        CreateTab(nameof(StswGroupBox), StswIcons.Dice3, new StswGroupBoxContext(), true)
    ];
    [StswObservableProperty] Visibility _newItemButtonVisibility;
    [StswObservableProperty] Dock _tabStripPlacement;

    private static StswTabItem CreateTab(string? name, Geometry? icon, object? content, bool isClosable)
    {
        return new StswTabItem
        {
            Content = content,
            Header = new StswLabel
            {
                Content = name,
                IconData = icon
            },
            IsClosable = isClosable
        };
    }
}
