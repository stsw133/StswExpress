using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
public class StswRatingControl : UserControl
{
    public StswRatingControl()
    {
        SetValue(ItemsProperty, new ObservableCollection<StswRatingItem>());
    }
    static StswRatingControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswRatingControl), new FrameworkPropertyMetadata(typeof(StswRatingControl)));
    }

    #region Events and methods
    /// <summary>
    /// 
    /// </summary>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.LeftButton == MouseButtonState.Pressed)
            Value = Placeholder;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var position = e.GetPosition(this);
        int x = (int)Math.Floor(position.X);
        int y = (int)Math.Floor(position.Y);

        if (Orientation == Orientation.Horizontal && ActualWidth != 0)
            Placeholder = Convert.ToInt32(Math.Round(x / ActualWidth * Items.Count + 0.4));
        else if (Orientation == Orientation.Vertical && ActualHeight != 0)
            Placeholder = Convert.ToInt32(Math.Round(y / ActualHeight * Items.Count + 0.4));
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        Placeholder = null;
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the geometry data of the icon.
    /// </summary>
    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }
    public static readonly DependencyProperty DataProperty
        = DependencyProperty.Register(
            nameof(Data),
            typeof(Geometry),
            typeof(StswRatingControl)
        );

    /// <summary>
    /// Gets or sets the scale of the icon.
    /// </summary>
    public ObservableCollection<StswRatingItem> Items
    {
        get => (ObservableCollection<StswRatingItem>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }
    public static readonly DependencyProperty ItemsProperty
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
                while (stsw.Items.Count < val)
                    stsw.Items.Add(new());
            }
            else if (val < stsw.Items.Count)
            {
                while (stsw.Items.Count > val)
                    stsw.Items.Remove(stsw.Items.Last());
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

            for (int i = 0; i <= val; i++)
                if (val - i >= 1)
                    stsw.Items[i].IsMouseOver = true;
        }
    }

    /// <summary>
    /// Gets or sets the orientation of the control.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StswRatingControl)
        );

    /// <summary>
    /// Gets or sets the scale of the icon.
    /// </summary>
    public GridLength Scale
    {
        get => (GridLength)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
    public static readonly DependencyProperty ScaleProperty
        = DependencyProperty.Register(
            nameof(Scale),
            typeof(GridLength),
            typeof(StswRatingControl)
        );

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
                if (val - i >= 1)
                    stsw.Items[i].IsChecked = true;
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the fill brush of the icon.
    /// </summary>
    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }
    public static readonly DependencyProperty FillProperty
        = DependencyProperty.Register(
            nameof(Fill),
            typeof(Brush),
            typeof(StswRatingControl)
        );

    /// <summary>
    /// Gets or sets the stroke brush of the icon.
    /// </summary>
    public Brush Stroke
    {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }
    public static readonly DependencyProperty StrokeProperty
        = DependencyProperty.Register(
            nameof(Stroke),
            typeof(Brush),
            typeof(StswRatingControl)
        );

    /// <summary>
    /// Gets or sets the stroke thickness of the icon.
    /// </summary>
    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }
    public static readonly DependencyProperty StrokeThicknessProperty
        = DependencyProperty.Register(
            nameof(StrokeThickness),
            typeof(double),
            typeof(StswRatingControl)
        );

    /// The following properties are hidden from the designer and serialization:

    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Brush? Background { get; private set; }

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
