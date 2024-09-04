using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// Represents a control that ...
/// </summary>
public class StswGallery : Control
{
    public ICommand PreviousCommand { get; set; }
    public ICommand NextCommand { get; set; }

    public StswGallery()
    {
        PreviousCommand = new StswCommand(Previous, PreviousCondition);
        NextCommand = new StswCommand(Next, NextCondition);
    }
    static StswGallery()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswGallery), new FrameworkPropertyMetadata(typeof(StswGallery)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        OnItemsSourceChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Left)
            Previous();
        else if (e.Key == Key.Right)
            Next();

        e.Handled = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        Keyboard.Focus(this);
    }
    /*
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (Orientation == Orientation.Horizontal)
        {
            if (e.Delta > 0)
                Left();
            else //if (e.Delta < 0)
                Right();
        }
        else
        {
            if (e.Delta > 0)
                Up();
            else //if (e.Delta < 0)
                Down();
        }

        CommandManager.InvalidateRequerySuggested();
    }
    */
    /// <summary>
    /// 
    /// </summary>
    private void Previous()
    {
        if (PreviousCondition())
            SelectedIndex--;
    }
    private bool PreviousCondition() => SelectedIndex > 0;

    /// <summary>
    /// 
    /// </summary>
    private void Next()
    {
        if (NextCondition())
            SelectedIndex++;
    }
    private bool NextCondition() => SelectedIndex < (ItemsSource?.Count - 1);
    #endregion

    #region Logic properties
    /// <summary>
    /// 
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
            typeof(StswGallery),
            new FrameworkPropertyMetadata(default(IList),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnItemsSourceChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswGallery stsw)
        {
            if (stsw.ItemsSource?.Count > 0)
            {
                stsw.SelectedIndex = 0;
                stsw.SelectedItem = stsw.ItemsSource[stsw.SelectedIndex];
            }
            else
            {
                stsw.SelectedIndex = -1;
                stsw.SelectedItem = null;
            }
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
            typeof(StswGallery),
            new FrameworkPropertyMetadata(default(Orientation), FrameworkPropertyMetadataOptions.AffectsArrange)
        );

    /// <summary>
    /// 
    /// </summary>
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }
    public static readonly DependencyProperty SelectedIndexProperty
        = DependencyProperty.Register(
            nameof(SelectedIndex),
            typeof(int),
            typeof(StswGallery),
            new FrameworkPropertyMetadata(-1,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedIndexChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnSelectedIndexChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswGallery stsw)
        {
            if (stsw.SelectedIndex >= 0)
                stsw.SelectedItem = stsw.ItemsSource[stsw.SelectedIndex];
            else
                stsw.SelectedItem = null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public object? SelectedItem
    {
        get => (object?)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }
    public static readonly DependencyProperty SelectedItemProperty
        = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(StswGallery),
            new FrameworkPropertyMetadata(default(object?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnSelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswGallery stsw)
        {
            if (stsw.SelectedItem != null)
                stsw.SelectedIndex = stsw.ItemsSource.IndexOf(stsw.SelectedItem);
            else
                stsw.SelectedIndex = -1;
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
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
            typeof(StswGallery),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
