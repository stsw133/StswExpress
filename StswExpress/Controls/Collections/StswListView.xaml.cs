using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control that displays a collection of items in a vertical list.
/// ItemsSource with items of <see cref="IStswSelectionItem"/> type automatically binds selected items.
/// </summary>
public class StswListView : ListView
{
    public StswListView()
    {
        OnItemsSourceChanged(this, EventArgs.Empty);
        DependencyPropertyDescriptor.FromProperty(ItemsSourceProperty, typeof(StswListView)).AddValueChanged(this, OnItemsSourceChanged);
    }
    static StswListView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswListView), new FrameworkPropertyMetadata(typeof(StswListView)));
    }

    #region Events & methods
    /// <summary>
    /// 
    /// </summary>
    private void OnItemsSourceChanged(object? sender, EventArgs e) => UsesSelectionItems = ItemsSource?.OfType<IStswSelectionItem>() != null;
    #endregion

    #region Main properties
    /// <summary>
    /// 
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
            typeof(StswListView)
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
            typeof(StswListView)
        );
    #endregion
}
