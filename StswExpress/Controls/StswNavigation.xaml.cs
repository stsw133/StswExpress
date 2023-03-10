using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// Interaction logic for StswNavigation.xaml
/// </summary>
public partial class StswNavigation : UserControl
{
    public StswNavigation()
    {
        InitializeComponent();

        SetValue(ButtonsProperty, new ObservableCollection<UIElement>());
        SetValue(ButtonsLastProperty, new ObservableCollection<UIElement>());
    }
    static StswNavigation()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigation), new FrameworkPropertyMetadata(typeof(StswNavigation)));
    }

    #region Events
    /// PART_Buttons_Loaded
    private void PART_Buttons_Loaded(object sender, RoutedEventArgs e)
    {
        var allButtons = new List<UIElement>();
        allButtons.AddRange(Buttons);
        allButtons.AddRange(ButtonsLast);

        var checkedButton = (allButtons.FirstOrDefault(x => x is StswNavigationElement r && r.IsChecked == true) as StswNavigationElement);
        var button = checkedButton ?? allButtons.FirstOrDefault(x => x is StswNavigationElement r && r.IsVisible && r.IsEnabled) as StswNavigationElement;

        if (button?.HasContent == true)
            button = ((Panel)button.Content).Children.OfType<StswNavigationElement>().FirstOrDefault(r => r.IsVisible && r.IsEnabled);

        if (button != null)
        {
            StswNavigationElement_Click(button, new RoutedEventArgs());
            button.IsChecked = true;
        }
    }

    /// StswNavigationElement_Click
    private void StswNavigationElement_Click(object sender, RoutedEventArgs e)
    {
        if (sender is StswNavigationElement stsw)
            PageChange(stsw.ContextNamespace, stsw.CreateNewInstance);
    }

    /// ...
    private readonly Dictionary<string, object> _contexts = new();

    public object? PageChange(object parameter, bool createNewInstance)
    {
        if (DesignerProperties.GetIsInDesignMode(this) || parameter is null)
            return null;

        object? newContent = null;
        Cursor = Cursors.Wait;

        if (parameter is string name1)
        {
            if (createNewInstance || !_contexts.ContainsKey(name1))
            {
                if (_contexts.ContainsKey(name1))
                    _contexts.Remove(name1);
                _contexts.Add(name1, Activator.CreateInstance(Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty, name1)?.Unwrap());
            }
            newContent = _contexts[name1];
        }
        else if (parameter?.GetType()?.FullName is string name2 && parameter?.GetType()?.IsValueType == false)
        {
            if (createNewInstance || !_contexts.ContainsKey(name2))
            {
                if (_contexts.ContainsKey(name2))
                    _contexts.Remove(name2);
                _contexts.Add(name2, parameter);
            }
            newContent = _contexts[name2];
        }
        else newContent = null;

        Content = newContent;

        Cursor = null;
        return newContent;
    }
    #endregion

    #region Properties
    /// Buttons
    public static readonly DependencyProperty ButtonsProperty
        = DependencyProperty.Register(
            nameof(Buttons),
            typeof(ObservableCollection<UIElement>),
            typeof(StswNavigation)
        );
    public ObservableCollection<UIElement> Buttons
    {
        get => (ObservableCollection<UIElement>)GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }
    /// ButtonsLast
    public static readonly DependencyProperty ButtonsLastProperty
        = DependencyProperty.Register(
            nameof(ButtonsLast),
            typeof(ObservableCollection<UIElement>),
            typeof(StswNavigation)
        );
    public ObservableCollection<UIElement> ButtonsLast
    {
        get => (ObservableCollection<UIElement>)GetValue(ButtonsLastProperty);
        set => SetValue(ButtonsLastProperty, value);
    }

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswNavigation),
            new PropertyMetadata(default(CornerRadius))
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// ExtendedMode
    public static readonly DependencyProperty ExtendedModeProperty
        = DependencyProperty.Register(
            nameof(ExtendedMode),
            typeof(bool),
            typeof(StswNavigation),
            new PropertyMetadata(default(bool))
        );
    public bool ExtendedMode
    {
        get => (bool)GetValue(ExtendedModeProperty);
        set => SetValue(ExtendedModeProperty, value);
    }

    /// Orientation
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StswNavigation),
            new PropertyMetadata(Orientation.Vertical)
        );
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    #endregion

    #region Style
    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswNavigation)
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    #endregion
}
