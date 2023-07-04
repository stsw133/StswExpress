using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;

public class StswListBox : ListBox
{
    static StswListBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswListBox), new FrameworkPropertyMetadata(typeof(StswListBox)));
    }

    #region Events
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        DataContextChanged += (s, e) => SelectionChanged -= OnSelectionChanged;
        SelectionChanged += OnSelectionChanged;

        base.OnApplyTemplate();
    }

    /// GetSelectedValues
    public List<object?> GetSelectedValues()
    {
        var selectedValues = new List<object?>();

        foreach (var selectedItem in SelectedItems)
        {
            if (SelectedValuePath != null && selectedItem.GetType().GetProperty(SelectedValuePath) is PropertyInfo propertyInfo)
                selectedValues.Add(propertyInfo.GetValue(selectedItem));
            else
                selectedValues.Add(selectedItem);
        }

        return selectedValues;
    }

    /// OnSelectionChanged
    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedItemsBinding != null)
        {
            SelectedItemsBinding.Clear();
            foreach (var item in SelectedItems)
                SelectedItemsBinding.Add(item);
            GetBindingExpression(SelectedItemsBindingProperty)?.UpdateSource();
        }
    }
    #endregion

    #region Main properties
    /// SelectedItemsBinding
    public static readonly DependencyProperty SelectedItemsBindingProperty
        = DependencyProperty.Register(
            nameof(SelectedItemsBinding),
            typeof(IList),
            typeof(StswListBox),
            new FrameworkPropertyMetadata(default(IList),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemsBindingChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public IList SelectedItemsBinding
    {
        get => (IList)GetValue(SelectedItemsBindingProperty);
        set => SetValue(SelectedItemsBindingProperty, value);
    }
    private static void OnSelectedItemsBindingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswListBox stsw)
            stsw.SetSelectedItems(stsw.SelectedItemsBinding);
    }
    #endregion

    #region Spatial properties
    /// > CornerRadius ...
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswListBox)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion
}
