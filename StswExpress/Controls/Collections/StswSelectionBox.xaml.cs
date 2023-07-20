using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a control that combines the functionality of a <see cref="ComboBox"/> and <see cref="ListBox"/> to allow multiple selection.
/// </summary>
public class StswSelectionBox : UserControl
{
    public StswSelectionBox()
    {
        Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnPreviewMouseDownOutsideCapturedElement);
        
        SetValue(ComponentsProperty, new ObservableCollection<UIElement>());
    }
    static StswSelectionBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSelectionBox), new FrameworkPropertyMetadata(typeof(StswSelectionBox)));
    }

    #region Events
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        /// ListBox
        if (GetTemplateChild("PART_ListBox") is StswListBox listBox)
        {
            listBox.skipSelectionChanged = true;
            DataContextChanged += (s, e) => listBox.SelectionChanged -= OnSelectionChanged;
            listBox.SelectionChanged += OnSelectionChanged;
        }

        base.OnApplyTemplate();
    }

    /// <summary>
    /// Handles the selection changed event of the internal ListBox.
    /// Updates the collection of selected items and the source binding.
    /// </summary>
    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
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
        SetText();
    }

    /// <summary>
    /// Sets the text for the control based on the selected items.
    /// </summary>
    internal void SetText()
    {
        var listSeparator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";
        var selectedText = new StringBuilder();

        foreach (var selectedItem in SelectedItemsBinding)
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
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the visibility of the arrow icon in the drop button.
    /// </summary>
    public Visibility ArrowVisibility
    {
        get => (Visibility)GetValue(ArrowVisibilityProperty);
        set => SetValue(ArrowVisibilityProperty, value);
    }
    public static readonly DependencyProperty ArrowVisibilityProperty
        = DependencyProperty.Register(
            nameof(ArrowVisibility),
            typeof(Visibility),
            typeof(StswSelectionBox)
        );

    /// <summary>
    /// Gets or sets the collection of components to be displayed in the control.
    /// </summary>
    public ObservableCollection<UIElement> Components
    {
        get => (ObservableCollection<UIElement>)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }
    public static readonly DependencyProperty ComponentsProperty
        = DependencyProperty.Register(
            nameof(Components),
            typeof(ObservableCollection<UIElement>),
            typeof(StswSelectionBox)
        );

    /// <summary>
    /// Gets or sets the alignment of the components within the control.
    /// </summary>
    public Dock ComponentsAlignment
    {
        get => (Dock)GetValue(ComponentsAlignmentProperty);
        set => SetValue(ComponentsAlignmentProperty, value);
    }
    public static readonly DependencyProperty ComponentsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ComponentsAlignment),
            typeof(Dock),
            typeof(StswSelectionBox)
        );

    /// <summary>
    /// Gets or sets the path to the display string property of the items in the ItemsSource.
    /// </summary>
    public string? DisplayMemberPath
    {
        get => (string?)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }
    public static readonly DependencyProperty DisplayMemberPathProperty
        = DependencyProperty.Register(
            nameof(DisplayMemberPath),
            typeof(string),
            typeof(StswSelectionBox)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the drop-down portion of the button is open.
    /// </summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    public static readonly DependencyProperty IsDropDownOpenProperty
        = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(StswSelectionBox),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsDropDownOpenChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnIsDropDownOpenChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswSelectionBox stsw)
        {
            if (stsw.IsDropDownOpen)
                _ = Mouse.Capture(stsw, CaptureMode.SubTree);
            else
                _ = Mouse.Capture(null);
        }
    }
    private void OnPreviewMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the drop button is in read-only mode.
    /// When set to true, the popup with items is accessible, but all items within the popup are disabled.
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswSelectionBox)
        );

    /// <summary>
    /// Gets or sets the collection that is used to generate the content of the control.
    /// </summary>
    public IList ItemsSource
    {
        get => (IList)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    public static readonly DependencyProperty ItemsSourceProperty
        = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IList),
            typeof(StswSelectionBox)
        );

    /// <summary>
    /// Gets or sets the placeholder text displayed in the control when no item is selected.
    /// </summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(StswSelectionBox)
        );

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
            typeof(StswSelectionBox),
            new FrameworkPropertyMetadata(default(IList),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                null, null, false, UpdateSourceTrigger.PropertyChanged)
        );

    /// <summary>
    /// Gets or sets the path to the value property of the selected items in the ItemsSource.
    /// </summary>
    public string? SelectedValuePath
    {
        get => (string?)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }
    public static readonly DependencyProperty SelectedValuePathProperty
        = DependencyProperty.Register(
            nameof(SelectedValuePath),
            typeof(string),
            typeof(StswSelectionBox)
        );

    /// <summary>
    /// Gets or sets the text value of the control.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        internal set => SetValue(TextProperty, value);
    }
    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StswSelectionBox)
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
            typeof(StswSelectionBox)
        );

    /// <summary>
    /// Gets or sets the maximum height of the drop-down portion of the button.
    /// </summary>
    public double? MaxDropDownHeight
    {
        get => (double?)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }
    public static readonly DependencyProperty MaxDropDownHeightProperty
        = DependencyProperty.Register(
            nameof(MaxDropDownHeight),
            typeof(double?),
            typeof(StswSelectionBox)
        );

    /// <summary>
    /// Gets or sets the border thickness of the drop-down popup.
    /// </summary>
    public Thickness PopupBorderThickness
    {
        get => (Thickness)GetValue(PopupBorderThicknessProperty);
        set => SetValue(PopupBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty PopupBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(PopupBorderThickness),
            typeof(Thickness),
            typeof(StswSelectionBox)
        );
    #endregion
}
