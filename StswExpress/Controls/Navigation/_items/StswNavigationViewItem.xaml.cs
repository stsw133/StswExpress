using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A navigation element that can contain sub-elements and interact with a parent navigation control.
/// Supports icons, busy states, and dynamic context loading.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswNavigationViewItem Header="Reports" IconData="{StaticResource UserIcon}" TargetType="{x:Type local:ReportsContext}"/&gt;
/// </code>
/// </example>
[StswPlannedChanges(StswPlannedChanges.Finish)]
public class StswNavigationViewItem : TreeViewItem, IStswCornerControl, IStswIconControl
{
    private StswNavigationView? _navigationView;

    static StswNavigationViewItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigationViewItem), new FrameworkPropertyMetadata(typeof(StswNavigationViewItem)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswNavigationViewItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswNavigationViewItem;

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _navigationView = StswFnUI.FindVisualAncestor<StswNavigationView>(this);
        if (IsSelected)
            _navigationView?.SetContent(this, TargetType, CreateNewInstance);
    }

    /// <inheritdoc/>
    protected override void OnSelected(RoutedEventArgs e)
    {
        base.OnSelected(e);
        _navigationView?.SetContent(this, TargetType, CreateNewInstance);
    }
    #endregion

    #region Logic properties
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
            typeof(StswNavigationViewItem)
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
            typeof(StswNavigationViewItem)
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
            typeof(StswNavigationViewItem)
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
            typeof(StswNavigationViewItem)
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
            typeof(StswNavigationViewItem)
        );

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
            typeof(StswNavigationViewItem)
        );

    /// <summary>
    /// Gets or sets the namespace of the context associated with this navigation element.
    /// This defines the view or logical context to be loaded when the element is selected.
    /// </summary>
    public Type? TargetType
    {
        get => (Type?)GetValue(TargetTypeProperty);
        set => SetValue(TargetTypeProperty, value);
    }
    public static readonly DependencyProperty TargetTypeProperty
        = DependencyProperty.Register(
            nameof(TargetType),
            typeof(Type),
            typeof(StswNavigationViewItem)
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
            typeof(StswNavigationViewItem),
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
            typeof(StswNavigationViewItem),
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
            typeof(StswNavigationViewItem),
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
            typeof(StswNavigationViewItem),
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
            typeof(StswNavigationViewItem),
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
            typeof(StswNavigationViewItem)
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
            typeof(StswNavigationViewItem),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
