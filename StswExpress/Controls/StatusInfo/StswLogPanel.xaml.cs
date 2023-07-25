using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswLogPanel : UserControl
{
    public ICommand RemoveLogCommand { get; set; }

    public StswLogPanel()
    {
        SetValue(ItemsProperty, new ObservableCollection<StswLogItem>());

        RemoveLogCommand = new StswRelayCommand<StswLogItem?>(RemoveLog_Executed);
    }
    static StswLogPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswLogPanel), new FrameworkPropertyMetadata(typeof(StswLogPanel)));
    }

    #region Events
    private RgsScrollViewer? stswScrollViewer;
    
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        /// Content
        if (GetTemplateChild("PART_ScrollViewer") is RgsScrollViewer scrollViewer)
            stswScrollViewer = scrollViewer;
        if (Items == null)
            Items = new();

        base.OnApplyTemplate();
    }

    /// Command: remove log
    /// <summary>
    /// 
    /// </summary>
    public void RemoveLog_Executed(StswLogItem? item)
    {
        if (item != null)
            Items.Remove(item);
    }
    #endregion

    #region Main properties
    /// <summary>
    /// 
    /// </summary>
    public ObservableCollection<StswLogItem> Items
    {
        get => (ObservableCollection<StswLogItem>)GetValue(ItemsProperty);
        set =>  SetValue(ItemsProperty, value);
    }
    public static readonly DependencyProperty ItemsProperty
        = DependencyProperty.Register(
            nameof(Items),
            typeof(ObservableCollection<StswLogItem>),
            typeof(StswLogPanel),
            new FrameworkPropertyMetadata(default(ObservableCollection<StswLogItem>),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnItemsChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnItemsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswLogPanel stsw)
        {
            if (stsw.Items != null)
                stsw.Items.CollectionChanged += (s, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                        stsw.stswScrollViewer?.ScrollToEnd();
                };
        }
    }
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
    /// Gets or sets the degree to which the corners of the items are rounded.
    /// </summary>
    public CornerRadius SubCornerRadius
    {
        get => (CornerRadius)GetValue(SubCornerRadiusProperty);
        set => SetValue(SubCornerRadiusProperty, value);
    }
    public static readonly DependencyProperty SubCornerRadiusProperty
        = DependencyProperty.Register(
            nameof(SubCornerRadius),
            typeof(CornerRadius),
            typeof(StswLogPanel)
        );

    /// <summary>
    /// Gets or sets the margin of the items.
    /// </summary>
    public Thickness SubMargin
    {
        get => (Thickness)GetValue(SubMarginProperty);
        set => SetValue(SubMarginProperty, value);
    }
    public static readonly DependencyProperty SubMarginProperty
        = DependencyProperty.Register(
            nameof(SubMargin),
            typeof(Thickness),
            typeof(StswLogPanel)
        );

    /// <summary>
    /// Gets or sets the padding of the items.
    /// </summary>
    public Thickness SubPadding
    {
        get => (Thickness)GetValue(SubPaddingProperty);
        set => SetValue(SubPaddingProperty, value);
    }
    public static readonly DependencyProperty SubPaddingProperty
        = DependencyProperty.Register(
            nameof(SubPadding),
            typeof(Thickness),
            typeof(StswLogPanel)
        );
    #endregion
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