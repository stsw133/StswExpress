using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress.Wpf;

/// <summary>
/// Represents a combo box column for <see cref="StswDataGrid"/> that allows selecting values from a dropdown list.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDataGridComboColumn Header="Category" SelectedItemBinding="{Binding SelectedCategory}" ItemsSource="{Binding Categories}" DisplayMemberPath="Name"/&gt;
/// </code>
/// </example>
public class StswDataGridComboColumn : DataGridComboBoxColumn
{
    #region Dependency properties
    /// <summary>
    /// Gets or sets the property path used for filtering the combo box items.
    /// </summary>
    public string FilterMemberPath
    {
        get => (string)GetValue(FilterMemberPathProperty);
        set => SetValue(FilterMemberPathProperty, value);
    }
    public static readonly DependencyProperty FilterMemberPathProperty
        = DependencyProperty.Register(
            nameof(FilterMemberPath),
            typeof(string),
            typeof(StswDataGridComboColumn)
        );

    /// <summary>
    /// Gets or sets the text value used for filtering the combo box items.
    /// </summary>
    public string FilterText
    {
        get => (string)GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }
    public static readonly DependencyProperty FilterTextProperty
        = DependencyProperty.Register(
            nameof(FilterText),
            typeof(string),
            typeof(StswDataGridComboColumn)
        );

    /// <summary>
    /// Gets or sets a value indicating whether filtering is enabled for the combo box.
    /// </summary>
    public bool IsFilterEnabled
    {
        get => (bool)GetValue(IsFilterEnabledProperty);
        set => SetValue(IsFilterEnabledProperty, value);
    }
    public static readonly DependencyProperty IsFilterEnabledProperty
        = DependencyProperty.Register(
            nameof(IsFilterEnabled),
            typeof(bool),
            typeof(StswDataGridComboColumn)
        );

    /// <summary>
    /// Gets or sets the padding around the content inside the column's cells.
    /// </summary>
    public Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }
    public static readonly DependencyProperty PaddingProperty
        = DependencyProperty.Register(
            nameof(Padding),
            typeof(Thickness),
            typeof(StswDataGridComboColumn)
        );

    /// <summary>
    /// Gets or sets the placeholder text displayed in the combo box when no value is selected.
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
            typeof(StswDataGridComboColumn)
        );

    /// <summary>
    /// Gets or sets the horizontal text alignment for both display and editing elements in the column.
    /// </summary>
    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }
    public static readonly DependencyProperty TextAlignmentProperty
        = DependencyProperty.Register(
            nameof(TextAlignment),
            typeof(TextAlignment),
            typeof(StswDataGridComboColumn)
        );

    /// <summary>
    /// Gets or sets how the text is trimmed when it overflows the available width in the display element.
    /// </summary>
    public TextTrimming TextTrimming
    {
        get => (TextTrimming)GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }
    public static readonly DependencyProperty TextTrimmingProperty
        = DependencyProperty.Register(
            nameof(TextTrimming),
            typeof(TextTrimming),
            typeof(StswDataGridComboColumn)
        );

    /// <summary>
    /// Gets or sets whether the text wraps within the column's cells when it exceeds the available space.
    /// </summary>
    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }
    public static readonly DependencyProperty TextWrappingProperty
        = DependencyProperty.Register(
            nameof(TextWrapping),
            typeof(TextWrapping),
            typeof(StswDataGridComboColumn),
            new PropertyMetadata(TextWrapping.NoWrap)
        );

    /// <summary>
    /// Gets or sets the horizontal alignment of the combo box inside the editing element.
    /// </summary>
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }
    public static readonly DependencyProperty HorizontalContentAlignmentProperty
        = DependencyProperty.Register(
            nameof(HorizontalContentAlignment),
            typeof(HorizontalAlignment),
            typeof(StswDataGridComboColumn),
            new PropertyMetadata(HorizontalAlignment.Left)
        );

    /// <summary>
    /// Gets or sets the vertical alignment of the combo box inside the editing element.
    /// </summary>
    public VerticalAlignment VerticalContentAlignment
    {
        get => (VerticalAlignment)GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }
    public static readonly DependencyProperty VerticalContentAlignmentProperty
        = DependencyProperty.Register(
            nameof(VerticalContentAlignment),
            typeof(VerticalAlignment),
            typeof(StswDataGridComboColumn),
            new PropertyMetadata(VerticalAlignment.Top)
        );
    #endregion

    #region Overrides
    private static readonly Style StswEditingElementStyle = new(typeof(StswComboBox), (Style)Application.Current.FindResource(typeof(StswComboBox)))
    {
        Setters =
        {
            new Setter(StswComboBox.BorderThicknessProperty, new Thickness(0)),
            new Setter(StswComboBox.CornerClippingProperty, false),
            new Setter(StswComboBox.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswComboBox.FocusVisualStyleProperty, null),
            new Setter(StswComboBox.IsDropDownOpenProperty, true),
            new Setter(StswComboBox.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswComboBox.VerticalAlignmentProperty, VerticalAlignment.Stretch),
            new Setter(StswPopup.CornerClippingProperty, false),
            new Setter(StswPopup.CornerRadiusProperty, new CornerRadius(0))
        }
    };

    /// <inheritdoc/>
    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        cell.PreviewKeyDown += OnPreviewKeyDown;

        var displayElement = new StswText()
        {
            Margin = new Thickness(2, 0, 2, 0)
        };
        displayElement.SetBinding(TextBlock.PaddingProperty, this.CreateColumnBinding(nameof(Padding)));
        displayElement.SetBinding(TextBlock.TextAlignmentProperty, this.CreateColumnBinding(nameof(TextAlignment)));
        displayElement.SetBinding(TextBlock.TextTrimmingProperty, this.CreateColumnBinding(nameof(TextTrimming)));
        displayElement.SetBinding(TextBlock.TextWrappingProperty, this.CreateColumnBinding(nameof(TextWrapping)));

        if (dataItem == CollectionView.NewItemPlaceholder || dataItem == null)
        {
            displayElement.Text = "";
            return displayElement;
        }

        /// bindings
        var itemsSourceBinding = GetClonedItemsSourceBinding();

        if (SelectedItemBinding is Binding selectedItemBinding && selectedItemBinding.Path?.Path is string selectedItemPath && !string.IsNullOrEmpty(DisplayMemberPath))
        {
            BindingOperations.SetBinding(displayElement, TextBlock.TextProperty, new Binding
            {
                Path = new PropertyPath($"{selectedItemPath}.{DisplayMemberPath}"),
                Mode = BindingMode.OneWay
            });
        }
        else if (SelectedValueBinding != null && (itemsSourceBinding != null || ItemsSource != null) && !string.IsNullOrEmpty(DisplayMemberPath))
        {
            BindingOperations.SetBinding(displayElement, TextBlock.TextProperty, new MultiBinding
            {
                Converter = new SelectedValueToDisplayConverter(SelectedValuePath, DisplayMemberPath),
                Bindings =
                {
                    SelectedValueBinding,
                    itemsSourceBinding ?? new Binding { Source = ItemsSource }
                }
            });
        }
        else if (SelectedValueBinding != null)
        {
            BindingOperations.SetBinding(displayElement, TextBlock.TextProperty, SelectedValueBinding);
        }

        return displayElement;
    }

    /// <inheritdoc/>
    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        var editingElement = new StswComboBox()
        {
            Style = StswEditingElementStyle
        };

        var itemsSourceBinding = GetClonedItemsSourceBinding();
        if (itemsSourceBinding != null)
            BindingOperations.SetBinding(editingElement, ItemsControl.ItemsSourceProperty, itemsSourceBinding);
        else
            editingElement.ItemsSource = ItemsSource;

        editingElement.SetBinding(StswComboBox.PaddingProperty, this.CreateColumnBinding(nameof(Padding)));
        editingElement.SetBinding(StswComboBox.PlaceholderProperty, this.CreateColumnBinding(nameof(Placeholder)));
        editingElement.SetBinding(StswComboBox.HorizontalContentAlignmentProperty, this.CreateColumnBinding(nameof(HorizontalContentAlignment)));
        editingElement.SetBinding(StswComboBox.VerticalContentAlignmentProperty, this.CreateColumnBinding(nameof(VerticalContentAlignment)));

        /// bindings
        if (SelectedItemBinding != null)
            BindingOperations.SetBinding(editingElement, StswComboBox.SelectedItemProperty, SelectedItemBinding);
        if (SelectedValueBinding != null)
            BindingOperations.SetBinding(editingElement, StswComboBox.SelectedValueProperty, SelectedValueBinding);

        return editingElement;
    }

    /// <summary>
    /// Handles key press events within the column. Ensures that pressing any key (except Tab)
    /// switches the cell to editing mode.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The key event arguments.</param>
    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
            return;

        if (sender is DataGridCell cell && !cell.IsEditing)
            cell.IsEditing = true;
    }

    /// <summary>
    /// Returns a clone of the binding associated with the column's <see cref="ItemsSourceProperty"/>, if any.
    /// </summary>
    private BindingBase? GetClonedItemsSourceBinding()
    {
        var binding = BindingOperations.GetBinding(this, ItemsSourceProperty);
        if (binding != null)
            return binding.Clone();

        var multiBinding = BindingOperations.GetMultiBinding(this, ItemsSourceProperty);
        if (multiBinding != null)
            return multiBinding.Clone();

        var priorityBinding = BindingOperations.GetPriorityBinding(this, ItemsSourceProperty);
        if (priorityBinding != null)
            return priorityBinding.Clone();

        return null;
    }
    #endregion

    /// <summary>
    /// Converter that retrieves the display value for a selected item in a combo box.
    /// </summary>
    private class SelectedValueToDisplayConverter(string? selectedValuePath, string? displayMemberPath) : IMultiValueConverter
    {
        private readonly string? selectedValuePath = selectedValuePath;
        private readonly string? displayMemberPath = displayMemberPath;
        private readonly Dictionary<object, string> _cache = [];

        /// <inheritdoc/>
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] == null || values[1] is not IEnumerable items)
                return null;

            var selectedValue = values[0];
            if (_cache.TryGetValue(selectedValue, out var cached))
                return cached;

            foreach (var item in items)
            {
                var itemValue = !string.IsNullOrEmpty(selectedValuePath)
                    ? item.GetPropertyValue(selectedValuePath)
                    : item;

                if (Equals(itemValue, selectedValue))
                {
                    var displayValue = !string.IsNullOrEmpty(displayMemberPath)
                        ? item.GetPropertyValue(displayMemberPath)
                        : item;
                    var display = displayValue?.ToString() ?? item.ToString() ?? string.Empty;
                    _cache[selectedValue] = display;
                    return display;
                }
            }

            var fallback = selectedValue.ToString() ?? string.Empty;
            _cache[selectedValue] = fallback;
            return fallback;
        }

        /// <inheritdoc/>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
