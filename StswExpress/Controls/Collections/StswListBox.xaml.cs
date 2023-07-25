using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Represents a control that displays a collection of items in a vertical list.
/// </summary>
public class StswListBox : ListBox
{
    public StswListBox()
    {
        DependencyPropertyDescriptor.FromProperty(SelectionModeProperty, typeof(StswListBox)).AddValueChanged(this, (s, e) => OnSelectedItemsBindingChanged(this, new DependencyPropertyChangedEventArgs()));
    }
    static StswListBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswListBox), new FrameworkPropertyMetadata(typeof(StswListBox)));
    }

    #region Events
    internal bool skipSelectionChanged;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        if (!skipSelectionChanged)
        {
            DataContextChanged += (s, e) => SelectionChanged -= OnSelectionChanged;
            SelectionChanged += OnSelectionChanged;
        }

        base.OnApplyTemplate();
    }

    /// <summary>
    /// Handles the selection changed event of the StswListBox.
    /// </summary>
    internal void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedItemsBinding != null)
        {
            foreach (var item in e.RemovedItems)
                if (SelectedItemsBinding.Contains(item))
                    SelectedItemsBinding.Remove(item);
            foreach (var item in e.AddedItems)
                if (!SelectedItemsBinding.Contains(item))
                    SelectedItemsBinding.Add(item);
            GetBindingExpression(SelectedItemsBindingProperty)?.UpdateSource();
        }
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the collection that holds the selected items of the control.
    /// </summary>
    public IList SelectedItemsBinding
    {
        get => (IList)GetValue(SelectedItemsBindingProperty);
        set => SetValue(SelectedItemsBindingProperty, value);
    }
    public static readonly DependencyProperty SelectedItemsBindingProperty
        = DependencyProperty.Register(
            nameof(SelectedItemsBinding),
            typeof(IList),
            typeof(StswListBox),
            new FrameworkPropertyMetadata(default(IList),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemsBindingChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnSelectedItemsBindingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswListBox stsw)
        {
            if (stsw.SelectionMode == SelectionMode.Multiple)
                stsw.SetSelectedItems(stsw.SelectedItemsBinding);
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
            typeof(StswListBox)
        );
    #endregion
}
