using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace StswExpress;

/// <summary>
/// Represents a control that displays a sequence of steps to indicate progress through a process.
/// </summary>
[StswPlannedChanges(StswPlannedChanges.Finish)]
public class StswStepBar : Control
{
    private Canvas? _canvas;
    private List<StswStepBarLine> _lines = [];

    public StswStepBar()
    {
        SetValue(StepsProperty, new ObservableCollection<StswStepBarItem>());
    }
    static StswStepBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswStepBar), new FrameworkPropertyMetadata(typeof(StswStepBar)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _canvas = GetTemplateChild("PART_Canvas") as Canvas;
        CreateSteps();
    }

    /// <summary>
    /// Creates and positions the steps and connecting lines on the canvas.
    /// </summary>
    private void CreateSteps()
    {
        if (_canvas == null || Steps.Count == 0)
            return;

        var left = 50.0;

        for (var i = 0; i < Steps.Count; i++)
        {
            var step = Steps[i];

            Canvas.SetLeft(step, left);
            Panel.SetZIndex(step, 99);
            step.SetStepNumber(i + 1);

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

    /// <summary>
    /// Handles changes to the StepNumber property.
    /// </summary>
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

        for (int i = Steps.Count - 1; i >= 0; i--)
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

    #region Login properties
    /// <summary>
    /// Gets or sets the collection of steps to be displayed in the step bar.
    /// </summary>
    public ObservableCollection<StswStepBarItem> Steps
    {
        get => (ObservableCollection<StswStepBarItem>)GetValue(StepsProperty);
        set => SetValue(StepsProperty, value);
    }
    public static readonly DependencyProperty StepsProperty
        = DependencyProperty.Register(
            nameof(Steps),
            typeof(ObservableCollection<StswStepBarItem>),
            typeof(StswStepBar)
        );

    /// <summary>
    /// Gets or sets the current step number (0-based index).
    /// </summary>
    public int StepNumber
    {
        get => (int)GetValue(StepNumberProperty);
        set => SetValue(StepNumberProperty, value);
    }
    public static readonly DependencyProperty StepNumberProperty
        = DependencyProperty.Register(
            nameof(StepNumber),
            typeof(int),
            typeof(StswStepBar),
            new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.AffectsRender, OnStepNumberChanged)
        );
    private static void OnStepNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswStepBar stsw)
            return;

        var index = (int)e.NewValue;

        for (var i = 0; i < stsw.Steps.Count; i++)
        {
            var step = stsw.Steps[i];

            if (i < index)
                step.SetStatus(StepBarItemStatus.Completed);
            else if (i == index)
                step.SetStatus(StepBarItemStatus.NextStep);
            else
                step.SetStatus(StepBarItemStatus.Normal);
        }

        for (var i = 0; i < stsw._lines.Count; i++)
        {
            var line = stsw._lines[i];

            if (i < index)
                line.SetStatus(StepBarItemStatus.Completed);
            else if (i == index)
                line.SetStatus(StepBarItemStatus.NextStep);
            else
                line.SetStatus(StepBarItemStatus.Normal);
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the size of each step in the step bar.
    /// </summary>
    public double StepsSize
    {
        get => (double)GetValue(StepsSizeProperty);
        set => SetValue(StepsSizeProperty, value);
    }
    public static readonly DependencyProperty StepsSizeProperty
        = DependencyProperty.Register(
            nameof(StepsSize),
            typeof(double),
            typeof(StswStepBar), 
            new PropertyMetadata(40.0)
        );
    #endregion
}

/// <summary>
/// Represents an individual step item within a step bar control.
/// </summary>
[StswPlannedChanges(StswPlannedChanges.Finish)]
public class StswStepBarItem : Control
{
    private GradientStop? _gradientStop1;
    private GradientStop? _gradientStop2;

    static StswStepBarItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswStepBarItem), new FrameworkPropertyMetadata(typeof(StswStepBarItem)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_Border") is Border border
         && border.Background is RadialGradientBrush brush
         && brush.GradientStops.Count == 4)
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

    /// <summary>
    /// Sets the status of the step bar item.
    /// </summary>
    /// <param name="status">The new status to assign to the step bar item.</param>
    public void SetStatus(StepBarItemStatus status) => Status = status;

    /// <summary>
    /// Sets the step number for the step bar item.
    /// </summary>
    /// <param name="stepNumber">The step number to assign.</param>
    /// <returns>The assigned step number.</returns>
    public int SetStepNumber(int stepNumber) => StepNumber = stepNumber;
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the next step item in the sequence.
    /// </summary>
    public StswStepBarItem? NextStep
    {
        get => (StswStepBarItem?)GetValue(NextStepProperty);
        set => SetValue(NextStepProperty, value);
    }
    public static readonly DependencyProperty NextStepProperty
        = DependencyProperty.Register(
            nameof(NextStep),
            typeof(StswStepBarItem),
            typeof(StswStepBarItem)
        );

    /// <summary>
    /// Gets the current status of the step item (Normal, NextStep, Completed).
    /// </summary>
    public StepBarItemStatus Status
    {
        get => (StepBarItemStatus)GetValue(IsCheckedProperty);
        private set => SetValue(IsCheckedProperty, value);
    }
    public static readonly DependencyProperty IsCheckedProperty
        = DependencyProperty.Register(
            nameof(Status),
            typeof(StepBarItemStatus),
            typeof(StswStepBarItem),
            new FrameworkPropertyMetadata(default(StepBarItemStatus), FrameworkPropertyMetadataOptions.AffectsRender, OnStatusChanged)
        );
    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswStepBarItem stsw)
            return;

        //stsw.AnimateStatusChange((StepBarItemStatus)e.OldValue, (StepBarItemStatus)e.NewValue);
        //stsw.ChangeState(((StepBarItemStatus)e.NewValue).ToString());
        stsw.ChangeState(((StepBarItemStatus)e.NewValue).ToString());
    }

    /// <summary>
    /// Gets the current step number in the process.
    /// </summary>
    public int StepNumber
    {
        get => (int)GetValue(StepNumberProperty);
        private set => SetValue(StepNumberProperty, value);
    }
    public static readonly DependencyProperty StepNumberProperty
        = DependencyProperty.Register(
            nameof(StepNumber),
            typeof(int),
            typeof(StswStepBarItem)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the color used for completed steps.
    /// </summary>
    public Color CompletedColor
    {
        get => (Color)GetValue(ComplatedColorProperty);
        set => SetValue(ComplatedColorProperty, value);
    }
    public static readonly DependencyProperty ComplatedColorProperty
        = DependencyProperty.Register(
            nameof(CompletedColor),
            typeof(Color),
            typeof(StswStepBarItem),
            new PropertyMetadata(Color.FromRgb(0, 130, 235))
        );

    /// <summary>
    /// Gets or sets the color used for uncompleted steps.
    /// </summary>
    public Color UncompletedColor
    {
        get => (Color)GetValue(UncomplatedColorProperty);
        set => SetValue(UncomplatedColorProperty, value);
    }
    public static readonly DependencyProperty UncomplatedColorProperty
        = DependencyProperty.Register(
            nameof(UncompletedColor),
            typeof(Color),
            typeof(StswStepBarItem),
            new PropertyMetadata(Color.FromRgb(180, 180, 180))
        );
    #endregion

    #region Animations
    /// <summary>
    /// Changes the visual state of the control to the specified state name.
    /// </summary>
    /// <param name="stateName">The name of the visual state to transition to.</param>
    public void ChangeState(string stateName)
    {
        VisualStateManager.GoToState(this, stateName, true);
    }
    #endregion
}

/// <summary>
/// Represents a line connecting two step bar items, visually indicating progress between them.
/// </summary>
[StswPlannedChanges(StswPlannedChanges.Finish)]
public class StswStepBarLine(Color completedColor, Color uncompletedColor) : Control
{
    static StswStepBarLine()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswStepBarLine), new FrameworkPropertyMetadata(typeof(StswStepBarLine)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_Line") is Line line
         && line.Stroke is LinearGradientBrush brush
         && brush.GradientStops.Count == 4)
        {
            // Clone the brush to make it writable
            var newBrush = brush.Clone();
            //border.Background = newBrush;

            // Now you can safely modify the GradientStops
            brush.GradientStops[0].Color = completedColor;
            brush.GradientStops[1].Color = completedColor;
            brush.GradientStops[2].Color = uncompletedColor;
            brush.GradientStops[3].Color = uncompletedColor;
        }

        VisualStateManager.GoToState(this, "Normal", false);
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the starting point of the line.
    /// </summary>
    public Point StartPoint
    {
        get => (Point)GetValue(StartPointProperty);
        set => SetValue(StartPointProperty, value);
    }
    public static readonly DependencyProperty StartPointProperty
        = DependencyProperty.Register(
            nameof(StartPoint),
            typeof(Point), 
            typeof(StswStepBarLine), 
            new PropertyMetadata(new Point(0, 0))
        );

    /// <summary>
    /// Gets or sets the ending point of the line.
    /// </summary>
    public Point EndPoint
    {
        get => (Point)GetValue(EndPointProperty);
        set => SetValue(EndPointProperty, value);
    }
    public static readonly DependencyProperty EndPointProperty
        = DependencyProperty.Register(
            nameof(EndPoint),
            typeof(Point),
            typeof(StswStepBarLine),
            new PropertyMetadata(new Point(0, 0))
        );

    /// <summary>
    /// Gets or sets the first step item that this line connects from.
    /// </summary>
    public StswStepBarItem? FirstStep
    {
        get => (StswStepBarItem?)GetValue(FirstStepProperty);
        set => SetValue(FirstStepProperty, value);
    }
    public static readonly DependencyProperty FirstStepProperty
        = DependencyProperty.Register(
            nameof(FirstStep),
            typeof(StswStepBarItem),
            typeof(StswStepBarLine)
        );

    /// <summary>
    /// Gets or sets the second step item that this line connects to.
    /// </summary>
    public StswStepBarItem? SecondStep
    {
        get => (StswStepBarItem?)GetValue(SecondStepProperty);
        set => SetValue(SecondStepProperty, value);
    }
    public static readonly DependencyProperty SecondStepProperty
        = DependencyProperty.Register(
            nameof(SecondStep),
            typeof(StswStepBarItem),
            typeof(StswStepBarLine)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the thickness of the line stroke.
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
            typeof(StswStepBarLine),
            new PropertyMetadata(3.0d)
        );
    #endregion

    #region Animations
    /// <summary>
    /// Sets the visual status of the line (Normal, NextStep, Completed).
    /// </summary>
    /// <param name="status">The new status to assign to the line.</param>
    internal void SetStatus(StepBarItemStatus status)
    {
        VisualStateManager.GoToState(this, status.ToString(), false);
    }
    #endregion
}

/// <summary>
/// Defines the status of a step bar item.
/// </summary>
public enum StepBarItemStatus
{
    Normal,
    NextStep,
    Completed
}
