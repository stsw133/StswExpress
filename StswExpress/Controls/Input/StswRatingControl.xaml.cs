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
    /// 
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.LeftButton == MouseButtonState.Pressed)
            Value = Placeholder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var position = e.GetPosition(this);
        int x = (int)Math.Floor(position.X);
        int y = (int)Math.Floor(position.Y);

        if (Direction == ExpandDirection.Down && ActualHeight != 0)
            Placeholder = Convert.ToInt32(Math.Round(y / ActualHeight * Items.Count + 0.4));
        else if (Direction == ExpandDirection.Left && ActualWidth != 0)
            Placeholder = Convert.ToInt32(Math.Round((ActualWidth - x) / ActualWidth * Items.Count + 0.4));
        else if (Direction == ExpandDirection.Right && ActualWidth != 0)
            Placeholder = Convert.ToInt32(Math.Round(x / ActualWidth * Items.Count + 0.4));
        else if (Direction == ExpandDirection.Up && ActualHeight != 0)
            Placeholder = Convert.ToInt32(Math.Round(Items.Count - y / ActualHeight * Items.Count + 0.4));

        if (!CanReset && Placeholder == 0)
            Placeholder = 1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        Placeholder = null;
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the reseting behaviour of the control.
    /// </summary>
    public bool CanReset
    {
        get => (bool)GetValue(CanResetProperty);
        set => SetValue(CanResetProperty, value);
    }
    public static readonly DependencyProperty CanResetProperty
        = DependencyProperty.Register(
            nameof(CanReset),
            typeof(bool),
            typeof(StswRatingControl)
        );

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
            new FrameworkPropertyMetadata(default(ExpandDirection),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );

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
        //get => (int)GetValue(ItemsNumberProperty);
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
            {
                if (stsw.Direction.In(ExpandDirection.Left, ExpandDirection.Up))
                {
                    while (stsw.Items.Count < val)
                        stsw.Items.Insert(0, new());
                }
                else
                {
                    while (stsw.Items.Count < val)
                        stsw.Items.Add(new());
                }
            }
            else if (val < stsw.Items.Count)
            {
                if (stsw.Direction.In(ExpandDirection.Left, ExpandDirection.Up))
                {
                    while (stsw.Items.Count > val)
                        stsw.Items.RemoveAt(0);
                }
                else
                {
                    while (stsw.Items.Count > val)
                        stsw.Items.Remove(stsw.Items.Last());
                }
            }
            stsw.Value = stsw.Items.Count(x => x.IsChecked);
        }
    }

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
            if (val is < 0 or null)
                return;
            else if (val > stsw.Items.Count)
                val = stsw.Items.Count;

            for (var i = 0; i <= val; i++)
            {
                if (val - i < 1)
                    continue;

                var index = stsw.Direction.In(ExpandDirection.Left, ExpandDirection.Up) ? stsw.Items.Count - 1 - i : i;
                stsw.Items[index].IsMouseOver = true;
            }
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
            if (val is < 0 or null)
                return;
            else if (val > stsw.Items.Count)
                val = stsw.Items.Count;

            for (int i = 0; i <= val; i++)
            {
                if (val - i < 1)
                    continue;

                var index = stsw.Direction.In(ExpandDirection.Left, ExpandDirection.Up) ? stsw.Items.Count - 1 - i : i;
                stsw.Items[index].IsChecked = true;
            }
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

    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Brush? Foreground { get; private set; }
    #endregion
}

/// <summary>
/// Data model for <see cref="StswRatingControl"/>'s items.
/// </summary>
public class StswRatingItem : StswObservableObject
{
    /// <summary>
    /// 
    /// </summary>
    public bool IsChecked
    {
        get => isChecked;
        internal set => SetProperty(ref isChecked, value);
    }
    private bool isChecked;

    /// <summary>
    /// 
    /// </summary>
    public bool IsMouseOver
    {
        get => isMouseOver;
        internal set => SetProperty(ref isMouseOver, value);
    }
    private bool isMouseOver;
}
