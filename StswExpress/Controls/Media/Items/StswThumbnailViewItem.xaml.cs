using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;
/// <summary>
/// 
/// </summary>
internal class StswThumbnailViewItem : ListBoxItem
{
    static StswThumbnailViewItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswThumbnailViewItem), new FrameworkPropertyMetadata(typeof(StswThumbnailViewItem)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswThumbnailViewItem), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (DataContext is IStswSelectionItem)
            SetBinding(IsSelectedProperty, new Binding(nameof(IStswSelectionItem.IsSelected)));
    }
    #endregion
}
