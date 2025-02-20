using System.Windows;
using System.Windows.Controls;

namespace StswExpress;/// <summary>
/// Represents an individual item inside the <see cref="StswComboBox"/>.
/// Supports selection state binding and corner customization.
/// </summary>
public class StswComboBoxItem : ComboBoxItem, IStswCornerControl
{
    static StswComboBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswComboBoxItem), new FrameworkPropertyMetadata(typeof(StswComboBoxItem)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswComboBoxItem), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// moved to StswComboBox, cause it causes bugs that opening second instance changes selection to selection of first instance
        //if (DataContext is IStswSelectionItem)
        //    SetBinding(IsSelectedProperty, new Binding(nameof(IStswSelectionItem.IsSelected)));
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the item is read-only.
    /// When set to <see langword="true"/>, the item cannot be selected.
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswComboBoxItem)
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
            typeof(StswComboBoxItem),
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
            typeof(StswComboBoxItem),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswComboBox>
    <se:StswComboBoxItem Content="Option 1"/>
    <se:StswComboBoxItem Content="Option 2"/>
</se:StswComboBox>

*/
