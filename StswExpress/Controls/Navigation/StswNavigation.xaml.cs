using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Represents a navigation control that allows managing multiple contexts and navigation elements.
/// </summary>
[ContentProperty(nameof(Items))]
public class StswNavigation : ContentControl, IStswCornerControl
{
    public StswNavigation()
    {
        SetValue(ComponentsProperty, new ObservableCollection<UIElement>());
        SetValue(ContextsProperty, new StswDictionary<string, object?>());
        SetValue(ItemsProperty, new ObservableCollection<StswNavigationElement>());
        SetValue(ItemsCompactProperty, new ObservableCollection<StswNavigationElement>());
        SetValue(ItemsPinnedProperty, new ObservableCollection<StswNavigationElement>());
    }
    static StswNavigation()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigation), new FrameworkPropertyMetadata(typeof(StswNavigation)));
    }

    #region Events & methods
    internal StswNavigationElement? CompactedExpander;
    
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_TabStripModeButton") is ToggleButton tabStripModeButton)
            tabStripModeButton.Click += BtnTabStripMode_Click;
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
        else if (context.GetType().FullName is string name2 && !context.GetType().IsValueType)
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnTabStripMode_Click(object sender, RoutedEventArgs e)
    {
        if (TabStripMode == StswToolbarMode.Full)
            TabStripMode = StswToolbarMode.Compact;
        else
            TabStripMode = StswToolbarMode.Full;
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the collection of UI elements used in the custom window's title bar.
    /// </summary>
    public ObservableCollection<UIElement> Components
    {
        get => (ObservableCollection<UIElement>)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }
    public static readonly DependencyProperty ComponentsProperty
        = DependencyProperty.Register(
            nameof(Components),
            typeof(ObservableCollection<UIElement>),
            typeof(StswNavigation)
        );

    /// <summary>
    /// Gets the collection of contexts associated with this navigation control.
    /// </summary>
    public StswDictionary<string, object?> Contexts
    {
        get => (StswDictionary<string, object?>)GetValue(ContextsProperty);
        set => SetValue(ContextsProperty, value);
    }
    public static readonly DependencyProperty ContextsProperty
        = DependencyProperty.Register(
            nameof(Contexts),
            typeof(StswDictionary<string, object?>),
            typeof(StswNavigation)
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
    /// Gets or sets the collection of navigation elements.
    /// </summary>
    public ObservableCollection<StswNavigationElement> ItemsCompact
    {
        get => (ObservableCollection<StswNavigationElement>)GetValue(ItemsCompactProperty);
        internal set => SetValue(ItemsCompactProperty, value);
    }
    public static readonly DependencyProperty ItemsCompactProperty
        = DependencyProperty.Register(
            nameof(ItemsCompact),
            typeof(ObservableCollection<StswNavigationElement>),
            typeof(StswNavigation)
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

    /// <summary>
    /// Gets or sets the last selected independent item.
    /// </summary>
    internal StswNavigationElement LastSelectedItem
    {
        get => (StswNavigationElement)GetValue(LastSelectedItemProperty);
        set => SetValue(LastSelectedItemProperty, value);
    }
    public static readonly DependencyProperty LastSelectedItemProperty
        = DependencyProperty.Register(
            nameof(LastSelectedItem),
            typeof(StswNavigationElement),
            typeof(StswNavigation),
            new FrameworkPropertyMetadata(default(StswNavigationElement),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnLastSelectedItemChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnLastSelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNavigation stsw)
        {
            var oldItem = e.OldValue as StswNavigationElement;
            var newItem = e.NewValue as StswNavigationElement;

            if (oldItem == newItem)
                return;

            if (!stsw.isLastSelectedItemChanging)
            {
                stsw.isLastSelectedItemChanging = true;

                if (oldItem != null)
                    oldItem.IsChecked = false;
                if (newItem != null)
                    newItem.IsChecked = true;

                stsw.isLastSelectedItemChanging = false;
            }
        }
    }
    bool isLastSelectedItemChanging;

    /// <summary>
    /// Gets or sets a value indicating whether the navigation shows elements and their names.
    /// </summary>
    public StswToolbarMode TabStripMode
    {
        get => (StswToolbarMode)GetValue(TabStripModeProperty);
        set => SetValue(TabStripModeProperty, value);
    }
    public static readonly DependencyProperty TabStripModeProperty
        = DependencyProperty.Register(
            nameof(TabStripMode),
            typeof(StswToolbarMode),
            typeof(StswNavigation),
            new FrameworkPropertyMetadata(default(StswToolbarMode),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTabStripModeChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnTabStripModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNavigation stsw)
        {
            /// get back all items from compact panel into original expander
            if (stsw.CompactedExpander != null && stsw.ItemsCompact.Count > 0)
            {
                if (stsw.TabStripMode == StswToolbarMode.Full)
                {
                    stsw.CompactedExpander.Items.Clear();
                    foreach (StswNavigationElement item in stsw.ItemsCompact.Clone())
                        stsw.CompactedExpander.Items.Add(item);
                }
                else if (stsw.TabStripMode == StswToolbarMode.Compact)
                {
                    stsw.ItemsCompact.Clear();
                    foreach (StswNavigationElement item in stsw.CompactedExpander.Items.Clone())
                        stsw.ItemsCompact.Add(item);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the alignment of navigation elements.
    /// </summary>
    public Dock TabStripPlacement
    {
        get => (Dock)GetValue(TabStripPlacementProperty);
        set => SetValue(TabStripPlacementProperty, value);
    }
    public static readonly DependencyProperty TabStripPlacementProperty
        = DependencyProperty.Register(
            nameof(TabStripPlacement),
            typeof(Dock),
            typeof(StswNavigation)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match the
    /// border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswNavigation)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
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
    /// Gets or sets the thickness of the separator between items and content.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty
        = DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswNavigation)
        );

    /// <summary>
    /// Gets or sets the width of the navigation items list.
    /// </summary>
    public double TabStripWidth
    {
        get => (double)GetValue(TabStripWidthProperty);
        set => SetValue(TabStripWidthProperty, value);
    }
    public static readonly DependencyProperty TabStripWidthProperty
        = DependencyProperty.Register(
            nameof(TabStripWidth),
            typeof(double),
            typeof(StswNavigation)
        );
    #endregion
}
