using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A navigation element that can contain sub-elements and interact with a parent navigation control.
/// Supports icons, busy states, and dynamic context loading.
/// </summary>
/// <remarks>
/// WARNING: There are bugs when switching from <see cref="StswCompactibility.Compact"/> mode
/// to <see cref="StswCompactibility.Full"/> mode - all buttons in expander can become invisible.
/// </remarks>
[ContentProperty(nameof(Items))]
public class StswNavigationElement : HeaderedItemsControl, IStswCornerControl, IStswIconControl
{
    private StswNavigation? _stswNavigation;

    static StswNavigationElement()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigationElement), new FrameworkPropertyMetadata(typeof(StswNavigationElement)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// StswNavigation
        if (StswFnUI.FindVisualAncestor<StswNavigation>(this) is StswNavigation stswNavigation)
            _stswNavigation = stswNavigation;

        OnIsCheckedChanged(this, new DependencyPropertyChangedEventArgs());
        //OnItemsIndentationChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <inheritdoc/>
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        OnItemsIndentationChanged(this, new DependencyPropertyChangedEventArgs());
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the namespace of the context associated with this navigation element.
    /// This defines the view or logical context to be loaded when the element is selected.
    /// </summary>
    public object ContextNamespace
    {
        get => (object)GetValue(ContextNamespaceProperty);
        set => SetValue(ContextNamespaceProperty, value);
    }
    public static readonly DependencyProperty ContextNamespaceProperty
        = DependencyProperty.Register(
            nameof(ContextNamespace),
            typeof(object),
            typeof(StswNavigationElement)
        );

    /// <summary>
    /// Gets or sets a value indicating whether to create a new instance of the context object when the element is checked.
    /// If set to <see langword="true"/>, a fresh instance of the context is created each time the navigation element is selected.
    /// </summary>
    public bool CreateNewInstance
    {
        get => (bool)GetValue(CreateNewInstanceProperty);
        set => SetValue(CreateNewInstanceProperty, value);
    }
    public static readonly DependencyProperty CreateNewInstanceProperty
        = DependencyProperty.Register(
            nameof(CreateNewInstance),
            typeof(bool),
            typeof(StswNavigationElement)
        );

    /// <inheritdoc/>
    public Geometry? IconData
    {
        get => (Geometry?)GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }
    public static readonly DependencyProperty IconDataProperty
        = DependencyProperty.Register(
            nameof(IconData),
            typeof(Geometry),
            typeof(StswNavigationElement)
        );

    /// <inheritdoc/>
    public GridLength IconScale
    {
        get => (GridLength)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }
    public static readonly DependencyProperty IconScaleProperty
        = DependencyProperty.Register(
            nameof(IconScale),
            typeof(GridLength),
            typeof(StswNavigationElement)
        );

    /// <summary>
    /// Gets or sets the source used for the icon image.
    /// Supports bitmap-based icons in addition to vector-based ones.
    /// </summary>
    public ImageSource? IconSource
    {
        get => (ImageSource?)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }
    public static readonly DependencyProperty IconSourceProperty
        = DependencyProperty.Register(
            nameof(IconSource),
            typeof(ImageSource),
            typeof(StswNavigationElement)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the navigation element is in the busy state.
    /// Used to display a loading indicator when processing a navigation change.
    /// </summary>
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        internal set => SetValue(IsBusyProperty, value);
    }
    public static readonly DependencyProperty IsBusyProperty
        = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool),
            typeof(StswNavigationElement)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the navigation element is inside the compact panel.
    /// This is relevant when switching between compact and full navigation modes.
    /// </summary>
    public bool IsInCompactPanel
    {
        get => (bool)GetValue(IsInCompactPanelProperty);
        internal set => SetValue(IsInCompactPanelProperty, value);
    }
    public static readonly DependencyProperty IsInCompactPanelProperty
        = DependencyProperty.Register(
            nameof(IsInCompactPanel),
            typeof(bool),
            typeof(StswNavigationElement)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the navigation element is checked.
    /// When checked, the element is highlighted and may trigger a context switch.
    /// </summary>
    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }
    public static readonly DependencyProperty IsCheckedProperty
        = DependencyProperty.Register(
            nameof(IsChecked),
            typeof(bool),
            typeof(StswNavigationElement),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsCheckedChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnIsCheckedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNavigationElement stsw)
        {
            if (stsw._stswNavigation != null)
            {
                /// when expanding expander in compact mode
                if (stsw.HasItems && stsw.IsChecked && stsw.TabStripMode == StswCompactibility.Compact)
                {
                    /// move compact panel items back to previous expander
                    if (stsw._stswNavigation.CompactedExpander != null && stsw._stswNavigation.ItemsCompact.Count > 0)
                    {
                        stsw._stswNavigation.CompactedExpander.Items.Clear();
                        foreach (StswNavigationElement item in stsw._stswNavigation.ItemsCompact.TryClone())
                        {
                            item.IsInCompactPanel = false;
                            stsw._stswNavigation.CompactedExpander.Items.Add(item);
                        }
                    }

                    /// when clicking the same expander
                    if (stsw._stswNavigation.CompactedExpander == stsw && stsw._stswNavigation.ItemsCompact.Count > 0)
                        stsw._stswNavigation.ItemsCompact = new ObservableCollection<StswNavigationElement>();
                    else /// when clicking different expander
                    {
                        /// load new items to compact panel
                        stsw._stswNavigation.CompactedExpander = stsw;
                        stsw._stswNavigation.ItemsCompact = [.. stsw.Items.TryClone().Cast<StswNavigationElement>()];
                        foreach (var item in stsw._stswNavigation.ItemsCompact)
                            item.IsInCompactPanel = true;
                    }

                    stsw.IsChecked = false;
                }
                /// when clicking button
                else if (!stsw.HasItems && stsw.IsChecked)
                {
                    /// uncheck last button, check new button
                    if (stsw._stswNavigation.LastSelectedItem != stsw)
                        stsw._stswNavigation.LastSelectedItem = stsw;

                    /// hide compact panel
                    if (stsw._stswNavigation.TabStripMode == StswCompactibility.Compact)
                        stsw._stswNavigation.ItemsCompact.Clear();

                    /// load context for content presenter
                    if (stsw.ContextNamespace != null)
                    {
                        stsw.IsBusy = true;
                        stsw._stswNavigation.ChangeContext(stsw.ContextNamespace, stsw.CreateNewInstance);
                        stsw.IsBusy = false;
                    }
                }
                /// do not allow to uncheck checked button
                else if (!stsw.HasItems && stsw._stswNavigation.LastSelectedItem == stsw)
                    stsw.IsChecked = true;
                /// collapse expander so it is not needed to click it twice
                //else if (stsw.HasItems && stsw.stswNavigation.CurrentlyExpandedElement == stsw && stsw.stswNavigation.TabStripMode == StswToolbarMode.Compact)
                //    stsw.IsChecked = false;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the navigation element is in the compact state.
    /// Determines how the element is displayed within the navigation structure.
    /// </summary>
    public StswCompactibility TabStripMode
    {
        get => (StswCompactibility)GetValue(TabStripModeProperty);
        internal set => SetValue(TabStripModeProperty, value);
    }
    public static readonly DependencyProperty TabStripModeProperty
        = DependencyProperty.Register(
            nameof(TabStripMode),
            typeof(StswCompactibility),
            typeof(StswNavigationElement),
            new FrameworkPropertyMetadata(default(StswCompactibility),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTabStripModeChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnTabStripModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNavigationElement stsw)
        {
            if (stsw.HasItems && stsw.TabStripMode == StswCompactibility.Compact)
                stsw.IsChecked = false;
        }
    }
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
            typeof(StswNavigationElement),
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
            typeof(StswNavigationElement),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public Brush IconFill
    {
        get => (Brush)GetValue(IconFillProperty);
        set => SetValue(IconFillProperty, value);
    }
    public static readonly DependencyProperty IconFillProperty
        = DependencyProperty.Register(
            nameof(IconFill),
            typeof(Brush),
            typeof(StswNavigationElement),
            new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public Brush IconStroke
    {
        get => (Brush)GetValue(IconStrokeProperty);
        set => SetValue(IconStrokeProperty, value);
    }
    public static readonly DependencyProperty IconStrokeProperty
        = DependencyProperty.Register(
            nameof(IconStroke),
            typeof(Brush),
            typeof(StswNavigationElement),
            new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public double IconStrokeThickness
    {
        get => (double)GetValue(IconStrokeThicknessProperty);
        set => SetValue(IconStrokeThicknessProperty, value);
    }
    public static readonly DependencyProperty IconStrokeThicknessProperty
        = DependencyProperty.Register(
            nameof(IconStrokeThickness),
            typeof(double),
            typeof(StswNavigationElement),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the indentation value for items.
    /// Determines the horizontal spacing of child elements within the navigation structure.
    /// </summary>
    public double ItemsIndentation
    {
        get => (double)GetValue(ItemsIndentationProperty);
        set => SetValue(ItemsIndentationProperty, value);
    }
    public static readonly DependencyProperty ItemsIndentationProperty
        = DependencyProperty.Register(
            nameof(ItemsIndentation),
            typeof(double),
            typeof(StswNavigationElement),
            new FrameworkPropertyMetadata(default(double),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnItemsIndentationChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnItemsIndentationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNavigationElement stsw)
        {
            var padding = stsw.Padding;
            var ancestorElement = stsw;

            while (ancestorElement != null)
            {
                ancestorElement = StswFnUI.FindVisualAncestor<StswNavigationElement>(ancestorElement);
                if (ancestorElement != null && ancestorElement.Items.Count > 0 && ancestorElement.TabStripMode == StswCompactibility.Full && ancestorElement.ContextNamespace == null)
                    padding = new Thickness(padding.Left + ancestorElement.ItemsIndentation, padding.Top, padding.Right, padding.Bottom);
            }
            stsw.ItemsMargin = padding;
        }
    }

    /// <summary>
    /// Gets or sets the indentation margin for nested items.
    /// Adjusts the alignment and spacing of child elements.
    /// </summary>
    internal Thickness? ItemsMargin
    {
        get => (Thickness?)GetValue(ItemsMarginProperty);
        set => SetValue(ItemsMarginProperty, value);
    }
    internal static readonly DependencyProperty ItemsMarginProperty
        = DependencyProperty.Register(
            nameof(ItemsMargin),
            typeof(Thickness?),
            typeof(StswNavigationElement),
            new FrameworkPropertyMetadata(default(Thickness?), FrameworkPropertyMetadataOptions.AffectsMeasure)
        );

    /// <summary>
    /// Gets or sets the thickness of the sub-item border.
    /// Defines the visual separation between navigation elements.
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
            typeof(StswNavigationElement),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswNavigationElement Header="Reports" IconData="{StaticResource UserIcon}" ContextNamespace="App.Views.ReportsView"/>

*/
