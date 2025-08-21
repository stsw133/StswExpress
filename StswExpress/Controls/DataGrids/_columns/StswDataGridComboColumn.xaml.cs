using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a combo box column for <see cref="StswDataGrid"/> that allows selecting values from a dropdown list.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDataGridComboColumn Header="Category" SelectedItemBinding="{Binding SelectedCategory}" ItemsSource="{Binding Categories}" DisplayMemberPath="Name"/&gt;
/// </code>
/// </example>
[StswInfo("0.13.0", "0.20.0")]
public class StswDataGridComboColumn : DataGridComboBoxColumn
{
    private static readonly Style StswEditingElementStyle = new(typeof(StswComboBox), (Style)Application.Current.FindResource(typeof(StswComboBox)))
    {
        Setters =
        {
            new Setter(StswComboBox.BorderThicknessProperty, new Thickness(0)),
            new Setter(StswComboBox.CornerClippingProperty, false),
            new Setter(StswComboBox.CornerRadiusProperty, new CornerRadius(0)),
            new Setter(StswComboBox.FocusVisualStyleProperty, null),
            new Setter(StswComboBox.SeparatorThicknessProperty, 0.0),
            new Setter(StswComboBox.IsDropDownOpenProperty, true),
            new Setter(StswComboBox.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(StswComboBox.VerticalAlignmentProperty, VerticalAlignment.Stretch),
            new Setter(StswPopup.CornerClippingProperty, false),
            new Setter(StswPopup.CornerRadiusProperty, new CornerRadius(0))
        }
    };

    /// <inheritdoc/>
    [StswInfo("0.13.0", "0.20.0")]
    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        cell.PreviewKeyDown += OnPreviewKeyDown;

        var displayElement = new StswText()
        {
            Margin = new Thickness(2, 0, 2, 0),
            Padding = Padding,
            TextAlignment = TextAlignment,
            TextTrimming = TextTrimming,
            TextWrapping = TextWrapping
        };

        if (dataItem == CollectionView.NewItemPlaceholder || dataItem == null)
        {
            displayElement.Text = "";
            return displayElement;
        }

        /// bindings
        if (SelectedItemBinding is Binding selectedItemBinding && selectedItemBinding.Path?.Path is string selectedItemPath && !string.IsNullOrEmpty(DisplayMemberPath))
        {
            BindingOperations.SetBinding(displayElement, StswText.TextProperty, new Binding
            {
                Path = new PropertyPath($"{selectedItemPath}.{DisplayMemberPath}"),
                Mode = BindingMode.OneWay
            });
        }
        else if (SelectedValueBinding != null && ItemsSource != null && !string.IsNullOrEmpty(DisplayMemberPath))
        {
            BindingOperations.SetBinding(displayElement, StswText.TextProperty, new MultiBinding
            {
                Converter = new SelectedValueToDisplayConverter(SelectedValuePath, DisplayMemberPath),
                Bindings =
                {
                    SelectedValueBinding,
                    new Binding { Source = ItemsSource }
                }
            });
        }
        else if (SelectedValueBinding != null)
        {
            BindingOperations.SetBinding(displayElement, StswText.TextProperty, SelectedValueBinding);
        }

        return displayElement;
    }

    /// <inheritdoc/>
    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        var editingElement = new StswComboBox()
        {
            Style = StswEditingElementStyle,
            ItemsSource = ItemsSource,
            Padding = Padding,
            Placeholder = Placeholder,
            HorizontalContentAlignment = HorizontalContentAlignment,
            VerticalContentAlignment = VerticalContentAlignment
        };

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

    #region Logic properties
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
    #endregion

    #region Style properties
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
    /// Gets or sets the horizontal text alignment for both display and editing elements in the column.
    /// </summary>
    [StswInfo("0.16.0")]
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
    [StswInfo("0.16.0")]
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
    [StswInfo("0.16.1")]
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

    /// <summary>
    /// Converter that retrieves the display value for a selected item in a combo box.
    /// </summary>
    [StswInfo("0.19.0", "0.20.0")]
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
                var itemType = item.GetType();
                var valueProp = !string.IsNullOrEmpty(selectedValuePath)
                    ? itemType.GetProperty(selectedValuePath)
                    : null;
                var displayProp = !string.IsNullOrEmpty(displayMemberPath)
                    ? itemType.GetProperty(displayMemberPath)
                    : null;

                var itemValue = valueProp?.GetValue(item) ?? item;
                if (Equals(itemValue, selectedValue))
                {
                    var display = displayProp?.GetValue(item)?.ToString() ?? item.ToString() ?? string.Empty;
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
