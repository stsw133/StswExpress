using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace StswExpress.Wpf;
/// <summary>
/// A navigation control that manages multiple contexts and navigation elements.
/// Supports pinned items, compact/full modes, and dynamic content switching.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswNavigation TabStripMode="Full"&gt;
///     &lt;se:StswNavigationItem Header="Dashboard"/&gt;
///     &lt;se:StswNavigationItem Header="Settings"/&gt;
/// &lt;/se:StswNavigation&gt;
/// </code>
/// </example>
public class StswNavigation : TreeView, IStswCornerControl
{
    static StswNavigation()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigation), new FrameworkPropertyMetadata(typeof(StswNavigation)));
    }
    public StswNavigation()
    {
        SetValue(ComponentsProperty, new ObservableCollection<UIElement>());
        SetValue(ContextsProperty, new StswObservableDictionary<string, object?>());
        SetValue(ItemsCompactProperty, new ObservableCollection<StswNavigationItem>());
        SetValue(ItemsPinnedProperty, new ObservableCollection<StswNavigationItem>());

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #region Dependency properties
    /// <summary>
    /// Gets or sets a value indicating whether to automatically scroll expanded items into view.
    /// </summary>
    public bool AutoScrollExpandedItemsIntoView
    {
        get => (bool)GetValue(AutoScrollExpandedItemsIntoViewProperty);
        set => SetValue(AutoScrollExpandedItemsIntoViewProperty, value);
    }
    public static readonly DependencyProperty AutoScrollExpandedItemsIntoViewProperty
        = DependencyProperty.Register(
            nameof(AutoScrollExpandedItemsIntoView),
            typeof(bool),
            typeof(StswNavigation)
        );

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

    /// <inheritdoc/>
    public object? Content
    {
        get => (object?)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
    public static readonly DependencyProperty ContentProperty = ContentControl.ContentProperty.AddOwner(typeof(StswNavigation));

    /// <summary>
    /// Gets or sets a string format applied to the <see cref="Content"/>.
    /// Useful for formatting text-based content.
    /// </summary>
    public string? ContentStringFormat
    {
        get => (string?)GetValue(ContentStringFormatProperty);
        set => SetValue(ContentStringFormatProperty, value);
    }
    public static readonly DependencyProperty ContentStringFormatProperty = ContentControl.ContentStringFormatProperty.AddOwner(typeof(StswNavigation));

    /// <summary>
    /// Gets or sets the data template used to display the <see cref="Content"/>.
    /// </summary>
    public DataTemplate? ContentTemplate
    {
        get => (DataTemplate?)GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }
    public static readonly DependencyProperty ContentTemplateProperty = ContentControl.ContentTemplateProperty.AddOwner(typeof(StswNavigation));

    /// <summary>
    /// Gets or sets a data template selector for the <see cref="Content"/>.
    /// Allows dynamic selection of templates based on content type.
    /// </summary>
    public DataTemplateSelector? ContentTemplateSelector
    {
        get => (DataTemplateSelector?)GetValue(ContentTemplateSelectorProperty);
        set => SetValue(ContentTemplateSelectorProperty, value);
    }
    public static readonly DependencyProperty ContentTemplateSelectorProperty = ContentControl.ContentTemplateSelectorProperty.AddOwner(typeof(StswNavigation));

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
            typeof(StswNavigation)
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
            typeof(StswNavigation)
        );

    /// <summary>
    /// Identifier used with <see cref="SetContent(object, bool, object?)"/> to locate a specific navigation instance.
    /// </summary>
    public object? Identifier
    {
        get => GetValue(IdentifierProperty);
        set => SetValue(IdentifierProperty, value);
    }
    public static readonly DependencyProperty IdentifierProperty
        = DependencyProperty.Register(
            nameof(Identifier),
            typeof(object),
            typeof(StswNavigation)
        );

    /// <summary>
    /// Gets or sets the collection of navigation elements when the control is in compact mode.
    /// Items are displayed in a more condensed form.
    /// </summary>
    public ObservableCollection<StswNavigationItem> ItemsCompact
    {
        get => (ObservableCollection<StswNavigationItem>)GetValue(ItemsCompactProperty);
        internal set => SetValue(ItemsCompactProperty, value);
    }
    public static readonly DependencyProperty ItemsCompactProperty
        = DependencyProperty.Register(
            nameof(ItemsCompact),
            typeof(ObservableCollection<StswNavigationItem>),
            typeof(StswNavigation)
        );

    /// <summary>
    /// Gets or sets the collection of pinned navigation elements.
    /// Pinned items remain accessible regardless of mode changes.
    /// </summary>
    public ObservableCollection<StswNavigationItem> ItemsPinned
    {
        get => (ObservableCollection<StswNavigationItem>)GetValue(ItemsPinnedProperty);
        set => SetValue(ItemsPinnedProperty, value);
    }
    public static readonly DependencyProperty ItemsPinnedProperty
        = DependencyProperty.Register(
            nameof(ItemsPinned),
            typeof(ObservableCollection<StswNavigationItem>),
            typeof(StswNavigation)
        );

    /// <summary>
    /// Gets or sets the last selected independent item.
    /// Ensures that only one item remains selected at a time.
    /// </summary>
    internal StswNavigationItem LastSelectedItem
    {
        get => (StswNavigationItem)GetValue(LastSelectedItemProperty);
        set => SetValue(LastSelectedItemProperty, value);
    }
    public static readonly DependencyProperty LastSelectedItemProperty
        = DependencyProperty.Register(
            nameof(LastSelectedItem),
            typeof(StswNavigationItem),
            typeof(StswNavigation),
            new FrameworkPropertyMetadata(default(StswNavigationItem),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnLastSelectedItemChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnLastSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswNavigation)d;

        var oldItem = e.OldValue as StswNavigationItem;
        var newItem = e.NewValue as StswNavigationItem;

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
    bool isLastSelectedItemChanging;

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
    public static void OnTabStripModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswNavigation)d;

        /// get back all items from compact panel into original expander
        if (stsw.TabStripMode != StswCompactibility.Compact)
        {
            stsw.RestoreCompactItems();
        }
        else if (stsw.CompactedExpander != null)
        {
            stsw.ItemsCompact.Clear();
            foreach (StswNavigationItem item in stsw.CompactedExpander.Items.TryClone())
            {
                item.IsInCompactPanel = true;
                stsw.ItemsCompact.Add(item);
            }
        }

        stsw.AnimateTabStripModeTransition((StswCompactibility)e.OldValue, stsw.TabStripMode);
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

    #region Template
    private static readonly HashSet<WeakReference<StswNavigation>> _loadedInstances = [];
    private ToggleButton? _tabStripModeButton;
    private FrameworkElement? _mainPanel;
    internal StswNavigationItem? CompactedExpander;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        DetachTemplateEvents();
        _mainPanel = GetTemplateChild("OPT_MainPanel") as FrameworkElement;
        _tabStripModeButton = GetTemplateChild("PART_TabStripModeButton") as ToggleButton;
        AttachTemplateEvents();
    }

    /// <summary>
    /// Attaches event handlers to the template parts.
    /// </summary>
    private void AttachTemplateEvents()
    {
        if (_tabStripModeButton != null)
            _tabStripModeButton.Click += PART_TabStripModeButton_Click;
    }

    /// <summary>
    /// Detaches event handlers from the template parts.
    /// </summary>
    private void DetachTemplateEvents()
    {
        if (_tabStripModeButton != null)
            _tabStripModeButton.Click -= PART_TabStripModeButton_Click;
    }

    /// <summary>
    /// Tracks loaded instances to enable identifier-based lookups.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        foreach (var weakRef in _loadedInstances.ToList())
            if (weakRef.TryGetTarget(out StswNavigation? navigation) && ReferenceEquals(navigation, this))
                return;

        _loadedInstances.Add(new WeakReference<StswNavigation>(this));
    }

    /// <summary>
    /// Removes unloaded instances to prevent memory leaks.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        foreach (var weakRef in _loadedInstances.ToList())
            if (!weakRef.TryGetTarget(out StswNavigation? navigation) || ReferenceEquals(navigation, this))
            {
                _loadedInstances.Remove(weakRef);
                break;
            }
    }
    #endregion

    #region Overrides
    /// <inheritdoc/>
    protected override DependencyObject GetContainerForItemOverride() => new StswNavigationItem();
    /// <inheritdoc/>
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswNavigationItem;
    #endregion

    #region Logic
    /// <summary>
    /// Handles the click event for toggling between compact and full tab strip modes.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    private void PART_TabStripModeButton_Click(object sender, RoutedEventArgs e)
        => TabStripMode = TabStripMode == StswCompactibility.Full
            ? StswCompactibility.Compact
            : StswCompactibility.Full;

    /// <summary>
    /// Finds the <see cref="StswNavigation"/> instance matching the provided identifier.
    /// </summary>
    /// <param name="identifier">The identifier used to locate the control.</param>
    /// <returns>The matching control instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no matching instance is found or when multiple matches are detected.</exception>
    internal static StswNavigation GetInstance(object? identifier)
    {
        if (_loadedInstances.Count == 0)
            throw new InvalidOperationException($"No loaded {nameof(StswNavigation)} instances.");

        var targets = new List<StswNavigation>();
        foreach (var instance in _loadedInstances.ToList())
        {
            if (instance.TryGetTarget(out var navigation))
            {
                object? navigationIdentifier = null;

                if (navigation.CheckAccess())
                    navigationIdentifier = navigation.Identifier;
                else navigationIdentifier = navigation.Dispatcher.Invoke(() => navigation.Identifier);

                if (Equals(identifier, navigationIdentifier))
                    targets.Add(navigation);
            }
            else _loadedInstances.Remove(instance);
        }

        if (targets.Count == 0)
            throw new InvalidOperationException($"No loaded {nameof(StswNavigation)} have an {nameof(Identifier)} property matching {nameof(identifier)} ('{identifier}') argument.");
        if (targets.Count > 1)
            throw new InvalidOperationException($"Multiple viable {nameof(StswNavigation)}s. Specify a unique Identifier on each {nameof(StswNavigation)}, especially where multiple Windows are a concern.");

        return targets[0];
    }

    /// <summary>
    /// Restores items placed into the compact panel back to their owning expander.
    /// </summary>
    internal void RestoreCompactItems()
    {
        if (CompactedExpander != null && ItemsCompact.Count > 0)
        {
            CompactedExpander.Items.Clear();
            foreach (StswNavigationItem item in ItemsCompact.ToList())
            {
                item.IsInCompactPanel = false;
                CompactedExpander.Items.Add(item);
            }
            ItemsCompact.Clear();
        }

        CompactedExpander = null;
    }

    /// <summary>
    /// Changes the current context and optionally creates a new instance of the context object.
    /// Supports switching between different views dynamically.
    /// </summary>
    /// <param name="context">The context to switch to, either as a type name or an object instance.</param>
    /// <param name="createNewInstance">Determines whether a new instance should be created.</param>
    /// <returns>The newly assigned content.</returns>
    public object? SetContent(object context, bool createNewInstance)
    {
        if (DesignerProperties.GetIsInDesignMode(this) || context is null)
            return null;

        var key = context switch
        {
            Type type => type.FullName,
            string name => name,
            _ when !context.GetType().IsValueType => context.GetType().FullName,
            _ => null
        };
        if (key is null)
            return Content = null;

        if (!createNewInstance && Contexts.TryGetValue(key, out var existingValue))
            return Content = existingValue;

        var value = context switch
        {
            Type type => StswDependencyInjectionHelper.Resolve(type),
            string name => StswDependencyInjectionHelper.Resolve(name),
            _ => context
        };

        Contexts.Remove(key);
        Contexts.Add(key, value);

        return Content = value;
    }

    /// <summary>
    /// Changes the context of a <see cref="StswNavigation"/> identified by <paramref name="identifier"/>.
    /// </summary>
    /// <param name="context">The context to switch to, either as a type name or an object instance.</param>
    /// <param name="createNewInstance">Determines whether a new instance should be created.</param>
    /// <param name="identifier">The <see cref="Identifier"/> used to locate the navigation control.</param>
    /// <returns>The newly assigned content.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no matching control is found or multiple matches exist.</exception>
    public static object? SetContent(object context, bool createNewInstance, object? identifier) => GetInstance(identifier).SetContent(context, createNewInstance);
    #endregion

    #region Animations
    /// <summary>
    /// Animates the tab strip panel width when switching between full and compact modes.
    /// </summary>
    /// <param name="oldMode">The previous tab strip mode.</param>
    /// <param name="newMode">The new tab strip mode.</param>
    private void AnimateTabStripModeTransition(StswCompactibility oldMode, StswCompactibility newMode)
    {
        if (oldMode == newMode || _mainPanel is not FrameworkElement mainPanel || !IsLoaded)
            return;

        if (!StswApp.Settings.AnimationsEnabled || !StswControl.GetEnableAnimations(this))
            return;

        var isAnimationRequested =
            oldMode == StswCompactibility.Full && newMode == StswCompactibility.Compact ||
            oldMode == StswCompactibility.Compact && newMode == StswCompactibility.Full;
        if (!isAnimationRequested)
            return;

        var startWidth = mainPanel.ActualWidth;
        if (double.IsNaN(startWidth) || startWidth <= 0)
            return;

        Dispatcher.InvokeAsync(() =>
        {
            if (_mainPanel is not FrameworkElement targetPanel)
                return;

            targetPanel.UpdateLayout();

            var targetWidth = targetPanel.DesiredSize.Width;
            if (newMode == StswCompactibility.Full && !double.IsNaN(TabStripWidth) && TabStripWidth > 0)
                targetWidth = TabStripWidth;

            if (double.IsNaN(targetWidth) || targetWidth <= 0)
                return;

            var animation = new DoubleAnimation
            {
                From = startWidth,
                To = targetWidth,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase(),
                FillBehavior = FillBehavior.Stop
            };
            animation.Completed += (_, _) => targetPanel.BeginAnimation(WidthProperty, null);

            targetPanel.BeginAnimation(WidthProperty, animation);
        }, DispatcherPriority.Loaded);
    }
    #endregion
}
