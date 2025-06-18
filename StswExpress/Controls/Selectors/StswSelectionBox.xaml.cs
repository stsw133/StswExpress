using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;/// <summary>
/// A multi-selection combo box that allows users to select multiple items from a drop-down list.
/// Supports item binding, selection tracking, drop-down customization, and error indication.
/// </summary>
/// <remarks>
/// The <see cref="ItemsSource"/> must contain elements implementing <see cref="IStswSelectionItem"/>.
/// </remarks>
public class StswSelectionBox : ItemsControl, IStswBoxControl, IStswCornerControl, IStswDropControl
{
    private ListBox? _listBox;
    private Popup? _popup;
    bool IStswDropControl.SuppressNextOpen { get; set; }

    public StswSelectionBox()
    {
        Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, IStswDropControl.PreviewMouseDownOutsideCapturedElement);
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
    }
    static StswSelectionBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSelectionBox), new FrameworkPropertyMetadata(typeof(StswSelectionBox)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswSelectionBox), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// ensure the command is not null; if it wasn't set from outside, we create a default.
        UpdateTextCommand ??= new StswCommand(UpdateText);

        /// Popup
        if (GetTemplateChild("PART_Popup") is Popup popup)
            _popup = popup;

        /// ListBox
        if (GetTemplateChild("PART_ListBox") is ListBox listBox)
        {
            listBox.SelectionChanged += (_, _) => UpdateTextCommand?.Execute(null);
            _listBox = listBox;
        }
    }

    /// <summary>
    /// Called when the <see cref="ItemsSource"/> property changes.
    /// Validates item compatibility and updates selection tracking.
    /// </summary>
    /// <param name="oldValue">The previous <see cref="ItemsSource"/> collection.</param>
    /// <param name="newValue">The new <see cref="ItemsSource"/> collection.</param>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        if (newValue?.GetType()?.IsListType(out var innerType) == true)
        {
            if (innerType?.IsAssignableTo(typeof(IStswSelectionItem)) != true)
                throw new InvalidOperationException($"{nameof(StswSelectionBox)} ItemsSource must contain objects implementing {nameof(IStswSelectionItem)}!");

            /// Optional: If using StswComboItem (short usage), set defaults
            if (innerType?.IsAssignableTo(typeof(StswComboItem)) == true)
            {
                if (string.IsNullOrEmpty(DisplayMemberPath) && ItemTemplate == null)
                    DisplayMemberPath = nameof(StswComboItem.Display);
                if (string.IsNullOrEmpty(SelectedValuePath))
                    SelectedValuePath = nameof(StswComboItem.Value);
            }
        }

        base.OnItemsSourceChanged(oldValue, newValue);

        /// refresh displayed text whenever the ItemsSource changes.
        UpdateTextCommand?.Execute(null);
    }

    /// <summary>
    /// Called when the <see cref="ItemTemplate"/> property changes.
    /// Resets the <see cref="DisplayMemberPath"/> if a custom template is applied.
    /// </summary>
    /// <param name="oldItemTemplate">The previous data template for items.</param>
    /// <param name="newItemTemplate">The new data template for items.</param>
    protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        if (newItemTemplate != null && !string.IsNullOrEmpty(DisplayMemberPath))
            DisplayMemberPath = string.Empty;
        base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
    }

    /// <summary>
    /// Updates the displayed text based on the selected items.
    /// Also synchronizes the internal selection state.
    /// </summary>
    internal void UpdateText()
    {
        if (ItemsSource == null)
        {
            Text = string.Empty;
            return;
        }

        if (_popup?.IsLoaded == true && _listBox?.IsLoaded == false)
            return;

        var itemsSource = ItemsSource.OfType<IStswSelectionItem>();

        /// build text from all selected items
        var newlySelected = new ObservableCollection<IStswSelectionItem>();
        var sb = new StringBuilder();

        /// use the local list separator (e.g. ", ")
        var listSeparator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";

        foreach (var selectedItem in itemsSource.Where(x => x.IsSelected))
        {
            newlySelected.Add(selectedItem);

            /// if we have a DisplayMemberPath, try to get that property
            if (!string.IsNullOrEmpty(DisplayMemberPath) && selectedItem.GetType().GetProperty(DisplayMemberPath) is PropertyInfo propInfo)
            {
                var value = propInfo.GetValue(selectedItem)?.ToString();
                if (!string.IsNullOrEmpty(value))
                    sb.Append(value).Append(listSeparator);
            }
            else
            {
                sb.Append(selectedItem).Append(listSeparator);
            }
        }

        /// remove the trailing separator if needed
        if (sb.Length >= listSeparator.Length)
            sb.Length -= listSeparator.Length;

        /// final text
        Text = sb.ToString();
    }
    #endregion

    #region Logic properties
    /// <inheritdoc/>
    public ReadOnlyObservableCollection<ValidationError> Errors
    {
        get => (ReadOnlyObservableCollection<ValidationError>)GetValue(ErrorsProperty);
        set => SetValue(ErrorsProperty, value);
    }
    public static readonly DependencyProperty ErrorsProperty
        = DependencyProperty.Register(
            nameof(Errors),
            typeof(ReadOnlyObservableCollection<ValidationError>),
            typeof(StswSelectionBox)
        );

    /// <inheritdoc/>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }
    public static readonly DependencyProperty HasErrorProperty
        = DependencyProperty.Register(
            nameof(HasError),
            typeof(bool),
            typeof(StswSelectionBox)
        );

    /// <inheritdoc/>
    public object? Icon
    {
        get => (object?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public static readonly DependencyProperty IconProperty
        = DependencyProperty.Register(
            nameof(Icon),
            typeof(object),
            typeof(StswSelectionBox)
        );

    /// <inheritdoc/>
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
    private static void OnIsDropDownOpenChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) => IStswDropControl.IsDropDownOpenChanged(obj, e);

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
    /// Gets or sets the property path used to retrieve the value of selected items.
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

    /// <inheritdoc/>
    public ObservableCollection<IStswSubControl> SubControls
    {
        get => (ObservableCollection<IStswSubControl>)GetValue(SubControlsProperty);
        set => SetValue(SubControlsProperty, value);
    }
    public static readonly DependencyProperty SubControlsProperty
        = DependencyProperty.Register(
            nameof(SubControls),
            typeof(ObservableCollection<IStswSubControl>),
            typeof(StswSelectionBox)
        );

    /// <summary>
    /// Gets or sets the text representation of the selected items.
    /// This property updates dynamically based on selection changes.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StswSelectionBox),
            new FrameworkPropertyMetadata(default(string),
                FrameworkPropertyMetadataOptions.None,
                null, null, false, UpdateSourceTrigger.PropertyChanged)
        );

    /// <summary>
    /// Gets or sets the command that updates the displayed text based on selected items.
    /// </summary>
    public ICommand UpdateTextCommand
    {
        get => (ICommand)GetValue(UpdateTextCommandProperty);
        set => SetValue(UpdateTextCommandProperty, value);
    }
    public static readonly DependencyProperty UpdateTextCommandProperty
        = DependencyProperty.Register(
            nameof(UpdateTextCommand),
            typeof(ICommand),
            typeof(StswSelectionBox)
        );
    #endregion

    #region Style properties
    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswSelectionBox),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswSelectionBox),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }
    public static readonly DependencyProperty MaxDropDownHeightProperty
        = DependencyProperty.Register(
            nameof(MaxDropDownHeight),
            typeof(double),
            typeof(StswSelectionBox),
            new PropertyMetadata(SystemParameters.PrimaryScreenHeight / 3)
        );

    /// <inheritdoc/>
    public double MaxDropDownWidth
    {
        get => (double)GetValue(MaxDropDownWidthProperty);
        set => SetValue(MaxDropDownWidthProperty, value);
    }
    public static readonly DependencyProperty MaxDropDownWidthProperty
        = DependencyProperty.Register(
            nameof(MaxDropDownWidth),
            typeof(double),
            typeof(StswSelectionBox),
            new PropertyMetadata(double.NaN)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between the drop-down button and the main input field.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty
        = DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswSelectionBox),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswSelectionBox ItemsSource="{Binding Tags}" Placeholder="Select tags"/>

*/
