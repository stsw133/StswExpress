using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Represents an individual toast item inside the <see cref="StswToaster"/> control.
/// Supports customizable styling, corner clipping, and a close button.
/// </summary>
/// <remarks>
/// This control is designed to display temporary notifications that can be dismissed individually.
/// </remarks>
[Stsw("0.14.0")]
public class StswToastItem : ContentControl, IStswCornerControl
{
    static StswToastItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswToastItem), new FrameworkPropertyMetadata(typeof(StswToastItem)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: close
        if (GetTemplateChild("PART_ButtonClose") is ButtonBase btnClose)
            btnClose.Click += (s, e) => StswToaster.RemoveItemFromItemsControl(StswFnUI.FindVisualAncestor<ItemsControl>(this), this);
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the info bar can be closed by the user.
    /// When enabled, a close button is displayed.
    /// </summary>
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }
    public static readonly DependencyProperty IsClosableProperty
        = DependencyProperty.Register(
            nameof(IsClosable),
            typeof(bool),
            typeof(StswToastItem)
        );

    /// <summary>
    /// Gets or sets the type of information displayed in the toast item, such as "Info," "Warning," or "Error."
    /// </summary>
    public StswDialogImage Type
    {
        get => (StswDialogImage)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }
    public static readonly DependencyProperty TypeProperty
        = DependencyProperty.Register(
            nameof(Type),
            typeof(StswDialogImage),
            typeof(StswToastItem)
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
            typeof(StswToastItem),
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
            typeof(StswToastItem),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
