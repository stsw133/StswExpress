using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StswExpress;

public class StswNavigation : UserControl
{
    public StswNavigation()
    {
        SetValue(ButtonsProperty, new ObservableCollection<UIElement>());
        SetValue(ButtonsLastProperty, new ObservableCollection<UIElement>());
    }
    static StswNavigation()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigation), new FrameworkPropertyMetadata(typeof(StswNavigation)));
    }

    #region Events
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        /// Button: minimize
        if (GetTemplateChild("PART_MainBorder") is Border mainBorder)
            mainBorder.Loaded += PART_Buttons_Loaded;

        base.OnApplyTemplate();
    }

    /// PART_Buttons_Loaded
    private void PART_Buttons_Loaded(object sender, RoutedEventArgs e)
    {
        /// get ALL StswNavigationElements in StswNavigation
        var allNaviElements = new List<StswNavigationElement>();
        allNaviElements.AddRange(Buttons.OfType<StswNavigationElement>());
        allNaviElements.AddRange(ButtonsLast.OfType<StswNavigationElement>());

        while (allNaviElements.Any(x => x.HasContent))
        {
            var elementsWithContents = allNaviElements.Where(x => x.HasContent).ToList();
            foreach (var naviElement in elementsWithContents)
                allNaviElements.AddRange(StswFn.FindVisualChildren<StswNavigationElement>((FrameworkElement)naviElement.Content));
            allNaviElements.RemoveAll(x => elementsWithContents.Contains(x));
        }
        
        /// for every StswNavigationElement assign click event
        foreach (var naviElement in allNaviElements)
            naviElement.Click += StswNavigationElement_Click;

        /// find any checked elements and if none is checked then check first that is visible and enabled
        var firstCheckedElement = allNaviElements.FirstOrDefault(x => x.IsChecked == true) ?? allNaviElements.FirstOrDefault(x => x.IsVisible && x.IsEnabled);
        if (firstCheckedElement != null)
        {
            StswNavigationElement_Click(firstCheckedElement, new RoutedEventArgs());
            firstCheckedElement.IsChecked = true;
        }
    }

    /// StswNavigationElement_Click
    private async void StswNavigationElement_Click(object sender, RoutedEventArgs e)
    {
        if (sender is StswNavigationElement stsw)
        {
            stsw.IsBusy = true;
            await Task.Run(() => Thread.Sleep(50));
            ContextChange(stsw.ContextNamespace, stsw.CreateNewInstance);
            stsw.IsBusy = false;
        }
    }

    /// ...
    private readonly StswDictionary<string, object> _contexts = new();

    public object? ContextChange(object parameter, bool createNewInstance)
    {
        if (DesignerProperties.GetIsInDesignMode(this) || parameter is null)
            return null;

        object? newContent;

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
            typeof(StswNavigation)
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
            typeof(StswNavigation)
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
            typeof(StswNavigation)
        );
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    #endregion

    #region Style
    /// > BorderThickness ...
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
