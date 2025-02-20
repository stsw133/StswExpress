using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Represents an individual alert item inside the <see cref="StswAlert"/> control.
/// Supports customizable styling, corner clipping, and a close button.
/// </summary>
/// <remarks>
/// This control is designed to display temporary notifications that can be dismissed individually.
/// </remarks>
public class StswAlertItem : ContentControl, IStswCornerControl
{
    static StswAlertItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswAlertItem), new FrameworkPropertyMetadata(typeof(StswAlertItem)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: close
        if (GetTemplateChild("PART_CloseButton") is ButtonBase btnClose)
            btnClose.Click += (s, e) => StswAlert.RemoveItemFromItemsControl(StswFn.FindVisualAncestor<ItemsControl>(this), this);
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
            typeof(StswAlertItem),
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
            typeof(StswAlertItem),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
