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

/// <summary>
/// Represents a navigation control that allows managing multiple contexts and navigation elements.
/// </summary>
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

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        if (IsExtended == default)
            OnIsExtendedChanged(this, new DependencyPropertyChangedEventArgs());

        base.OnApplyTemplate();
    }

    /// <summary>
    /// Changes the current context and optionally creates a new instance of the context object.
    /// </summary>
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
    /// <summary>
    /// Gets the collection of contexts associated with this navigation control.
    /// </summary>
    public StswDictionary<string, object?> Contexts
    {
        get => (StswDictionary<string, object?>)GetValue(ContextsProperty);
        internal set => SetValue(ContextsProperty, value);
    }
    public static readonly DependencyProperty ContextsProperty
        = DependencyProperty.Register(
            nameof(Contexts),
            typeof(StswDictionary<string, object?>),
            typeof(StswNavigation)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the navigation is in the extended mode.
    /// </summary>
    public bool IsExtended
    {
        get => (bool)GetValue(IsExtendedProperty);
        set => SetValue(IsExtendedProperty, value);
    }
    public static readonly DependencyProperty IsExtendedProperty
        = DependencyProperty.Register(
            nameof(IsExtended),
            typeof(bool),
            typeof(StswNavigation),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsExtendedChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnIsExtendedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNavigation stsw)
        {
            stsw.Items.ToList().ForEach(x => x.IsCompact = !stsw.IsExtended || stsw.ItemsAlignment.In(Dock.Top, Dock.Bottom));
            stsw.ItemsPinned.ToList().ForEach(x => x.IsCompact = !stsw.IsExtended || stsw.ItemsAlignment.In(Dock.Top, Dock.Bottom));
        }
    }

    /// <summary>
    /// Gets or sets the name of the navigation group (used for radio buttons).
    /// </summary>
    public string? GroupName
    {
        get => (string?)GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }
    public static readonly DependencyProperty GroupNameProperty
        = DependencyProperty.Register(
            nameof(GroupName),
            typeof(string),
            typeof(StswNavigation),
            new PropertyMetadata(Guid.NewGuid().ToString())
        );

    /// <summary>
    /// Gets or sets the collection of navigation elements.
    /// </summary>
    public ObservableCollection<StswNavigationElement> Items
    {
        get => (ObservableCollection<StswNavigationElement>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }
    public static readonly DependencyProperty ItemsProperty
        = DependencyProperty.Register(
            nameof(Items),
            typeof(ObservableCollection<StswNavigationElement>),
            typeof(StswNavigation)
        );

    /// <summary>
    /// Gets or sets the alignment of navigation elements.
    /// </summary>
    public Dock ItemsAlignment
    {
        get => (Dock)GetValue(ItemsAlignmentProperty);
        set => SetValue(ItemsAlignmentProperty, value);
    }
    public static readonly DependencyProperty ItemsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ItemsAlignment),
            typeof(Dock),
            typeof(StswNavigation),
            new FrameworkPropertyMetadata(default(Dock),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsExtendedChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );

    /// <summary>
    /// Gets or sets the collection of pinned navigation elements.
    /// </summary>
    public ObservableCollection<StswNavigationElement> ItemsPinned
    {
        get => (ObservableCollection<StswNavigationElement>)GetValue(ItemsPinnedProperty);
        set => SetValue(ItemsPinnedProperty, value);
    }
    public static readonly DependencyProperty ItemsPinnedProperty
        = DependencyProperty.Register(
            nameof(ItemsPinned),
            typeof(ObservableCollection<StswNavigationElement>),
            typeof(StswNavigation)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the degree to which the corners of the control are rounded.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswNavigation)
        );

    /// <summary>
    /// Gets or sets the width of the navigation items list.
    /// </summary>
    public double ItemsWidth
    {
        get => (double)GetValue(ItemsWidthProperty);
        set => SetValue(ItemsWidthProperty, value);
    }
    public static readonly DependencyProperty ItemsWidthProperty
        = DependencyProperty.Register(
            nameof(ItemsWidth),
            typeof(double),
            typeof(StswNavigation)
        );

    /// <summary>
    /// Gets or sets the thickness of the border used as separator between content and items list.
    /// </summary>
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswNavigation)
        );
    #endregion
}
