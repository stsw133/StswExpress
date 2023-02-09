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
        SetValue(ButtonsProperty, new ObservableCollection<UIElement>());
    }
    static StswNavigation() => DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigation), new FrameworkPropertyMetadata(typeof(StswNavigation)));
}

public class StswNavigationBase : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public void NotifyPropertyChanged([CallerMemberName] string name = "none passed") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public StswNavigationBase()
    {
        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        Loaded += (s, e) =>
        {
            var checkedButton = (Buttons.FirstOrDefault(x => x is StswNavigationButton r && r.IsChecked == true) as StswNavigationButton);
            var button = checkedButton != null ? checkedButton : (Buttons.FirstOrDefault(x => x is StswNavigationButton r && r.IsVisible && r.IsEnabled) as StswNavigationButton);
            if (button != null)
            {
                StswNavigationButton_Click(button, null);
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

    /// Pages (main logic)
    private Frame? naviFrame;

    private ExtDictionary<string, Page?> pages = new();
    public ExtDictionary<string, Page?> Pages
    {
        get => pages;
        set
        {
            pages = value;
            NotifyPropertyChanged();
        }
    }

    public void PageChange(string parameter, bool createNewInstance)
    {
        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        if (string.IsNullOrEmpty(parameter))
        {
            Content = null;
            return;
        }

        Cursor = Cursors.Wait;

        if (naviFrame != null && naviFrame.BackStack != null)
            naviFrame.RemoveBackEntry();
        if (createNewInstance && Pages.ContainsKey(parameter))
            Pages.Remove(parameter);
        if (!Pages.ContainsKey(parameter))
            Pages.Add(new KeyValuePair<string, Page?>(parameter, Activator.CreateInstance(Assembly.GetEntryAssembly()?.GetName().Name, parameter)?.Unwrap() as Page));
        Content = pages[parameter];

        Cursor = null;
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
        naviFrame = GetTemplateChild("NaviFrame") as Frame;
        base.OnApplyTemplate();
    }
}
