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
public partial class StswNavigation : StswNavigationBase
{
    public StswNavigation()
    {
        InitializeComponent();
    }
    static StswNavigation()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigation), new FrameworkPropertyMetadata(typeof(StswNavigation)));
    }
}

public class StswNavigationBase : UserControl, INotifyPropertyChanged
{
    public StswNavigationBase()
    {
        SetValue(ButtonsProperty, new ObservableCollection<UIElement>());
        SetValue(PagesProperty, new StswDictionary<string, Page?>());

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

    /// Buttons
    public static readonly DependencyProperty ButtonsProperty
        = DependencyProperty.Register(
            nameof(Buttons),
            typeof(ObservableCollection<UIElement>),
            typeof(StswNavigationBase),
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
            typeof(StswNavigationBase),
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
            typeof(StswNavigationBase),
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
            typeof(StswNavigationBase),
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
            typeof(StswNavigationBase),
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
            typeof(StswNavigationBase),
            new PropertyMetadata(Orientation.Vertical)
        );
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// Pages
    public static readonly DependencyProperty PagesProperty
        = DependencyProperty.Register(
            nameof(Pages),
            typeof(StswDictionary<string, Page?>),
            typeof(StswNavigationBase),
            new PropertyMetadata(default(StswDictionary<string, Page?>))
        );
    public StswDictionary<string, Page?> Pages
    {
        get => (StswDictionary<string, Page?>)GetValue(PagesProperty);
        set => SetValue(PagesProperty, value);
    }

    /// ...
    private Frame? partFrame;

    public object? PageChange(string parameter, bool createNewInstance)
    {
        if (DesignerProperties.GetIsInDesignMode(this))
            return null;

        if (string.IsNullOrEmpty(parameter))
            return Content = null;

        Cursor = Cursors.Wait;

        if (partFrame != null && partFrame.BackStack != null)
            partFrame.RemoveBackEntry();
        if (createNewInstance && Pages.ContainsKey(parameter))
            Pages.Remove(parameter);
        if (!Pages.ContainsKey(parameter))
            Pages.Add(new KeyValuePair<string, Page?>(parameter, Activator.CreateInstance(Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty, parameter)?.Unwrap() as Page));
        Content = Pages[parameter];

        Cursor = null;

        return Content;
    }

    /// StswNavigationButton_Click
    private void StswNavigationButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is StswNavigationButton button)
            PageChange(button.PageNamespace, button.CreateNewInstance);
    }

    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        partFrame = GetTemplateChild("PART_Frame") as Frame;
        base.OnApplyTemplate();
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    public void NotifyPropertyChanged([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
