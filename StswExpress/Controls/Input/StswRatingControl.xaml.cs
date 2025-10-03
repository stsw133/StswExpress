using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A rating control that allows users to select a rating level.
/// Supports customizable number of items, reset option, and different expand directions.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswRatingControl Value="{Binding ProductRating}" ItemsNumber="10" IconData="{StaticResource StarIcon}" Direction="Right"/&gt;
/// </code>
/// </example>
public class StswRatingControl : Control, IStswIconControl
{
    public StswRatingControl()
    {
        SetValue(ItemsProperty, new ObservableCollection<StswRatingItem>());
    }
    static StswRatingControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswRatingControl), new FrameworkPropertyMetadata(typeof(StswRatingControl)));
    }

    #region Events & methods
    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Tab)
            return;

        if (IsReadOnly)
            return;

        switch (e.Key)
        {
            case Key.Back when IsResetEnabled:
                Value = 0;
                break;
            case Key.D1:
            case Key.NumPad1:
            case Key.D2:
            case Key.NumPad2:
            case Key.D3:
            case Key.NumPad3:
            case Key.D4:
            case Key.NumPad4:
            case Key.D5:
            case Key.NumPad5:
            case Key.D6:
            case Key.NumPad6:
            case Key.D7:
            case Key.NumPad7:
            case Key.D8:
            case Key.NumPad8:
            case Key.D9:
            case Key.NumPad9:
            case Key.D0:
            case Key.NumPad0:
                var key = e.Key.ToString().Last();
                var numericValue = key == '0' ? 10 : int.Parse(key.ToString());
                if (ItemsNumber >= numericValue)
                    Value = numericValue;
                break;
            case Key.Down:
                if (Direction == ExpandDirection.Down)
                    Increment(-Step);
                else if (Direction == ExpandDirection.Up)
                    Increment(Step);
                break;
            case Key.Up:
                if (Direction == ExpandDirection.Up)
                    Increment(-Step);
                else if (Direction == ExpandDirection.Down)
                    Increment(Step);
                break;
            case Key.Left:
                if (Direction == ExpandDirection.Right)
                    Increment(-Step);
                else if (Direction == ExpandDirection.Left)
                    Increment(Step);
                break;
            case Key.Right:
                if (Direction == ExpandDirection.Left)
                    Increment(-Step);
                else if (Direction == ExpandDirection.Right)
                    Increment(Step);
                break;
        }

        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.LeftButton == MouseButtonState.Pressed && !IsReadOnly)
            Value = Placeholder;

        Focus();
        Keyboard.Focus(this);

        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var position = e.GetPosition(this);
        var fraction = Math.Round(Direction switch
        {
            ExpandDirection.Down => position.Y / ActualHeight,
            ExpandDirection.Left => (ActualWidth - position.X) / ActualWidth,
            ExpandDirection.Right => position.X / ActualWidth,
            ExpandDirection.Up => (ActualHeight - position.Y) / ActualHeight,
            _ => 0
        } * Items.Count / Step) * Step;

        if (fraction < 0)
            fraction = 0;
        if (fraction > ItemsNumber)
            fraction = ItemsNumber;

        Placeholder = fraction;

        if (!IsResetEnabled && Placeholder == 0)
            Placeholder = 1;
    }

    /// <inheritdoc/>
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        Placeholder = null;
    }

    /// <summary>
    /// Increments the current rating value by the specified direction.
    /// </summary>
    /// <param name="direction">The direction to increment: -1 for decrement, 1 for increment.</param>
    private void Increment(double direction)
    {
        if (!Value.HasValue)
            Value = 0;

        double current = Value.Value;
        double newVal = current + direction;

        if (newVal < 0 && IsResetEnabled)
            newVal = 0;
        else if (newVal < 1 && !IsResetEnabled)
            newVal = 1;

        if (newVal > ItemsNumber)
            newVal = ItemsNumber;

        Value = newVal;
    }

    /// <summary>
    /// Updates the fill fractions of each rating item based on the current value.
    /// </summary>
    /// <param name="tempValue">Optional temporary value to use instead of the current Value.</param>
    /// <param name="isPlaceholder">Indicates if the update is for a placeholder (mouse hover) state.</param>
    private void UpdateFillFractions(double? tempValue = null, bool isPlaceholder = false)
    {
        double actualValue = tempValue ?? Value ?? 0.0;

        foreach (var item in Items)
        {
            item.FillFraction = 0;
            if (!isPlaceholder)
                item.IsChecked = false;
            item.IsMouseOver = false;
        }

        foreach (var item in Items)
        {
            double index = item.Value;
            double lowerBound = index - 1.0;
            double upperBound = index;

            if (actualValue >= upperBound)
            {
                item.FillFraction = 1.0;
                if (!isPlaceholder)
                    item.IsChecked = true;
            }
            else if (actualValue <= lowerBound)
            {
                item.FillFraction = 0.0;
            }
            else
            {
                item.FillFraction = actualValue - lowerBound;
            }

            if (isPlaceholder)
                item.IsMouseOver = item.FillFraction > 0;
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the expansion direction of the rating items.
    /// Determines how rating items are laid out (Left, Right, Up, or Down).
    /// </summary>
    public ExpandDirection Direction
    {
        get => (ExpandDirection)GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }
    public static readonly DependencyProperty DirectionProperty
        = DependencyProperty.Register(
            nameof(Direction),
            typeof(ExpandDirection),
            typeof(StswRatingControl),
            new FrameworkPropertyMetadata(default(ExpandDirection), OnDirectionChanged)
        );
    private static void OnDirectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswRatingControl stsw)
            return;

        if (stsw.Direction.In(ExpandDirection.Left, ExpandDirection.Up))
            stsw.Items = [.. stsw.Items.OrderByDescending(x => x.Value)];
        else
            stsw.Items = [.. stsw.Items.OrderBy(x => x.Value)];
    }

    /// <inheritdoc/>
    public Geometry? IconData
    {
        get => (Geometry?)GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }
    public static readonly DependencyProperty IconDataProperty
        = DependencyProperty.Register(
            nameof(IconData),
            typeof(Geometry),
            typeof(StswRatingControl)
        );

    /// <inheritdoc/>
    public GridLength IconScale
    {
        get => (GridLength)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }
    public static readonly DependencyProperty IconScaleProperty
        = DependencyProperty.Register(
            nameof(IconScale),
            typeof(GridLength),
            typeof(StswRatingControl)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the rating control is read-only.
    /// When set to <see langword="true"/>, user input (mouse and keyboard) is disabled.
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
            typeof(StswRatingControl)
        );

    /// <summary>
    /// Gets or sets whether the rating control allows resetting to zero.
    /// If enabled, users can clear the rating by pressing the Backspace key.
    /// </summary>
    public bool IsResetEnabled
    {
        get => (bool)GetValue(IsResetEnabledProperty);
        set => SetValue(IsResetEnabledProperty, value);
    }
    public static readonly DependencyProperty IsResetEnabledProperty
        = DependencyProperty.Register(
            nameof(IsResetEnabled),
            typeof(bool),
            typeof(StswRatingControl)
        );

    /// <summary>
    /// Gets or sets the collection of rating items.
    /// Each item represents a selectable rating level.
    /// </summary>
    internal ObservableCollection<StswRatingItem> Items
    {
        get => (ObservableCollection<StswRatingItem>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }
    internal static readonly DependencyProperty ItemsProperty
        = DependencyProperty.Register(
            nameof(Items),
            typeof(ObservableCollection<StswRatingItem>),
            typeof(StswRatingControl)
        );

    /// <summary>
    /// Gets or sets the number of rating items (icons).
    /// Defines how many selectable levels exist within the control.
    /// </summary>
    public int ItemsNumber
    {
        get => (int)GetValue(ItemsNumberProperty);
        set => SetValue(ItemsNumberProperty, value);
    }
    public static readonly DependencyProperty ItemsNumberProperty
        = DependencyProperty.Register(
            nameof(ItemsNumber),
            typeof(int),
            typeof(StswRatingControl),
            new FrameworkPropertyMetadata(default(int),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnItemsNumberChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnItemsNumberChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswRatingControl stsw)
            return;

        var val = (int)e.NewValue;
        if (val < 0)
            val = 0;

        if (val > stsw.Items.Count)
        {
            for (var i = stsw.Items.Count + 1; i <= val; i++)
            {
                var newItem = new StswRatingItem { Value = i };
                if (stsw.Direction.In(ExpandDirection.Left, ExpandDirection.Up))
                    stsw.Items.Insert(0, newItem);
                else
                    stsw.Items.Add(newItem);
            }
        }
        else if (val < stsw.Items.Count)
        {
            for (var i = stsw.Items.Count - 1; i >= val; i--)
            {
                if (stsw.Direction.In(ExpandDirection.Left, ExpandDirection.Up))
                    stsw.Items.RemoveAt(0);
                else
                    stsw.Items.RemoveAt(stsw.Items.Count - 1);
            }
        }

        if (stsw.Value.HasValue && stsw.Value.Value > val)
            stsw.Value = val;
        else
            stsw.UpdateFillFractions();
    }

    /// <summary>
    /// Gets or sets the visibility of the rating item count.
    /// This property controls whether the number of rating levels is displayed.
    /// </summary>
    public Visibility ItemsNumberVisibility
    {
        get => (Visibility)GetValue(ItemsNumberVisibilityProperty);
        set => SetValue(ItemsNumberVisibilityProperty, value);
    }
    public static readonly DependencyProperty ItemsNumberVisibilityProperty
        = DependencyProperty.Register(
            nameof(ItemsNumberVisibility),
            typeof(Visibility),
            typeof(StswRatingControl)
        );

    /// <summary>
    /// Gets or sets a temporary rating value based on mouse hover.
    /// Provides a visual preview of the rating before selection.
    /// </summary>
    public double? Placeholder
    {
        get => (double?)GetValue(PlaceholderProperty);
        internal set => SetValue(PlaceholderProperty, value);
    }
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(double?),
            typeof(StswRatingControl),
            new FrameworkPropertyMetadata(default(double?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnPlaceholderChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnPlaceholderChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswRatingControl stsw)
            return;

        if (stsw.Placeholder.HasValue)
            stsw.UpdateFillFractions(stsw.Placeholder.Value, isPlaceholder: true);
        else
            stsw.UpdateFillFractions();
    }

    /// <summary>
    /// Gets or sets the step value for the rating control.
    /// </summary>
    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }
    public static readonly DependencyProperty StepProperty
        = DependencyProperty.Register(
            nameof(Step),
            typeof(double),
            typeof(StswRatingControl)
        );

    /// <summary>
    /// Gets or sets the currently selected rating value.
    /// Represents the user's chosen rating level within the control.
    /// </summary>
    public double? Value
    {
        get => (double?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    public static readonly DependencyProperty ValueProperty
        = DependencyProperty.Register(
            nameof(Value),
            typeof(double?),
            typeof(StswRatingControl),
            new FrameworkPropertyMetadata(default(double?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswRatingControl stsw)
            return;

        if (stsw.Value is < 0 && stsw.IsResetEnabled)
            stsw.Value = 0;
        if (stsw.Value > stsw.Items.Count)
            stsw.Value = stsw.Items.Count;

        stsw.UpdateFillFractions();
    }
    #endregion

    #region Style properties
    /// <inheritdoc/>
    public Brush IconFill
    {
        get => (Brush)GetValue(IconFillProperty);
        set => SetValue(IconFillProperty, value);
    }
    public static readonly DependencyProperty IconFillProperty
        = DependencyProperty.Register(
            nameof(IconFill),
            typeof(Brush),
            typeof(StswRatingControl),
            new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public Brush IconStroke
    {
        get => (Brush)GetValue(IconStrokeProperty);
        set => SetValue(IconStrokeProperty, value);
    }
    public static readonly DependencyProperty IconStrokeProperty
        = DependencyProperty.Register(
            nameof(IconStroke),
            typeof(Brush),
            typeof(StswRatingControl),
            new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public double IconStrokeThickness
    {
        get => (double)GetValue(IconStrokeThicknessProperty);
        set => SetValue(IconStrokeThicknessProperty, value);
    }
    public static readonly DependencyProperty IconStrokeThicknessProperty
        = DependencyProperty.Register(
            nameof(IconStrokeThickness),
            typeof(double),
            typeof(StswRatingControl),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion

    #region Excluded properties
    /// The following properties are hidden from the designer and serialization:

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Brush? BorderBrush { get; private set; }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Thickness? BorderThickness { get; private set; }
    #endregion
}

/// <summary>
/// Converts a rating value and item index to a brush mask for partial fills.
/// </summary>
internal sealed class StswRatingMaskConverter : IMultiValueConverter
{
    /// <summary>
    /// Converts the rating value and item index to a LinearGradientBrush mask.
    /// </summary>
    /// <param name="values">An array containing the rating value, item index, and expand direction.</param>
    /// <param name="targetType">The target type of the conversion (expected to be Brush).</param>
    /// <param name="parameter">An optional parameter (not used).</param>
    /// <param name="culture">The culture info (not used).</param>
    /// <returns>A LinearGradientBrush representing the fill mask for the rating item.</returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is not [object rawVal, object idxObj, object dirObj])
            return Brushes.Transparent;

        var v = rawVal is double d ? d : (rawVal as double?) ?? 0.0;
        var idx = idxObj is int i ? i : 0;
        var dir = dirObj is ExpandDirection ed ? ed : ExpandDirection.Right;

        var lower = idx - 1.0;
        var upper = idx;
        var frac = v >= upper ? 1.0 : (v <= lower ? 0.0 : v - lower);

        frac = Math.Round(frac * 256.0) / 256.0;
        return MakeMask(frac, dir);
    }

    /// <summary>
    /// Not implemented. Converts back is not supported.
    /// </summary>
    /// <param name="value">The value produced by the binding target (not used).</param>
    /// <param name="targetTypes">The array of target types (not used).</param>
    /// <param name="parameter">An optional parameter (not used).</param>
    /// <param name="culture">The culture info (not used).</param>
    /// <returns>Throws NotSupportedException.</returns>
    /// <exception cref="NotSupportedException">Always thrown since ConvertBack is not supported.</exception>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

    /// <summary>
    /// Creates a LinearGradientBrush mask based on the fill fraction and expand direction.
    /// </summary>
    /// <param name="fraction">The fill fraction (0 to 1) indicating how much of the item should be filled.</param>
    /// <param name="dir">The direction in which the rating items expand.</param>
    /// <returns>A LinearGradientBrush representing the fill mask.</returns>
    private static LinearGradientBrush MakeMask(double fraction, ExpandDirection dir)
    {
        if (fraction < 0) fraction = 0;
        if (fraction > 1) fraction = 1;

        var b = new LinearGradientBrush { MappingMode = BrushMappingMode.RelativeToBoundingBox };

        switch (dir)
        {
            case ExpandDirection.Right: b.StartPoint = new Point(0, 0); b.EndPoint = new Point(1, 0); break;
            case ExpandDirection.Left: b.StartPoint = new Point(1, 0); b.EndPoint = new Point(0, 0); break;
            case ExpandDirection.Down: b.StartPoint = new Point(0, 0); b.EndPoint = new Point(0, 1); break;
            case ExpandDirection.Up: b.StartPoint = new Point(0, 1); b.EndPoint = new Point(0, 0); break;
        }

        b.GradientStops.Add(new GradientStop(Colors.White, 0));
        b.GradientStops.Add(new GradientStop(Colors.White, fraction));
        b.GradientStops.Add(new GradientStop(Colors.Transparent, fraction));
        b.GradientStops.Add(new GradientStop(Colors.Transparent, 1));

        if (b.CanFreeze) b.Freeze();
        return b;
    }
}
