using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswLogPanel : ListBox, IStswCornerControl
{
    public ICommand RemoveLogCommand { get; set; }

    public StswLogPanel()
    {
        RemoveLogCommand = new StswCommand<StswLogItem?>(RemoveLog);
    }
    static StswLogPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswLogPanel), new FrameworkPropertyMetadata(typeof(StswLogPanel)));
    }

    #region Events & methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        if (e.NewItems?.Count > 0 && GetTemplateChild("PART_ScrollViewer") is ScrollViewer scrollViewer)
            scrollViewer?.ScrollToEnd();
    }
    
    /// Command: remove log
    /// <summary>
    /// 
    /// </summary>
    public void RemoveLog(StswLogItem? item)
    {
        if (ItemsSource is IList list and not null)
            list.Remove(item);
        else
            Items?.Remove(item);
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets a value indicating whether the log is closable and has a close button.
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
            typeof(StswLogPanel)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match the
    /// border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswLogPanel)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswLogPanel)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="StswScrollViewer"/> inside the control is dynamic.
    /// </summary>
    public bool IsScrollDynamic
    {
        get => (bool)GetValue(IsScrollDynamicProperty);
        set => SetValue(IsScrollDynamicProperty, value);
    }
    public static readonly DependencyProperty IsScrollDynamicProperty
        = DependencyProperty.Register(
            nameof(IsScrollDynamic),
            typeof(bool),
            typeof(StswLogPanel),
            new PropertyMetadata(true)
        );
    #endregion
}
