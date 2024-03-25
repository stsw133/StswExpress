using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a control that allows users to view and set ratings that reflect degrees of satisfaction with content and services.
/// </summary>
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
    /// <summary>
    /// Handles the key down event.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Tab)
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
                if (Direction == ExpandDirection.Down && Value < ItemsNumber)
                    Value += 1;
                else if (Direction == ExpandDirection.Up && Value > 0 && !(Value == 1 && !IsResetEnabled))
                    Value -= 1;
                break;
            case Key.Left:
                if (Direction == ExpandDirection.Left && Value < ItemsNumber)
                    Value += 1;
                else if (Direction == ExpandDirection.Right && Value > 0 && !(Value == 1 && !IsResetEnabled))
                    Value -= 1;
                break;
            case Key.Right:
                if (Direction == ExpandDirection.Right && Value < ItemsNumber)
                    Value += 1;
                else if (Direction == ExpandDirection.Left && Value > 0 && !(Value == 1 && !IsResetEnabled))
                    Value -= 1;
                break;
            case Key.Up:
                if (Direction == ExpandDirection.Up && Value < ItemsNumber)
                    Value += 1;
                else if (Direction == ExpandDirection.Down && Value > 0 && !(Value == 1 && !IsResetEnabled))
                    Value -= 1;
                break;
        }
        e.Handled = true;
    }

    /// <summary>
    /// Handles the mouse down event.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.LeftButton == MouseButtonState.Pressed)
            Value = Placeholder;
        Focus();
    }

    /// <summary>
    /// Handles the mouse move event.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var position = e.GetPosition(this);
        var percentage = Direction switch
        {
            ExpandDirection.Down => position.Y / ActualHeight,
            ExpandDirection.Left => (ActualWidth - position.X) / ActualWidth,
            ExpandDirection.Right => position.X / ActualWidth,
            ExpandDirection.Up => (Items.Count - position.Y / ActualHeight),
            _ => 0
        };
        Placeholder = Convert.ToInt32(Math.Round(percentage * Items.Count + 0.4));

        if (!IsResetEnabled && Placeholder == 0)
            Placeholder = 1;
    }

    /// <summary>
    /// Handles the mouse leave event.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        Placeholder = null;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the direction of the control.
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
        if (obj is StswRatingControl stsw)
        {
            if (stsw.Direction.In(ExpandDirection.Left, ExpandDirection.Up))
                stsw.Items = stsw.Items.OrderByDescending(x => x.Value).ToObservableCollection();
            else
                stsw.Items = stsw.Items.OrderBy(x => x.Value).ToObservableCollection();
        }
    }

    /// <summary>
    /// Gets or sets the geometry data of the icon.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the scale of the icon.
    /// </summary>
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
    /// Gets or sets the reseting behaviour of the control.
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
    /// Gets or sets the items for control.
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
    /// Gets or sets the number of items (icons).
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
        if (obj is StswRatingControl stsw)
        {
            var val = (int)e.NewValue;
            if (val < 0)
                val = 0;

            if (val > stsw.Items.Count)
                while (stsw.Items.Count < val)
                {
                    var index = stsw.Direction.In(ExpandDirection.Left, ExpandDirection.Up) ? 0 : stsw.Items.Count;
                    stsw.Items.Insert(index, new() { Value = stsw.Items.Count + 1 });
                }
            else if (val < stsw.Items.Count)
                while (stsw.Items.Count > val)
                {
                    var index = stsw.Direction.In(ExpandDirection.Left, ExpandDirection.Up) ? 0 : stsw.Items.Count - 1;
                    stsw.Items.RemoveAt(index);
                }
            
            stsw.Value = stsw.Items.Count(x => x.IsChecked);
        }
    }

    /// <summary>
    /// Gets or sets the visibility of the number of items.
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
    /// Gets or sets the numeric value of the control.
    /// </summary>
    public int? Placeholder
    {
        get => (int?)GetValue(PlaceholderProperty);
        internal set => SetValue(PlaceholderProperty, value);
    }
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(int?),
            typeof(StswRatingControl),
            new FrameworkPropertyMetadata(default(int?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnPlaceholderChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnPlaceholderChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswRatingControl stsw)
        {
            stsw.Items.ToList().ForEach(x => x.IsMouseOver = false);

            var val = stsw.Placeholder;
            if (val is < 0 or null || val > stsw.Items.Count)
                return;

            var startIndex = stsw.Direction switch
            {
                ExpandDirection.Down => 0,
                ExpandDirection.Left => Math.Max(0, stsw.Items.Count - val.Value),
                ExpandDirection.Right => 0,
                ExpandDirection.Up => Math.Max(0, stsw.Items.Count - val.Value),
                _ => 0
            };
            var endIndex = stsw.Direction switch
            {
                ExpandDirection.Down => val,
                ExpandDirection.Left => stsw.Items.Count,
                ExpandDirection.Right => val,
                ExpandDirection.Up => stsw.Items.Count,
                _ => 0
            };
            for (var i = startIndex; i < endIndex; i++)
                stsw.Items[i].IsMouseOver = true;
        }
    }

    /// <summary>
    /// Gets or sets the numeric value of the control.
    /// </summary>
    public int? Value
    {
        get => (int?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    public static readonly DependencyProperty ValueProperty
        = DependencyProperty.Register(
            nameof(Value),
            typeof(int?),
            typeof(StswRatingControl),
            new FrameworkPropertyMetadata(default(int?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswRatingControl stsw)
        {
            stsw.Items.ToList().ForEach(x => x.IsChecked = false);

            var val = stsw.Value;
            if (val is < 0 or null || val > stsw.Items.Count)
                return;

            var startIndex = stsw.Direction switch
            {
                ExpandDirection.Down => 0,
                ExpandDirection.Left => Math.Max(0, stsw.Items.Count - val.Value),
                ExpandDirection.Right => 0,
                ExpandDirection.Up => Math.Max(0, stsw.Items.Count - val.Value),
                _ => 0
            };
            var endIndex = stsw.Direction switch
            {
                ExpandDirection.Down => val,
                ExpandDirection.Left => stsw.Items.Count,
                ExpandDirection.Right => val,
                ExpandDirection.Up => stsw.Items.Count,
                _ => 0
            };
            for (var i = startIndex; i < endIndex; i++)
                stsw.Items[i].IsChecked = true;
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the fill brush of the icon.
    /// </summary>
    public Brush IconFill
    {
        get => (Brush)GetValue(IconFillProperty);
        set => SetValue(IconFillProperty, value);
    }
    public static readonly DependencyProperty IconFillProperty
        = DependencyProperty.Register(
            nameof(IconFill),
            typeof(Brush),
            typeof(StswRatingControl)
        );

    /// <summary>
    /// Gets or sets the stroke brush of the icon.
    /// </summary>
    public Brush IconStroke
    {
        get => (Brush)GetValue(IconStrokeProperty);
        set => SetValue(IconStrokeProperty, value);
    }
    public static readonly DependencyProperty IconStrokeProperty
        = DependencyProperty.Register(
            nameof(IconStroke),
            typeof(Brush),
            typeof(StswRatingControl)
        );

    /// <summary>
    /// Gets or sets the stroke thickness of the icon.
    /// </summary>
    public double IconStrokeThickness
    {
        get => (double)GetValue(IconStrokeThicknessProperty);
        set => SetValue(IconStrokeThicknessProperty, value);
    }
    public static readonly DependencyProperty IconStrokeThicknessProperty
        = DependencyProperty.Register(
            nameof(IconStrokeThickness),
            typeof(double),
            typeof(StswRatingControl)
        );
    #endregion

    #region Excluded properties
    /// The following properties are hidden from the designer and serialization:

    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Brush? BorderBrush { get; private set; }

    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Thickness? BorderThickness { get; private set; }
    #endregion
}

/// <summary>
/// Data model for <see cref="StswRatingControl"/>'s items.
/// </summary>
internal class StswRatingItem : StswObservableObject
{
    /// <summary>
    /// Gets or sets a value indicating whether the item is checked.
    /// </summary>
    public bool IsChecked
    {
        get => isChecked;
        internal set => SetProperty(ref isChecked, value);
    }
    private bool isChecked;

    /// <summary>
    /// Gets or sets a value indicating whether the mouse is over the item.
    /// </summary>
    public bool IsMouseOver
    {
        get => isMouseOver;
        internal set => SetProperty(ref isMouseOver, value);
    }
    private bool isMouseOver;

    /// <summary>
    /// Gets or sets the value associated with the item.
    /// </summary>
    public int Value
    {
        get => value;
        internal set => SetProperty(ref this.value, value);
    }
    private int value;
}
