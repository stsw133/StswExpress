using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;

public class StswReorderListBox : ListBox, IStswCornerControl, IStswSelectionControl
{
    private object? _dragDropItem;
    private int _dragDropItemIndex = -1;

    static StswReorderListBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswReorderListBox), new FrameworkPropertyMetadata(typeof(StswReorderListBox)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswReorderListBox), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        DragLeave += OnDragLeave;
        DragOver += OnDragOver;

        ItemContainerStyle = new Style(typeof(StswReorderListBoxItem));

        ItemContainerStyle.Setters.Add(new EventSetter()
        {
            Event = MouseMoveEvent,
            Handler = new MouseEventHandler(OnItemMouseMouse)
        });

        ItemContainerStyle.Setters.Add(new EventSetter()
        {
            Event = DragOverEvent,
            Handler = new DragEventHandler(OnItemDragOver)
        });
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        var item = e.Data.GetData(DataFormats.Serializable);
        DropItem(item);
        _dragDropItem = item;
        RefreshList();
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        if (sender is UIElement listbox)
        {
            var hitTestResult = VisualTreeHelper.HitTest(listbox, e.GetPosition(listbox));

            if (hitTestResult != null)
                return;

            if (ItemsSource is IList list)
            {
                var removedItem = e.Data.GetData(DataFormats.Serializable);
                list.Remove(removedItem);
                SelectedIndex = list.Count - 1;
                RefreshList();
            }
        }
    }

    private void OnItemMouseMouse(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed
           && sender is FrameworkElement panelControl)
        {
            _dragDropItem = panelControl.DataContext;
            _dragDropItemIndex = ItemsSource is IList list ? list.IndexOf(_dragDropItem) : -1;
            try
            {
                DragDropEffects dragDropResult = DragDrop.DoDragDrop(panelControl,
                    new DataObject(DataFormats.Serializable, _dragDropItem),
                    DragDropEffects.Move);

                if (dragDropResult == DragDropEffects.None)
                {
                    DropItem(_dragDropItem);
                }
            }
            catch { throw; }
            finally
            {
                _dragDropItem = null;
                _dragDropItemIndex = -1;
            }
        }
    }

    void DropItem(object item)
    {
        if (ItemsSource is IList list)
        {
            if (list.Contains(item))
                return;
            var itemType = list.GetType().GenericTypeArguments.FirstOrDefault() ?? typeof(object);
            if (_dragDropItemIndex != -1)
            {
                list.Insert(_dragDropItemIndex, item);
                //SelectedIndex = _dragDropItemIndex;
            }
            else
            {
                list.Add(item);
                //SelectedIndex = list.Count - 1;
            }
        }
    }

    private void OnItemDragOver(object sender, DragEventArgs e)
    {
        if (sender is FrameworkElement targetItem
            && ItemsSource is IList list)
        {
            Swap(list, _dragDropItem, targetItem.DataContext);
        }
    }

    void Swap(IList list, object obj1, object obj2)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));

        int index1 = list.IndexOf(obj1);
        int index2 = list.IndexOf(obj2);

        System.Diagnostics.Debug.WriteLine($"{index1} : {index2}");

        if (index1 == -1 || index2 == -1)
            return;

        if (index1 != index2)
        {
            object temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }

        RefreshList();
    }

    void RefreshList()
    {
        var items = Items.Cast<object>().ToList();
        ItemsSource = null;
        ItemsSource = items;
        //CollectionViewSource.GetDefaultView(this.Items)?.Refresh();
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswReorderListBoxItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswReorderListBoxItem;

    #region Events & methods
    /// <summary>
    /// Occurs when the ItemsSource property value changes.
    /// </summary>
    /// <param name="oldValue">The old value of the ItemsSource property.</param>
    /// <param name="newValue">The new value of the ItemsSource property.</param>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        IStswSelectionControl.ItemsSourceChanged(this, newValue);
        base.OnItemsSourceChanged(oldValue, newValue);
    }

    /// <summary>
    /// Occurs when the ItemTemplate property value changes.
    /// </summary>
    /// <param name="oldItemTemplate">The old value of the ItemTemplate property.</param>
    /// <param name="newItemTemplate">The new value of the ItemTemplate property.</param>
    protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        IStswSelectionControl.ItemTemplateChanged(this, newItemTemplate);
        base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        IStswSelectionControl.SelectionChanged(this, e.AddedItems, e.RemovedItems);
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the items in control can be rearranged by drag and drop.
    /// </summary>
    internal bool CanRearrange  //TODO - CanRearrange
    {
        get => (bool)GetValue(CanRearrangeProperty);
        set => SetValue(CanRearrangeProperty, value);
    }
    public static readonly DependencyProperty CanRearrangeProperty
        = DependencyProperty.Register(
            nameof(CanRearrange),
            typeof(bool),
            typeof(StswReorderListBox),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnCanRearrangeChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnCanRearrangeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswReorderListBox stsw)
        {

        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
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
            typeof(StswReorderListBox),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
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
            typeof(StswReorderListBox),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/// <summary>
/// 
/// </summary>
public class StswReorderListBoxItem : ListBoxItem
{
    static StswReorderListBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswReorderListBoxItem), new FrameworkPropertyMetadata(typeof(StswReorderListBoxItem)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswReorderListBoxItem), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        //if (DataContext?.GetType()?.IsAssignableTo(typeof(IStswSelectionItem)) == true)
        //    SetBinding(IsSelectedProperty, new Binding(nameof(IStswSelectionItem.IsSelected)));
    }

    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
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
            typeof(StswReorderListBoxItem),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
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
            typeof(StswReorderListBoxItem),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
