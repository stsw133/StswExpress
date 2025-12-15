using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.ObjectModel;

namespace TestApp.Ava;
/// <summary>
/// Interaction logic for ControlsBase.axaml
/// </summary>
public partial class ControlsBase : UserControl
{
    public ControlsBase()
    {
        AvaloniaXamlLoader.Load(this);
        SetValue(PropertiesProperty, []);
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the description text.
    /// </summary>
    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    public static readonly StyledProperty<string?> DescriptionProperty = AvaloniaProperty.Register<ControlsBase, string?>(nameof(Description));

    /// <summary>
    /// Gets or sets a value indicating whether the content alignment options are visible.
    /// </summary>
    public bool IsContentAlignmentVisible
    {
        get => GetValue(IsContentAlignmentVisibleProperty);
        set => SetValue(IsContentAlignmentVisibleProperty, value);
    }
    public static readonly StyledProperty<bool> IsContentAlignmentVisibleProperty = AvaloniaProperty.Register<ControlsBase, bool>(nameof(IsContentAlignmentVisible));

    /// <summary>
    /// Gets or sets the collection of property controls.
    /// </summary>
    public ObservableCollection<Control>? Properties
    {
        get => GetValue(PropertiesProperty);
        set => SetValue(PropertiesProperty, value);
    }
    public static readonly StyledProperty<ObservableCollection<Control>?> PropertiesProperty = AvaloniaProperty.Register<ControlsBase, ObservableCollection<Control>?>(nameof(Properties));

    /// <summary>
    /// Gets or sets the status panel control.
    /// </summary>
    public Control? StatusPanel
    {
        get => GetValue(StatusPanelProperty);
        set => SetValue(StatusPanelProperty, value);
    }
    public static readonly StyledProperty<Control?> StatusPanelProperty = AvaloniaProperty.Register<ControlsBase, Control?>(nameof(StatusPanel));
    #endregion
}
