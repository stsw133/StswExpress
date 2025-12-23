using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace StswExpress.Wpf;

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

    #region Dependency properties
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

    #region Template
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

    #region Animations
    /// <summary>
    /// Sets the visual status of the line (Normal, NextStep, Completed).
    /// </summary>
    /// <param name="status">The new status to assign to the line.</param>
    internal void SetStatus(StswStepBarItemStatus status)
    {
        VisualStateManager.GoToState(this, status.ToString(), false);
    }
    #endregion
}
