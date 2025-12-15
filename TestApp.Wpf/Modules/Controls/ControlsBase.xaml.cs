using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace TestApp.Wpf;
/// <summary>
/// Interaction logic for ControlsBase.xaml
/// </summary>
public partial class ControlsBase : UserControl
{
    public ControlsBase()
    {
        InitializeComponent();
        SetValue(PropertiesProperty, new ObservableCollection<UIElement>());
    }

    /// <summary>
    /// Gets or sets the description text.
    /// </summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    public static readonly DependencyProperty DescriptionProperty
        = DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(ControlsBase)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the content alignment options are visible.
    /// </summary>
    public bool IsContentAlignmentVisible
    {
        get => (bool)GetValue(IsContentAlignmentVisibleProperty);
        set => SetValue(IsContentAlignmentVisibleProperty, value);
    }
    public static readonly DependencyProperty IsContentAlignmentVisibleProperty
        = DependencyProperty.Register(
            nameof(IsContentAlignmentVisible),
            typeof(bool),
            typeof(ControlsBase)
        );

    /// <summary>
    /// Gets or sets the collection of property elements.
    /// </summary>
    public ObservableCollection<UIElement> Properties
    {
        get => (ObservableCollection<UIElement>)GetValue(PropertiesProperty);
        set => SetValue(PropertiesProperty, value);
    }
    public static readonly DependencyProperty PropertiesProperty
        = DependencyProperty.Register(
            nameof(Properties),
            typeof(ObservableCollection<UIElement>),
            typeof(ControlsBase)
        );

    /// <summary>
    /// Gets or sets the status panel element.
    /// </summary>
    public UIElement StatusPanel
    {
        get => (UIElement)GetValue(StatusPanelProperty);
        set => SetValue(StatusPanelProperty, value);
    }
    public static readonly DependencyProperty StatusPanelProperty
        = DependencyProperty.Register(
            nameof(StatusPanel),
            typeof(UIElement),
            typeof(ControlsBase)
        );
}
