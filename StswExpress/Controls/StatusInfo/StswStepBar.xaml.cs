using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
[StswInfo("0.19.0", Changes = StswPlannedChanges.Finish)]
public class StswStepBar : Control
{
    #region PARTS
    private Canvas? _canvas;

    private List<StswStepBarLine> _lines = [];

    #endregion

    public StswStepBar()
    {
        SetValue(StepsProperty, new ObservableCollection<StswStepBarItem>());
    }

    static StswStepBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswStepBar), new FrameworkPropertyMetadata(typeof(StswStepBar)));
        //ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswStepBar), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region OnApplyTemplate
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _canvas = GetTemplateChild("PART_Canvas") as Canvas;

        CreateSteps();
    }

    #endregion

    #region Login properties

    public ObservableCollection<StswStepBarItem> Steps
    {
        get { return (ObservableCollection<StswStepBarItem>)GetValue(StepsProperty); }
        set { SetValue(StepsProperty, value); }
    }
    public static readonly DependencyProperty StepsProperty =
        DependencyProperty.Register(nameof(Steps),
            typeof(ObservableCollection<StswStepBarItem>),
            typeof(StswStepBar));

    public int StepNumber
    {
        get { return (int)GetValue(StepNumberProperty); }
        set { SetValue(StepNumberProperty, value); }
    }
    public static readonly DependencyProperty StepNumberProperty =
        DependencyProperty.Register(nameof(StepNumber),
            typeof(int),
            typeof(StswStepBar),
            new FrameworkPropertyMetadata(0,
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnStepNumberChanged));

    #endregion

    #region Style properties

    public double StepsSize
    {
        get { return (double)GetValue(StepsSizeProperty); }
        set { SetValue(StepsSizeProperty, value); }
    }
    public static readonly DependencyProperty StepsSizeProperty =
        DependencyProperty.Register(nameof(StepsSize),
            typeof(double),
            typeof(StswStepBar), 
            new PropertyMetadata(40.0));

    #endregion

    #region Events & Methods
    private void CreateSteps()
    {
        var left = 50.0;

        for (int i = 0; i < Steps.Count; i++)
        {
            var step = Steps[i];

            Canvas.SetLeft(step, left);
            Panel.SetZIndex(step, 99);
            step.SetStepNumber(i+1);

            _canvas.Children.Add(step);

            if (i != 0)
            {
                Steps[i - 1].NextStep = step;

                var stop1 = new GradientStop(step.CompletedColor, 0.0);
                var stop2 = new GradientStop(step.CompletedColor, 0.0);
                var stop3 = new GradientStop(step.UncompletedColor, 0.0);
                var stop4 = new GradientStop(step.UncompletedColor, 0.0);

                var gradientStops = new GradientStopCollection([
                    stop1, stop2, stop3, stop4
                    ]);

                var line = new StswStepBarLine(step.CompletedColor, step.UncompletedColor)
                {
                    StartPoint = new Point(Canvas.GetLeft(Steps[i - 1]) + StepsSize / 2, StepsSize / 2),
                    EndPoint = new Point(Canvas.GetLeft(step) + StepsSize / 2, StepsSize / 2)
                };

                _lines.Add(line);

                _canvas.Children.Add(line);
            }

            left += 100.0;
        }
    }

    private static void OnStepNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StswStepBar stepBar)
        {
            var index = (int)e.NewValue;

            for (int i = 0; i < stepBar.Steps.Count; i++)
            {
                var step = stepBar.Steps[i];

                if (i < index)
                    step.SetStatus(StepBarItemStatus.Completed);
                else if (i == index)
                    step.SetStatus(StepBarItemStatus.NextStep);
                else
                    step.SetStatus(StepBarItemStatus.Normal);
            }

            for (int i = 0; i < stepBar._lines.Count; i++)
            {
                var line = stepBar._lines[i];

                if (i < index)
                    line.SetStatus(StepBarItemStatus.Completed);
                else if (i == index)
                    line.SetStatus(StepBarItemStatus.NextStep);
                else
                    line.SetStatus(StepBarItemStatus.Normal);
            }

        }
    }


    private async void SimulateWorking()
    {
        for (int i = 0; i < Steps.Count; i++)
        {
            var step = Steps[i];

            await Task.Delay(3000);

            step.SetStatus(StepBarItemStatus.Completed);

            if (i < Steps.Count - 1)
                Steps[i + 1].SetStatus(StepBarItemStatus.NextStep);

            if (i != 0)
                _lines[i - 1].SetStatus(StepBarItemStatus.Completed);
            //CheckLine(_lines[i - 1]);
        }

        for (int i = Steps.Count-1; i >=0; i--)
        {
            var step = Steps[i];

            await Task.Delay(3000);

            step.SetStatus(StepBarItemStatus.Normal);

            if (i > 0)
                Steps[i - 1].SetStatus(StepBarItemStatus.NextStep);

            if (i != 0)
                _lines[i - 1].SetStatus(StepBarItemStatus.Normal);
            //CheckLine(_lines[i - 1]);
        }
    }

    #endregion
}

/// <summary>
/// 
/// </summary>
[StswInfo("0.19.0", Changes = StswPlannedChanges.Finish)]
public class StswStepBarItem : Control
{
    #region PARTS
    private GradientStop? _gradientStop1;
    private GradientStop? _gradientStop2;
    #endregion

    public StswStepBarItem()
    {
    }

    static StswStepBarItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswStepBarItem), new FrameworkPropertyMetadata(typeof(StswStepBarItem)));
        //ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswStepBarItem), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region OnApplyTemplate
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_Border") is Border border &&
        border.Background is RadialGradientBrush brush &&
        brush.GradientStops.Count == 4)
        {
            // Clone the brush to make it writable
            var newBrush = brush.Clone();
            //border.Background = newBrush;

            // Now you can safely modify the GradientStops
            brush.GradientStops[0].Color = CompletedColor;
            brush.GradientStops[1].Color = CompletedColor;
            brush.GradientStops[2].Color = UncompletedColor;
            brush.GradientStops[3].Color = UncompletedColor;
        }

        VisualStateManager.GoToState(this, "Normal", false);
    }
    #endregion

    #region Logic properties

    public StepBarItemStatus Status
    {
        get { return (StepBarItemStatus)GetValue(IsCheckedProperty); }
        private set { SetValue(IsCheckedProperty, value); }
    }
    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(nameof(Status),
            typeof(StepBarItemStatus),
            typeof(StswStepBarItem),
            new FrameworkPropertyMetadata(
                StepBarItemStatus.Normal,
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnStatusChanged));

    public void SetStatus(StepBarItemStatus status) => Status = status;

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StswStepBarItem step)
        {
            //step.AnimateStatusChange((StepBarItemStatus)e.OldValue, (StepBarItemStatus)e.NewValue);
            //step.ChangeState(((StepBarItemStatus)e.NewValue).ToString());
            step.ChangeState(((StepBarItemStatus)e.NewValue).ToString());
        }
    }

    public int StepNumber
    {
        get { return (int)GetValue(StepNumberProperty); }
        private set { SetValue(StepNumberProperty, value); }
    }
    public static readonly DependencyProperty StepNumberProperty =
        DependencyProperty.Register(nameof(StepNumber),
            typeof(int),
            typeof(StswStepBarItem),
            new PropertyMetadata(0));

    public int SetStepNumber(int stepNumber) => StepNumber = stepNumber;

    public StswStepBarItem? NextStep
    {
        get { return (StswStepBarItem?)GetValue(NextStepProperty); }
        set { SetValue(NextStepProperty, value); }
    }
    public static readonly DependencyProperty NextStepProperty =
        DependencyProperty.Register(nameof(NextStep),
            typeof(StswStepBarItem),
            typeof(StswStepBarItem),
            new PropertyMetadata(null));

    #endregion

    #region Style properties
    public Color UncompletedColor
    {
        get { return (Color)GetValue(UncomplatedColorProperty); }
        set { SetValue(UncomplatedColorProperty, value); }
    }
    public static readonly DependencyProperty UncomplatedColorProperty =
        DependencyProperty.Register(nameof(UncompletedColor),
            typeof(Color),
            typeof(StswStepBarItem),
            new PropertyMetadata(Color.FromRgb(180, 180, 180)));

    public Color CompletedColor
    {
        get { return (Color)GetValue(ComplatedColorProperty); }
        set { SetValue(ComplatedColorProperty, value); }
    }
    public static readonly DependencyProperty ComplatedColorProperty =
        DependencyProperty.Register(nameof(CompletedColor),
            typeof(Color),
            typeof(StswStepBarItem),
            new PropertyMetadata(Color.FromRgb(0, 130, 235)));
    #endregion

    #region Animation

    public void ChangeState(string stateName)
    {
        VisualStateManager.GoToState(this, stateName, true);
    }

    #endregion
}

/// <summary>
/// 
/// </summary>
[StswInfo("0.19.0", Changes = StswPlannedChanges.Finish)]
public class StswStepBarLine : Control
{
    private Color _completedColor;
    private Color _uncompletedColor;

    public StswStepBarLine(Color completedColor, Color uncompletedColor)
    {
        _completedColor = completedColor;
        _uncompletedColor = uncompletedColor;
    }

    static StswStepBarLine()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswStepBarLine), new FrameworkPropertyMetadata(typeof(StswStepBarLine)));
        //ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswStepBarLine), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region OnApplyTemplate
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_Line") is Line line &&
        line.Stroke is LinearGradientBrush brush &&
        brush.GradientStops.Count == 4)
        {
            // Clone the brush to make it writable
            var newBrush = brush.Clone();
            //border.Background = newBrush;

            // Now you can safely modify the GradientStops
            brush.GradientStops[0].Color = _completedColor;
            brush.GradientStops[1].Color = _completedColor;
            brush.GradientStops[2].Color = _uncompletedColor;
            brush.GradientStops[3].Color = _uncompletedColor;
        }

        VisualStateManager.GoToState(this, "Normal", false);
    }
    #endregion

    #region Logic properties

    public Point StartPoint
    {
        get { return (Point)GetValue(StartPointProperty); }
        set { SetValue(StartPointProperty, value); }
    }
    public static readonly DependencyProperty StartPointProperty =
        DependencyProperty.Register(nameof(StartPoint),
            typeof(Point), 
            typeof(StswStepBarLine), 
            new PropertyMetadata(new Point(0,0)));

    public Point EndPoint
    {
        get { return (Point)GetValue(EndPointProperty); }
        set { SetValue(EndPointProperty, value); }
    }
    public static readonly DependencyProperty EndPointProperty =
        DependencyProperty.Register(nameof(EndPoint),
            typeof(Point),
            typeof(StswStepBarLine),
            new PropertyMetadata(new Point(0, 0)));

    public double StrokeThickness
    {
        get { return (double)GetValue(StrokeThicknessProperty); }
        set { SetValue(StrokeThicknessProperty, value); }
    }
    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(nameof(StrokeThickness),
            typeof(double), 
            typeof(StswStepBarLine), 
            new PropertyMetadata(3.0));

    public StswStepBarItem FirstStep
    {
        get { return (StswStepBarItem)GetValue(FirstStepProperty); }
        set { SetValue(FirstStepProperty, value); }
    }
    public static readonly DependencyProperty FirstStepProperty =
        DependencyProperty.Register(nameof(FirstStep),
            typeof(StswStepBarItem),
            typeof(StswStepBarLine),
            new PropertyMetadata(null));

    public StswStepBarItem SecondStep
    {
        get { return (StswStepBarItem)GetValue(SecondStepProperty); }
        set { SetValue(SecondStepProperty, value); }
    }
    public static readonly DependencyProperty SecondStepProperty =
        DependencyProperty.Register(nameof(SecondStep),
            typeof(StswStepBarItem),
            typeof(StswStepBarLine),
            new PropertyMetadata(null));

    #endregion

    internal void SetStatus(StepBarItemStatus status)
    {
        VisualStateManager.GoToState(this, status.ToString(), false);
    }
}

/// <summary>
/// 
/// </summary>
[StswInfo("0.19.0", Changes = StswPlannedChanges.Finish)]
public enum StepBarItemStatus
{
    Normal,
    NextStep,
    Completed
}
