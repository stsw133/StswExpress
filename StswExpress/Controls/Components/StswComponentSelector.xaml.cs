using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a panel control that functions as a component and displays an icon. 
/// It can expand to show additional components when the mouse is over an icon.
/// </summary>
[ContentProperty(nameof(Items))]
public class StswComponentSelector : ItemsControl, IStswComponent
{
    static StswComponentSelector()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswComponentSelector), new FrameworkPropertyMetadata(typeof(StswComponentSelector)));
    }

    #region Events & methods
    private Popup? popup;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// StswPopup: popup
        if (GetTemplateChild("PART_Popup") is Popup popup)
        {
            popup.Child.MouseLeave += (s, e) => popup.IsOpen = false;
            this.popup = popup;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);

        if (popup != null)
            popup.IsOpen = true;
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the visibility of the header within the control.
    /// </summary>
    public Visibility? HeaderVisibility
    {
        get => (Visibility)GetValue(HeaderVisibilityProperty);
        set => SetValue(HeaderVisibilityProperty, value);
    }
    public static readonly DependencyProperty HeaderVisibilityProperty
        = DependencyProperty.Register(
            nameof(HeaderVisibility),
            typeof(Visibility),
            typeof(StswComponentSelector)
        );

    /// <summary>
    /// 
    /// </summary>
    public object? Header
    {
        get => (object?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    public static readonly DependencyProperty HeaderProperty
        = DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(StswComponentSelector)
        );

    /// <summary>
    /// Gets or sets the geometry used for the icon.
    /// </summary>
    public Geometry? IconData
    {
        get => (Geometry?)GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }
    public static readonly DependencyProperty IconDataProperty
        = DependencyProperty.Register(
            nameof(IconData),
            typeof(Geometry),
            typeof(StswComponentSelector)
        );

    /// <summary>
    /// Gets or sets the scale of the arrow icon.
    /// </summary>
    public GridLength? IconScale
    {
        get => (GridLength?)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }
    public static readonly DependencyProperty IconScaleProperty
        = DependencyProperty.Register(
            nameof(IconScale),
            typeof(GridLength?),
            typeof(StswComponentSelector)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the control is in a busy/loading state.
    /// </summary>
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }
    public static readonly DependencyProperty IsBusyProperty
        = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool),
            typeof(StswComponentSelector)
        );

    /// <summary>
    /// Gets or sets the orientation of the control.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StswComponentSelector)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the maximum height of the drop-down portion of the button.
    /// </summary>
    public double? MaxDropDownHeight
    {
        get => (double?)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }
    public static readonly DependencyProperty MaxDropDownHeightProperty
        = DependencyProperty.Register(
            nameof(MaxDropDownHeight),
            typeof(double?),
            typeof(StswComponentSelector)
        );

    /// <summary>
    /// Gets or sets the border thickness of the drop-down popup.
    /// </summary>
    public Thickness PopupThickness
    {
        get => (Thickness)GetValue(PopupThicknessProperty);
        set => SetValue(PopupThicknessProperty, value);
    }
    public static readonly DependencyProperty PopupThicknessProperty
        = DependencyProperty.Register(
            nameof(PopupThickness),
            typeof(Thickness),
            typeof(StswComponentSelector)
        );
    #endregion
}
