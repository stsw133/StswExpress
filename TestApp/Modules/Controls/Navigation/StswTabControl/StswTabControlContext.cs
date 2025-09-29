using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TestApp;
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
    [StswObservableProperty] ObservableCollection<StswTabItemModel> _items =
    [
        new(nameof(StswButton), StswIcons.Dice1, new StswButtonContext(), false),
        new(nameof(StswCheckBox), StswIcons.Dice2, new StswCheckBoxContext(), true),
        new(nameof(StswGroupBox), StswIcons.Dice3, new StswGroupBoxContext(), true)
    ];
    [StswObservableProperty] Visibility _newItemButtonVisibility;
    [StswObservableProperty] Dock _tabStripPlacement;

    public StswTabItemModel NewItem
    {
        get => _newItem;
        set
        {
            value.Content = new StswTextBoxContext();
            value.Name = nameof(StswTextBox);
            value.Icon = StswIcons.Plus;
            value.IsClosable = true;
            SetProperty(ref _newItem, value);
        }
    }
    private StswTabItemModel _newItem = new();
}

/// <summary>
/// Data model for StswTabControl's new tab template.
/// </summary>
public struct StswTabItemModel(string? name, Geometry? icon, object? content, bool isClosable)
{
    /// <summary>
    /// Gets or sets the name for the new tab item.
    /// </summary>
    public string? Name { get; set; } = name;

    /// <summary>
    /// Gets or sets the icon data represented by a Geometry object for the new tab item.
    /// </summary>
    public Geometry? Icon { get; set; } = icon;

    /// <summary>
    /// Gets or sets the type of the content to be created for the new tab item.
    /// </summary>
    public object? Content { get; set; } = content;

    /// <summary>
    /// Gets or sets whether the tab item is closable or not.
    /// </summary>
    public bool IsClosable { get; set; } = isClosable;
}
