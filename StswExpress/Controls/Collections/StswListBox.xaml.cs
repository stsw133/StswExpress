using System.Collections;
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
    internal bool skipSelectionChanged;
    
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        if (!skipSelectionChanged)
        {
            DataContextChanged += (s, e) => SelectionChanged -= OnSelectionChanged;
            SelectionChanged += OnSelectionChanged;
        }

        base.OnApplyTemplate();
    }

    /// OnSelectionChanged
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
