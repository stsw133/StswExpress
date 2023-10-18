using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
[StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(StswLogPanelItem))]
public class StswLogPanel : ItemsControl
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
    private StswScrollViewer? stswScrollViewer;
    
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Content
        if (GetTemplateChild("PART_ScrollViewer") is StswScrollViewer scrollViewer)
            stswScrollViewer = scrollViewer;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override DependencyObject GetContainerForItemOverride() => new StswLogPanelItem();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswLogPanelItem;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        if (e.NewItems?.Count > 0)
            stswScrollViewer?.ScrollToEnd();
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
    /// Gets or sets the degree to which the corners of the control are rounded.
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
    /// Gets or sets the border thickness of the items.
    /// </summary>
    public Thickness ItemBorderThickness
    {
        get => (Thickness)GetValue(ItemBorderThicknessProperty);
        set => SetValue(ItemBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty ItemBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(ItemBorderThickness),
            typeof(Thickness),
            typeof(StswLogPanel)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the items are rounded.
    /// </summary>
    public CornerRadius ItemCornerRadius
    {
        get => (CornerRadius)GetValue(ItemCornerRadiusProperty);
        set => SetValue(ItemCornerRadiusProperty, value);
    }
    public static readonly DependencyProperty ItemCornerRadiusProperty
        = DependencyProperty.Register(
            nameof(ItemCornerRadius),
            typeof(CornerRadius),
            typeof(StswLogPanel)
        );

    /// <summary>
    /// Gets or sets the margin of the items.
    /// </summary>
    public Thickness ItemMargin
    {
        get => (Thickness)GetValue(ItemMarginProperty);
        set => SetValue(ItemMarginProperty, value);
    }
    public static readonly DependencyProperty ItemMarginProperty
        = DependencyProperty.Register(
            nameof(ItemMargin),
            typeof(Thickness),
            typeof(StswLogPanel)
        );

    /// <summary>
    /// Gets or sets the padding of the items.
    /// </summary>
    public Thickness ItemPadding
    {
        get => (Thickness)GetValue(ItemPaddingProperty);
        set => SetValue(ItemPaddingProperty, value);
    }
    public static readonly DependencyProperty ItemPaddingProperty
        = DependencyProperty.Register(
            nameof(ItemPadding),
            typeof(Thickness),
            typeof(StswLogPanel)
        );
    #endregion
}

/// <summary>
/// 
/// </summary>
public class StswLogPanelItem : ContentControl
{
    
}

/// <summary>
/// 
/// </summary>
public class StswLogItem
{
    public StswLogItem(StswLogType type, string description)
    {
        Type = type;
        Description = description;
    }

    /// <summary>
    /// 
    /// </summary>
    public StswLogType Type { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DateTime DateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 
    /// </summary>
    public string? Description { get; set; }
}
