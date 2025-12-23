using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress.Wpf;

/// <summary>
/// Represents an individual step item within a step bar control.
/// </summary>
[StswPlannedChanges(StswPlannedChanges.Finish)]
public class StswStepBarItem : Control
{
    static StswStepBarItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswStepBarItem), new FrameworkPropertyMetadata(typeof(StswStepBarItem)));
    }

    #region Dependency properties
    /// <summary>
    /// Gets or sets the color used for completed steps.
    /// </summary>
    public Color CompletedColor
    {
        get => (Color)GetValue(CompletedColorProperty);
        set => SetValue(CompletedColorProperty, value);
    }
    public static readonly DependencyProperty CompletedColorProperty
        = DependencyProperty.Register(
            nameof(CompletedColor),
            typeof(Color),
            typeof(StswStepBarItem),
            new PropertyMetadata(Color.FromRgb(0, 130, 235))
        );

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
    public StswStepBarItemStatus Status
    {
        get => (StswStepBarItemStatus)GetValue(IsCheckedProperty);
        private set => SetValue(IsCheckedProperty, value);
    }
    public static readonly DependencyProperty IsCheckedProperty
        = DependencyProperty.Register(
            nameof(Status),
            typeof(StswStepBarItemStatus),
            typeof(StswStepBarItem),
            new FrameworkPropertyMetadata(default(StswStepBarItemStatus), FrameworkPropertyMetadataOptions.AffectsRender, OnStatusChanged)
        );
    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswStepBarItem)d;
        //stsw.AnimateStatusChange((StepBarItemStatus)e.OldValue, (StepBarItemStatus)e.NewValue);
        //stsw.ChangeState(((StepBarItemStatus)e.NewValue).ToString());
        stsw.ChangeState(((StswStepBarItemStatus)e.NewValue).ToString());
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

    /// <summary>
    /// Gets or sets the color used for uncompleted steps.
    /// </summary>
    public Color UncompletedColor
    {
        get => (Color)GetValue(UncompletedColorProperty);
        set => SetValue(UncompletedColorProperty, value);
    }
    public static readonly DependencyProperty UncompletedColorProperty
        = DependencyProperty.Register(
            nameof(UncompletedColor),
            typeof(Color),
            typeof(StswStepBarItem),
            new PropertyMetadata(Color.FromRgb(180, 180, 180))
        );
    #endregion

    #region Template
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
    #endregion

    #region Helpers
    /// <summary>
    /// Sets the status of the step bar item.
    /// </summary>
    /// <param name="status">The new status to assign to the step bar item.</param>
    public void SetStatus(StswStepBarItemStatus status) => Status = status;

    /// <summary>
    /// Sets the step number for the step bar item.
    /// </summary>
    /// <param name="stepNumber">The step number to assign.</param>
    /// <returns>The assigned step number.</returns>
    public int SetStepNumber(int stepNumber) => StepNumber = stepNumber;
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
