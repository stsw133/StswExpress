using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace TestApp;
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

    #region Main properties
    /// <summary>
    /// 
    /// </summary>
    public Visibility ContentAlignmentVisibility
    {
        get => (Visibility)GetValue(ContentAlignmentVisibilityProperty);
        set => SetValue(ContentAlignmentVisibilityProperty, value);
    }
    public static readonly DependencyProperty ContentAlignmentVisibilityProperty
        = DependencyProperty.Register(
            nameof(ContentAlignmentVisibility),
            typeof(Visibility),
            typeof(ControlsBase)
        );

    /// <summary>
    /// 
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
    /// 
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
    /// 
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
    #endregion
}
