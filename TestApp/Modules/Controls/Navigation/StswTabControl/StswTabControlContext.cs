using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TestApp;

public class StswTabControlContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        NewItemButtonVisibility = (Visibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(NewItemButtonVisibility)))?.Value ?? default;
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
    public ObservableCollection<StswTabItemModel> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }
    private ObservableCollection<StswTabItemModel> _items =
    [
        new(nameof(StswButton), StswIcons.Dice1, new StswButtonContext(), false),
        new(nameof(StswCheckBox), StswIcons.Dice2, new StswCheckBoxContext(), true),
        new(nameof(StswGroupBox), StswIcons.Dice3, new StswGroupBoxContext(), true)
    ];

    /// NewItemButtonVisibility
    public Visibility NewItemButtonVisibility
    {
        get => _newItemButtonVisibility;
        set => SetProperty(ref _newItemButtonVisibility, value);
    }
    private Visibility _newItemButtonVisibility;

    /// NewItem
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

    /// TabStripPlacement
    public Dock TabStripPlacement
    {
        get => _tabStripPlacement;
        set => SetProperty(ref _tabStripPlacement, value);
    }
    private Dock _tabStripPlacement;
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
