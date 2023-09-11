using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a control that extends the <see cref="ScrollViewer"/> class with additional functionality.
/// </summary>
public class StswPager : UserControl
{
    public ICommand UpCommand { get; set; }
    public ICommand DownCommand { get; set; }
    public ICommand LeftCommand { get; set; }
    public ICommand RightCommand { get; set; }

    public StswPager()
    {
        UpCommand = new StswCommand(Up, UpCondition);
        DownCommand = new StswCommand(Down, DownCondition);
        LeftCommand = new StswCommand(Left, LeftCondition);
        RightCommand = new StswCommand(Right, RightCondition);
    }
    static StswPager()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswPager), new FrameworkPropertyMetadata(typeof(StswPager)));
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
    /// <param name="oldContent"></param>
    /// <param name="newContent"></param>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);

        /// setting DataContext of Content element
        if (Content is FrameworkElement elem)
        {
            elem.SetBinding(DataContextProperty, new Binding()
            {
                Mode = BindingMode.OneWay,
                Path = new PropertyPath(nameof(SelectedItem)),
                Source = this
            });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Down)
            Down();
        else if (e.Key == Key.Left)
            Left();
        else if (e.Key == Key.Right)
            Right();
        else if (e.Key == Key.Up)
            Up();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        Keyboard.Focus(this);
    }
    /*
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (Orientation == Orientation.Horizontal)
        {
            if (e.Delta > 0)
                Left();
            else
                Right();
        }
        else
        {
            if (e.Delta > 0)
                Up();
            else
                Down();
        }

        CommandManager.InvalidateRequerySuggested();
    }
    */
    /// <summary>
    /// 
    /// </summary>
    private void Down()
    {
        if (DownCondition())
            SelectedIndex++;
    }
    private bool DownCondition() => SelectedIndex < (ItemsSource?.Count - 1);

    /// <summary>
    /// 
    /// </summary>
    private void Left()
    {
        if (LeftCondition())
            SelectedIndex--;
    }
    private bool LeftCondition() => SelectedIndex > 0;

    /// <summary>
    /// 
    /// </summary>
    private void Right()
    {
        if (RightCondition())
            SelectedIndex++;
    }
    private bool RightCondition() => SelectedIndex < (ItemsSource?.Count - 1);

    /// <summary>
    /// 
    /// </summary>
    private void Up()
    {
        if (UpCondition())
            SelectedIndex--;
    }
    private bool UpCondition() => SelectedIndex > 0;
    #endregion

    #region Main properties
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
            typeof(StswPager),
            new FrameworkPropertyMetadata(default(IList),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnItemsSourceChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswPager stsw)
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
            typeof(StswPager)
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
            typeof(StswPager),
            new FrameworkPropertyMetadata(-1,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedIndexChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnSelectedIndexChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswPager stsw)
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
            typeof(StswPager),
            new FrameworkPropertyMetadata(default(object?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnSelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswPager stsw)
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
            typeof(StswPager)
        );
    #endregion
}
