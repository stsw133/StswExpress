using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        if (DesignerProperties.GetIsInDesignMode(this)) return;

        /// selecting first visible and enabled button from navigation
        Loaded += (s, e) =>
        {
            var checkedButton = (Buttons.FirstOrDefault(x => x is StswNavigationButton r && r.IsChecked == true) as StswNavigationButton);
            var button = checkedButton ?? Buttons.FirstOrDefault(x => x is StswNavigationButton r && r.IsVisible && r.IsEnabled) as StswNavigationButton;
            if (button != null)
            {
                StswNavigationButton_Click(button, new RoutedEventArgs());
                button.IsChecked = true;
            }
        };
    }
    static StswNavigation()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigation), new FrameworkPropertyMetadata(typeof(StswNavigation)));
    }

    #region Properties
    /// Buttons
    public static readonly DependencyProperty ButtonsProperty
        = DependencyProperty.Register(
            nameof(Buttons),
            typeof(ObservableCollection<UIElement>),
            typeof(StswNavigation),
            new PropertyMetadata(default(ObservableCollection<UIElement>))
        );
    public ObservableCollection<UIElement> Buttons
    {
        get => (ObservableCollection<UIElement>)GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }

    /// ButtonsAlignment
    public static readonly DependencyProperty ButtonsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ButtonsAlignment),
            typeof(Dock),
            typeof(StswNavigation),
            new PropertyMetadata(default(Dock))
        );
    public Dock ButtonsAlignment
    {
        get => (Dock)GetValue(ButtonsAlignmentProperty);
        set => SetValue(ButtonsAlignmentProperty, value);
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

    /// FrameVisibility
    public static readonly DependencyProperty FrameVisibilityProperty
        = DependencyProperty.Register(
            nameof(FrameVisibility),
            typeof(Visibility),
            typeof(StswNavigation),
            new PropertyMetadata(default(Visibility))
        );
    public Visibility FrameVisibility
    {
        get => (Visibility)GetValue(FrameVisibilityProperty);
        set => SetValue(FrameVisibilityProperty, value);
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

    /// ...
    private readonly Dictionary<string, object> _contexts = new();
    
    public object? PageChange(object parameter, bool createNewInstance)
    {
        if (DesignerProperties.GetIsInDesignMode(this))
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

    #region Events
    /// StswNavigationButton_Click
    private void StswNavigationButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is StswNavigationButton button)
            PageChange(button.PageNamespace, button.CreateNewInstance);
    }
    #endregion
}
