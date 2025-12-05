using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress.Wpf;
/// <summary>
/// A sub-control that expands to show additional sub-controls when hovered over.
/// Supports a primary icon, customizable drop-down content, and various styling options.
/// </summary>
/// <remarks>
/// This control is useful for compact UI designs where a small, expandable selector provides access to multiple actions.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswSubSelector IconData="{StaticResource MoreIcon}"&gt;
///     &lt;se:StswSubButton Content="Option 1"/&gt;
///     &lt;se:StswSubButton Content="Option 2"/&gt;
/// &lt;/se:StswSubSelector&gt;
/// </code>
/// </example>
[ContentProperty(nameof(Items))]
public class StswSubSelector : ContentControl, IStswSubControl, IStswCornerControl, IStswDropControl, IStswIconControl
{
    private Popup? _popup;

    bool IStswDropControl.SuppressNextOpen { get; set; }

    public StswSubSelector()
    {
        SetValue(ItemsProperty, new ObservableCollection<IStswSubControl>());
    }
    static StswSubSelector()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSubSelector), new FrameworkPropertyMetadata(typeof(StswSubSelector)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_popup?.Child != null)
            _popup.Child.MouseLeave -= PopupChild_MouseLeave;

        /// StswPopup: popup
        _popup = GetTemplateChild("PART_Popup") as Popup;
        if (_popup?.Child != null)
            _popup.Child.MouseLeave += PopupChild_MouseLeave;
    }

    /// <inheritdoc/>
    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        IsDropDownOpen = true;
    }

    /// <summary>
    /// Handles the MouseLeave event of the popup child to close the drop-down.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
    private void PopupChild_MouseLeave(object sender, MouseEventArgs e) => IsDropDownOpen = false;
    #endregion

    #region Logic properties
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
            typeof(StswSubSelector)
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
            typeof(StswSubSelector)
        );

    /// <inheritdoc/>
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }
    public static readonly DependencyProperty IsBusyProperty
        = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool?),
            typeof(StswSubSelector)
        );

    /// <inheritdoc/>
    public bool IsContentVisible
    {
        get => (bool)GetValue(IsContentVisibleProperty);
        set => SetValue(IsContentVisibleProperty, value);
    }
    public static readonly DependencyProperty IsContentVisibleProperty
        = DependencyProperty.Register(
            nameof(IsContentVisible),
            typeof(bool),
            typeof(StswSubSelector)
        );

    /// <inheritdoc/>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    public static readonly DependencyProperty IsDropDownOpenProperty
        = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(StswSubSelector)
        );

    /// <summary>
    /// Gets or sets the collection of sub-controls to be displayed inside the drop-down.
    /// </summary>
    public ObservableCollection<IStswSubControl> Items
    {
        get => (ObservableCollection<IStswSubControl>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }
    public static readonly DependencyProperty ItemsProperty
        = DependencyProperty.Register(
            nameof(Items),
            typeof(ObservableCollection<IStswSubControl>),
            typeof(StswSubSelector)
        );

    /// <inheritdoc/>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StswSubSelector),
            new FrameworkPropertyMetadata(default(Orientation), FrameworkPropertyMetadataOptions.AffectsArrange)
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
            typeof(StswSubSelector),
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
            typeof(StswSubSelector),
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
            typeof(StswSubSelector),
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
            typeof(StswSubSelector),
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
            typeof(StswSubSelector),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }
    public static readonly DependencyProperty MaxDropDownHeightProperty
        = DependencyProperty.Register(
            nameof(MaxDropDownHeight),
            typeof(double),
            typeof(StswSubSelector),
            new PropertyMetadata(SystemParameters.PrimaryScreenHeight / 3)
        );

    /// <inheritdoc/>
    public double MaxDropDownWidth
    {
        get => (double)GetValue(MaxDropDownWidthProperty);
        set => SetValue(MaxDropDownWidthProperty, value);
    }
    public static readonly DependencyProperty MaxDropDownWidthProperty
        = DependencyProperty.Register(
            nameof(MaxDropDownWidth),
            typeof(double),
            typeof(StswSubSelector),
            new PropertyMetadata(double.NaN)
        );
    #endregion
}
