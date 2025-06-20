using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A navigation control that manages multiple contexts and navigation elements.
/// Supports pinned items, compact/full modes, and dynamic content switching.
/// </summary>
[ContentProperty(nameof(Items))]
public class StswNavigation : ContentControl, IStswCornerControl
{
    internal StswNavigationElement? CompactedExpander;

    public StswNavigation()
    {
        SetValue(ComponentsProperty, new ObservableCollection<UIElement>());
        SetValue(ContextsProperty, new StswObservableDictionary<string, object?>());
        SetValue(ItemsProperty, new ObservableCollection<StswNavigationElement>());
        SetValue(ItemsCompactProperty, new ObservableCollection<StswNavigationElement>());
        SetValue(ItemsPinnedProperty, new ObservableCollection<StswNavigationElement>());
    }
    static StswNavigation()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigation), new FrameworkPropertyMetadata(typeof(StswNavigation)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_TabStripModeButton") is ToggleButton tabStripModeButton)
            tabStripModeButton.Click += PART_TabStripModeButton_Click;
    }

    /// <summary>
    /// Changes the current context and optionally creates a new instance of the context object.
    /// Supports switching between different views dynamically.
    /// </summary>
    /// <param name="context">The context to switch to, either as a type name or an object instance.</param>
    /// <param name="createNewInstance">Determines whether a new instance should be created.</param>
    /// <returns>The newly assigned content.</returns>
    public object? ChangeContext(object context, bool createNewInstance)
    {
        if (DesignerProperties.GetIsInDesignMode(this) || context is null)
            return null;

        if (context is string name1)
        {
            if (createNewInstance || !Contexts.TryGetValue(name1, out var value))
            {
                if (Contexts.ContainsKey(name1))
                    Contexts.Remove(name1);
                value = (Activator.CreateInstance(Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty, name1)?.Unwrap());
                Contexts.Add(name1, value);
            }
            return Content = value;
        }
        else if (context.GetType().FullName is string name2 && !context.GetType().IsValueType)
        {
            if (createNewInstance || !Contexts.TryGetValue(name2, out var value))
            {
                if (Contexts.ContainsKey(name2))
                    Contexts.Remove(name2);
                value = context;
                Contexts.Add(name2, value);
            }
            return Content = value;
        }
        return Content = null;
    }

    /// <summary>
    /// Handles the click event for toggling between compact and full tab strip modes.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    private void PART_TabStripModeButton_Click(object sender, RoutedEventArgs e)
    {
        if (TabStripMode == StswCompactibility.Full)
            TabStripMode = StswCompactibility.Compact;
        else
            TabStripMode = StswCompactibility.Full;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the collection of UI elements used in the custom window's title bar.
    /// Allows adding extra controls such as buttons, search fields, or indicators.
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
    /// Each context represents a separate view that can be dynamically switched.
    /// </summary>
    public StswObservableDictionary<string, object?> Contexts
    {
        get => (StswObservableDictionary<string, object?>)GetValue(ContextsProperty);
        set => SetValue(ContextsProperty, value);
    }
    public static readonly DependencyProperty ContextsProperty
        = DependencyProperty.Register(
            nameof(Contexts),
            typeof(StswObservableDictionary<string, object?>),
            typeof(StswNavigation)
        );

    /// <summary>
    /// Gets or sets the collection of navigation elements displayed in the control.
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
    /// Gets or sets the collection of navigation elements when the control is in compact mode.
    /// Items are displayed in a more condensed form.
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
    /// Pinned items remain accessible regardless of mode changes.
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
    /// Ensures that only one item remains selected at a time.
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
    /// Controls the navigation layout between compact and full modes.
    /// </summary>
    public StswCompactibility TabStripMode
    {
        get => (StswCompactibility)GetValue(TabStripModeProperty);
        set => SetValue(TabStripModeProperty, value);
    }
    public static readonly DependencyProperty TabStripModeProperty
        = DependencyProperty.Register(
            nameof(TabStripMode),
            typeof(StswCompactibility),
            typeof(StswNavigation),
            new FrameworkPropertyMetadata(default(StswCompactibility),
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
                if (stsw.TabStripMode == StswCompactibility.Full)
                {
                    stsw.CompactedExpander.Items.Clear();
                    foreach (StswNavigationElement item in stsw.ItemsCompact.TryClone())
                        stsw.CompactedExpander.Items.Add(item);
                }
                else if (stsw.TabStripMode == StswCompactibility.Compact)
                {
                    stsw.ItemsCompact.Clear();
                    foreach (StswNavigationElement item in stsw.CompactedExpander.Items.TryClone())
                        stsw.ItemsCompact.Add(item);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the alignment of navigation elements.
    /// Determines the placement of the tab strip within the control.
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
    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswNavigation),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswNavigation),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between items and content.
    /// Affects the spacing and visual separation in the navigation layout.
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
            typeof(StswNavigation),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the width of the navigation items list.
    /// Adjusts the size of the tab strip for a custom layout.
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
            typeof(StswNavigation),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsMeasure)
        );
    #endregion
}

/* usage:

<se:StswNavigation TabStripMode="Full">
    <se:StswNavigationElement Header="Dashboard"/>
    <se:StswNavigationElement Header="Settings"/>
</se:StswNavigation>

*/
