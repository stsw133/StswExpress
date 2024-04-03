using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a control that displays a collection of items in a vertical list.
/// ItemsSource with items of <see cref="IStswSelectionItem"/> type automatically bind selected items.
/// </summary>
public class StswListBox : ListBox, IStswCornerControl, IStswScrollableControl
{
    static StswListBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswListBox), new FrameworkPropertyMetadata(typeof(StswListBox)));
    }

    #region Events & methods
    /// <summary>
    /// Gets a <see cref="StswScrollViewer"/> of the control.
    /// </summary>
    public StswScrollViewer GetScrollViewer() => (StswScrollViewer)GetTemplateChild("PART_ScrollViewer");

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        GetScrollViewer()?.InitAttachedProperties(this);
    }

    /// <summary>
    /// Occurs when the ItemsSource property value changes.
    /// </summary>
    /// <param name="oldValue">The old value of the ItemsSource property.</param>
    /// <param name="newValue">The new value of the ItemsSource property.</param>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        if (newValue?.GetType()?.IsListType(out var innerType) == true)
        {
            UsesSelectionItems = innerType?.IsAssignableTo(typeof(IStswSelectionItem)) == true;
            if (innerType?.IsAssignableTo(typeof(StswComboItem)) == true)
            {
                if (string.IsNullOrEmpty(DisplayMemberPath))
                    DisplayMemberPath = nameof(StswComboItem.Display);
                if (string.IsNullOrEmpty(SelectedValuePath))
                    SelectedValuePath = nameof(StswComboItem.Value);
            }
        }
        base.OnItemsSourceChanged(oldValue, newValue);

        /// CanRearrange
        if (CanRearrange)
        {
            UpdateLayout();

            if (oldValue != null)
                foreach (var elem in oldValue)
                    if (ItemContainerGenerator.ContainerFromItem(elem) is ListBoxItem item)
                    {
                        //item.PreviewMouseMove -= Item_PreviewMouseMove;
                        item.PreviewMouseLeftButtonDown -= Item_PreviewMouseLeftButtonDown;
                        item.Drop -= Item_Drop;
                    }
            if (newValue != null)
                foreach (var elem in newValue)
                    if (ItemContainerGenerator.ContainerFromItem(elem) is ListBoxItem item)
                    {
                        //item.PreviewMouseMove += Item_PreviewMouseMove;
                        item.PreviewMouseLeftButtonDown += Item_PreviewMouseLeftButtonDown;
                        item.Drop += Item_Drop;
                    }
        }
    }

    /// <summary>
    /// Occurs when the ItemTemplate property value changes.
    /// </summary>
    /// <param name="oldItemTemplate">The old value of the ItemTemplate property.</param>
    /// <param name="newItemTemplate">The new value of the ItemTemplate property.</param>
    protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        if (newItemTemplate != null && !string.IsNullOrEmpty(DisplayMemberPath))
            DisplayMemberPath = string.Empty;
        base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
        base.OnPreviewMouseMove(e);

        Point point = e.GetPosition(null);
        Vector diff = _dragStartPoint - point;
        if (e.LeftButton == MouseButtonState.Pressed &&
            (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
        {
            var lbi = StswFn.FindVisualAncestor<ListBoxItem>(((DependencyObject)e.OriginalSource));
            if (lbi != null)
            {
                DragDrop.DoDragDrop(lbi, lbi.DataContext, DragDropEffects.Move);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Item_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => _dragStartPoint = e.GetPosition(null);
    private Point _dragStartPoint;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Item_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is ListBoxItem draggedItem && e.LeftButton == MouseButtonState.Pressed)
        {
            DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
            draggedItem.IsSelected = true;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Item_Drop(object sender, DragEventArgs e)
    {
        if (sender is ListBoxItem item)
        {
            var type = ItemContainerGenerator.ItemFromContainer(item).GetType();

            var source = e.Data.GetData(type);
            var target = ((ListBoxItem)sender).DataContext;

            int sourceIndex = Items.IndexOf(source);
            int targetIndex = Items.IndexOf(target);

            Move(source, sourceIndex, targetIndex);
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="sourceIndex"></param>
    /// <param name="targetIndex"></param>
    private void Move(object? source, int sourceIndex, int targetIndex)
    {
        if (ItemsSource is IList items)
        {
            if (sourceIndex < targetIndex)
            {
                items.Insert(targetIndex + 1, source);
                items.RemoveAt(sourceIndex);
            }
            else if (items.Count + 1 > sourceIndex + 1)
            {
                items.Insert(targetIndex, source);
                items.RemoveAt(sourceIndex + 1);
            }
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the items in control can be rearranged by drag and drop.
    /// </summary>
    public bool CanRearrange
    {
        get => (bool)GetValue(CanRearrangeProperty);
        set => SetValue(CanRearrangeProperty, value);
    }
    public static readonly DependencyProperty CanRearrangeProperty
        = DependencyProperty.Register(
            nameof(CanRearrange),
            typeof(bool),
            typeof(StswListBox),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnCanRearrangeChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnCanRearrangeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswListBox stsw)
        {
            if (stsw.ItemsSource != null)
            {
                if (stsw.CanRearrange)
                {
                    foreach (var elem in stsw.ItemsSource)
                        if (stsw.ItemContainerGenerator.ContainerFromItem(elem) is ListBoxItem item)
                        {
                            item.PreviewMouseMove += stsw.Item_PreviewMouseMove;
                            item.Drop += stsw.Item_Drop;
                        }
                }
                else if ((bool?)e.OldValue == true)
                {
                    foreach (var elem in stsw.ItemsSource)
                        if (stsw.ItemContainerGenerator.ContainerFromItem(elem) is ListBoxItem item)
                        {
                            item.PreviewMouseMove -= stsw.Item_PreviewMouseMove;
                            item.Drop -= stsw.Item_Drop;
                        }
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the control uses selection items that implement
    /// the <see cref="IStswSelectionItem"/> interface to enable advanced selection features.
    /// </summary>
    internal bool UsesSelectionItems
    {
        get => (bool)GetValue(UsesSelectionItemsProperty);
        set => SetValue(UsesSelectionItemsProperty, value);
    }
    public static readonly DependencyProperty UsesSelectionItemsProperty
        = DependencyProperty.Register(
            nameof(UsesSelectionItems),
            typeof(bool),
            typeof(StswListBox)
        );
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
            typeof(StswListBox)
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
            typeof(StswListBox)
        );
    #endregion
}
