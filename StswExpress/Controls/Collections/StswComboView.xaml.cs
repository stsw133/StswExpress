using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;

public class StswComboView : ListBox
{
    static StswComboView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswComboView), new FrameworkPropertyMetadata(typeof(StswComboView)));
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
        SetSelectedText();
    }

    /// SetSelectedText
    internal void SetSelectedText()
    {
        var listSeparator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";
        var selectedText = new StringBuilder();

        foreach (var selectedItem in SelectedItems)
        {
            if (DisplayMemberPath != null && selectedItem.GetType().GetProperty(DisplayMemberPath) is PropertyInfo propertyInfo)
            {
                var value = propertyInfo.GetValue(selectedItem);
                selectedText.Append(value?.ToString());
                selectedText.Append(listSeparator);
            }
            else
            {
                selectedText.Append(selectedItem);
                selectedText.Append(listSeparator);
            }
        }
        if (selectedText.Length > listSeparator.Length)
            selectedText.Length -= listSeparator.Length;

        Text = selectedText.ToString();

        /* /// OTHER METHOD FOR SETTING SELECTED TEXT
        if (SelectedItems?.Count > 0)
        {
            var items = SelectedItems.OfType<object?>().ToList();
            var item = SelectedItems.OfType<object?>().FirstOrDefault();
            var listSep = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";

            if (!item?.GetType()?.IsValueType == true && item?.GetType() != typeof(string))
            {
                var displayProperty = item?.GetType()?.GetProperty(DisplayMemberPath);
                var display = string.Join(listSep, items.Select(x => (displayProperty != null ? displayProperty.GetValue(x) : x)?.ToString()));
                Text = !string.IsNullOrEmpty(display) ? $"[{SelectedItems?.Count}] {display}" : string.Empty;
            }
            else Text = $"[{SelectedItems?.Count}] {string.Join(listSep, items)}";
        }
        else Text = string.Empty;
        */
    }
    #endregion

    #region Main properties
    /// ArrowVisibility
    public static readonly DependencyProperty ArrowVisibilityProperty
        = DependencyProperty.Register(
            nameof(ArrowVisibility),
            typeof(Visibility),
            typeof(StswComboView)
        );
    public Visibility ArrowVisibility
    {
        get => (Visibility)GetValue(ArrowVisibilityProperty);
        set => SetValue(ArrowVisibilityProperty, value);
    }

    /// Components
    public static readonly DependencyProperty ComponentsProperty
        = DependencyProperty.Register(
            nameof(Components),
            typeof(ObservableCollection<UIElement>),
            typeof(StswComboView)
        );
    public ObservableCollection<UIElement> Components
    {
        get => (ObservableCollection<UIElement>)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }
    /// ComponentsAlignment
    public static readonly DependencyProperty ComponentsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ComponentsAlignment),
            typeof(Dock),
            typeof(StswComboView)
        );
    public Dock ComponentsAlignment
    {
        get => (Dock)GetValue(ComponentsAlignmentProperty);
        set => SetValue(ComponentsAlignmentProperty, value);
    }

    /// IsDropDownOpen
    public static readonly DependencyProperty IsDropDownOpenProperty
        = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(StswComboView)
        );
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    /// IsReadOnly
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswComboView)
        );
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// Placeholder
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(StswComboView)
        );
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// SelectedItemsBinding
    public static readonly DependencyProperty SelectedItemsBindingProperty
        = DependencyProperty.Register(
            nameof(SelectedItemsBinding),
            typeof(IList),
            typeof(StswComboView),
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
        if (obj is StswComboView stsw)
            stsw.SetSelectedItems(stsw.SelectedItemsBinding);
    }

    /// Text
    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StswComboView)
        );
    public string Text
    {
        get => (string)GetValue(TextProperty);
        internal set => SetValue(TextProperty, value);
    }
    #endregion

    #region Spatial properties
    /// > BorderThickness ...
    /// PopupBorderThickness
    public static readonly DependencyProperty PopupBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(PopupBorderThickness),
            typeof(Thickness),
            typeof(StswComboView)
        );
    public Thickness PopupBorderThickness
    {
        get => (Thickness)GetValue(PopupBorderThicknessProperty);
        set => SetValue(PopupBorderThicknessProperty, value);
    }
    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswComboView)
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }

    /// > CornerRadius ...
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswComboView)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// > Height ...
    /// MaxDropDownHeight
    public static readonly DependencyProperty MaxDropDownHeightProperty
        = DependencyProperty.Register(
            nameof(MaxDropDownHeight),
            typeof(double?),
            typeof(StswComboView)
        );
    public double? MaxDropDownHeight
    {
        get => (double?)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }
    #endregion
}
