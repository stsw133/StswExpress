using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

[ContentProperty(nameof(Items))]
public class StswNavigation : UserControl
{
    public StswNavigation()
    {
        SetValue(ContextsProperty, new StswDictionary<string, object?>());
        SetValue(ItemsProperty, new ObservableCollection<StswNavigationElement>());
        SetValue(ItemsPinnedProperty, new ObservableCollection<StswNavigationElement>());
    }
    static StswNavigation()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigation), new FrameworkPropertyMetadata(typeof(StswNavigation)));
    }

    #region Events
    /*
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        OnItemsChanged(this, new DependencyPropertyChangedEventArgs());

        base.OnApplyTemplate();
    }
    */
    /// ChangeContext
    public object? ChangeContext(object context, bool createNewInstance)
    {
        if (DesignerProperties.GetIsInDesignMode(this) || context is null)
            return null;

        if (context is string name1)
        {
            if (createNewInstance || !Contexts.ContainsKey(name1))
            {
                if (Contexts.ContainsKey(name1))
                    Contexts.Remove(name1);
                Contexts.Add(name1, Activator.CreateInstance(Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty, name1)?.Unwrap());
            }
            return Content = Contexts[name1];
        }
        else if (context.GetType().FullName is string name2 && context.GetType().IsValueType == false)
        {
            if (createNewInstance || !Contexts.ContainsKey(name2))
            {
                if (Contexts.ContainsKey(name2))
                    Contexts.Remove(name2);
                Contexts.Add(name2, context);
            }
            return Content = Contexts[name2];
        }
        return Content = null;
    }
    #endregion

    #region Main properties
    /// Contexts
    public static readonly DependencyProperty ContextsProperty
        = DependencyProperty.Register(
            nameof(Contexts),
            typeof(StswDictionary<string, object?>),
            typeof(StswNavigation)
        );
    public StswDictionary<string, object?> Contexts
    {
        get => (StswDictionary<string, object?>)GetValue(ContextsProperty);
        internal set => SetValue(ContextsProperty, value);
    }

    /// IsExtended
    public static readonly DependencyProperty IsExtendedProperty
        = DependencyProperty.Register(
            nameof(IsExtended),
            typeof(bool),
            typeof(StswNavigation),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsExtendedChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public bool IsExtended
    {
        get => (bool)GetValue(IsExtendedProperty);
        set => SetValue(IsExtendedProperty, value);
    }
    public static void OnIsExtendedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNavigation stsw)
        {
            stsw.Items.ToList().ForEach(x => x.IsCompact = !stsw.IsExtended || stsw.ItemsAlignment.In(Dock.Top, Dock.Bottom));
            stsw.ItemsPinned.ToList().ForEach(x => x.IsCompact = !stsw.IsExtended || stsw.ItemsAlignment.In(Dock.Top, Dock.Bottom));
        }
    }

    /// GroupName
    public static readonly DependencyProperty GroupNameProperty
        = DependencyProperty.Register(
            nameof(GroupName),
            typeof(string),
            typeof(StswNavigation),
            new PropertyMetadata(Guid.NewGuid().ToString())
        );
    public string? GroupName
    {
        get => (string?)GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }

    /// Items
    public static readonly DependencyProperty ItemsProperty
        = DependencyProperty.Register(
            nameof(Items),
            typeof(ObservableCollection<StswNavigationElement>),
            typeof(StswNavigation)
        );
    public ObservableCollection<StswNavigationElement> Items
    {
        get => (ObservableCollection<StswNavigationElement>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }
    /// ItemsAlignment
    public static readonly DependencyProperty ItemsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ItemsAlignment),
            typeof(Dock),
            typeof(StswNavigation),
            new FrameworkPropertyMetadata(default(Dock),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsExtendedChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public Dock ItemsAlignment
    {
        get => (Dock)GetValue(ItemsAlignmentProperty);
        set => SetValue(ItemsAlignmentProperty, value);
    }
    /// ItemsPinned
    public static readonly DependencyProperty ItemsPinnedProperty
        = DependencyProperty.Register(
            nameof(ItemsPinned),
            typeof(ObservableCollection<StswNavigationElement>),
            typeof(StswNavigation)
        );
    public ObservableCollection<StswNavigationElement> ItemsPinned
    {
        get => (ObservableCollection<StswNavigationElement>)GetValue(ItemsPinnedProperty);
        set => SetValue(ItemsPinnedProperty, value);
    }
    #endregion

    #region Spatial properties
    /// > BorderThickness ...
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

    /// > CornerRadius ...
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
    #endregion
}
