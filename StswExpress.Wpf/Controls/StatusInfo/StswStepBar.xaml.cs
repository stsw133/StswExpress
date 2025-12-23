using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress.Wpf;

/// <summary>
/// Represents a control that displays a sequence of steps to indicate progress through a process.
/// </summary>
[StswPlannedChanges(StswPlannedChanges.Finish)]
public class StswStepBar : Control
{
    static StswStepBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswStepBar), new FrameworkPropertyMetadata(typeof(StswStepBar)));
    }
    public StswStepBar()
    {
        SetValue(StepsProperty, new ObservableCollection<StswStepBarItem>());
    }

    #region Dependency properties
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
            typeof(StswStepBar),
            new PropertyMetadata(default(ObservableCollection<StswStepBarItem>), OnStepsPropertyChanged)
        );
    private static void OnStepsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswStepBar)d;

        if (e.OldValue is ObservableCollection<StswStepBarItem> oldCollection)
            oldCollection.CollectionChanged -= stsw.OnStepsChanged;

        if (e.NewValue is ObservableCollection<StswStepBarItem> newCollection)
            newCollection.CollectionChanged += stsw.OnStepsChanged;

        stsw.CreateSteps();
    }

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
        var stsw = (StswStepBar)d;

        var index = (int)e.NewValue;

        for (var i = 0; i < stsw.Steps.Count; i++)
        {
            var step = stsw.Steps[i];

            if (i < index)
                step.SetStatus(StswStepBarItemStatus.Completed);
            else if (i == index)
                step.SetStatus(StswStepBarItemStatus.NextStep);
            else
                step.SetStatus(StswStepBarItemStatus.Normal);
        }

        for (var i = 0; i < stsw._lines.Count; i++)
        {
            var line = stsw._lines[i];

            if (i < index)
                line.SetStatus(StswStepBarItemStatus.Completed);
            else if (i == index)
                line.SetStatus(StswStepBarItemStatus.NextStep);
            else
                line.SetStatus(StswStepBarItemStatus.Normal);
        }
    }

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
            new PropertyMetadata(40.0, OnStepsSizeChanged)
        );

    private static void OnStepsSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswStepBar)d;
        stsw.CreateSteps();
    }
    #endregion

    #region Template
    private Canvas? _canvas;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _canvas = GetTemplateChild("PART_Canvas") as Canvas;

        SizeChanged -= OnSizeChanged;
        SizeChanged += OnSizeChanged;

        Steps.CollectionChanged -= OnStepsChanged;
        Steps.CollectionChanged += OnStepsChanged;

        CreateSteps();
    }
    #endregion

    #region Overrides
    /// <inheritdoc/>
    protected override Size MeasureOverride(Size constraint)
    {
        var stepSize = StepsSize;
        var desiredWidth = Steps.Count > 0
            ? stepSize + DefaultSpacing * (Steps.Count - 1)
            : stepSize;

        if (!double.IsInfinity(constraint.Width) && constraint.Width > 0)
            desiredWidth = Math.Min(desiredWidth, constraint.Width);

        var desiredHeight = stepSize;
        if (!double.IsInfinity(constraint.Height) && constraint.Height > 0)
            desiredHeight = Math.Min(desiredHeight, constraint.Height);

        return new Size(Math.Max(stepSize, desiredWidth), desiredHeight);
    }
    #endregion

    #region Logic
    private List<StswStepBarLine> _lines = [];
    private const double DefaultSpacing = 100.0;

    /// <summary>
    /// Creates and positions the steps and connecting lines on the canvas.
    /// </summary>
    private void CreateSteps()
    {
        if (_canvas == null)
            return;

        _canvas.Children.Clear();
        _lines.Clear();

        if (Steps.Count == 0)
            return;

        var stepSize = CalculateStepSize();
        var spacing = CalculateSpacing(stepSize);
        var contentWidth = stepSize + spacing * (Steps.Count - 1);

        _canvas.Width = contentWidth;
        _canvas.Height = stepSize;

        for (var i = 0; i < Steps.Count; i++)
        {
            var step = Steps[i];
            var left = spacing * i;

            step.Width = stepSize;
            step.Height = stepSize;

            Canvas.SetLeft(step, left);
            Canvas.SetTop(step, 0);
            Panel.SetZIndex(step, 99);
            step.SetStepNumber(i + 1);

            _canvas.Children.Add(step);

            if (i == 0)
                continue;

            Steps[i - 1].NextStep = step;

            var line = new StswStepBarLine(step.CompletedColor, step.UncompletedColor)
            {
                StartPoint = new Point(spacing * (i - 1) + stepSize / 2, stepSize / 2),
                EndPoint = new Point(left + stepSize / 2, stepSize / 2)
            };

            _lines.Add(line);
            _canvas.Children.Add(line);
        }
    }

    /// <summary>
    /// Calculates the spacing between steps based on the available width.
    /// </summary>
    /// <param name="stepSize">The size of each step.</param>
    /// <returns>The calculated spacing between steps.</returns>
    private double CalculateSpacing(double stepSize)
    {
        if (Steps.Count <= 1)
            return stepSize;

        if (double.IsNaN(ActualWidth) || ActualWidth <= 0)
            return DefaultSpacing;

        return Math.Max((ActualWidth - stepSize) / (Steps.Count - 1), 0);
    }

    /// <summary>
    /// Calculates the appropriate step size based on the available width.
    /// </summary>
    /// <returns>The calculated step size.</returns>
    private double CalculateStepSize()
    {
        if (double.IsNaN(ActualWidth) || ActualWidth <= 0)
            return StepsSize;

        return Math.Min(StepsSize, ActualWidth / Math.Max(1, Steps.Count));
    }

    /// <summary>
    /// Handles size changes of the control to recreate steps accordingly.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments containing size change information.</param>
    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => CreateSteps();

    /// <summary>
    /// Handles changes to the Steps collection to recreate steps accordingly.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments containing collection change information.</param>
    private void OnStepsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => CreateSteps();
    #endregion
}
