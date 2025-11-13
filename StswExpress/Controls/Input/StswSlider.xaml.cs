using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace StswExpress;
/// <summary>
/// A slider control that allows users to select a numeric value within a range.
/// Supports custom thumb size, track size, and optional icon inside the thumb.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswSlider Minimum="1" Maximum="10" Value="{Binding UserPreference}" ThumbIcon="{StaticResource CustomIcon}" ThumbSize="20" TrackSize="4"/&gt;
/// </code>
/// </example>
public class StswSlider : Slider
{
    private Thumb? _rangeStartThumb, _rangeEndThumb, _valueThumb;
    private Track? _track;
    private Canvas? _canvas;
    private Rectangle? _selectionRangeElement;
    private bool? _storedSelectionRangeEnabled;

    static StswSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSlider), new FrameworkPropertyMetadata(typeof(StswSlider)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_track != null)
            _track.SizeChanged -= Track_SizeChanged;
        if (_rangeStartThumb != null)
            _rangeStartThumb.DragDelta -= RangeStartThumb_DragDelta;
        if (_rangeEndThumb != null)
            _rangeEndThumb.DragDelta -= RangeEndThumb_DragDelta;

        _track = GetTemplateChild("PART_Track") as Track;
        _valueThumb = GetTemplateChild("OPT_Thumb") as Thumb;
        _rangeStartThumb = GetTemplateChild("PART_StartThumb") as Thumb;
        _rangeEndThumb = GetTemplateChild("PART_EndThumb") as Thumb;
        _canvas = GetTemplateChild("OPT_Canvas") as Canvas;
        _selectionRangeElement = GetTemplateChild("PART_SelectionRange") as Rectangle;

        if (_track != null)
            _track.SizeChanged += Track_SizeChanged;
        if (_rangeStartThumb != null)
            _rangeStartThumb.DragDelta += RangeStartThumb_DragDelta;
        if (_rangeEndThumb != null)
            _rangeEndThumb.DragDelta += RangeEndThumb_DragDelta;

        UpdateMode(SliderMode);
        UpdateRangeThumbs();
    }

    /// <inheritdoc/>
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (SliderMode == StswSliderMode.Range)
        {
            var smallStep = GetStep(isLargeChange: false);
            var largeStep = GetStep(isLargeChange: true);

            switch (e.Key)
            {
                case Key.Left:
                case Key.Down:
                    MoveRange(-smallStep);
                    e.Handled = true;
                    return;

                case Key.Right:
                case Key.Up:
                    MoveRange(smallStep);
                    e.Handled = true;
                    return;

                case Key.PageDown:
                    MoveRange(-largeStep);
                    e.Handled = true;
                    return;

                case Key.PageUp:
                    MoveRange(largeStep);
                    e.Handled = true;
                    return;

                case Key.Home:
                    {
                        var width = SelectionEnd - SelectionStart;
                        if (width < 0)
                            width = 0;

                        var newStart = Minimum;
                        var newEnd = Clamp(Minimum + width, Minimum, Maximum);

                        SetCurrentValue(SelectionStartProperty, newStart);
                        SetCurrentValue(SelectionEndProperty, newEnd);
                        e.Handled = true;
                        return;
                    }

                case Key.End:
                    {
                        var width = SelectionEnd - SelectionStart;
                        if (width < 0)
                            width = 0;

                        var newEnd = Maximum;
                        var newStart = Clamp(Maximum - width, Minimum, Maximum);

                        SetCurrentValue(SelectionStartProperty, newStart);
                        SetCurrentValue(SelectionEndProperty, newEnd);
                        e.Handled = true;
                        return;
                    }
            }
        }

        base.OnPreviewKeyDown(e);
    }

    /// <inheritdoc/>
    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject d)
        {
            base.OnPreviewMouseDown(e);
            return;
        }

        var thumb = StswFnUI.FindVisualAncestor<Thumb>(d);
        if (thumb != null && e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
        {
            if (SliderMode == StswSliderMode.Range)
                ResetToDefaultRange();
            else
                ResetToDefaultValue();

            e.Handled = true;
            return;
        }

        if (SliderMode == StswSliderMode.Range)
        {
            HandleRangeClick(e, thumb);
            if (e.Handled)
                return;
        }

        base.OnPreviewMouseDown(e);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == SliderModeProperty)
        {
            UpdateMode((StswSliderMode)e.NewValue);
            UpdateRangeThumbs();
            return;
        }

        if (SliderMode != StswSliderMode.Range)
            return;

        if (e.Property == SelectionStartProperty)
        {
            var coerced = CoerceSelectionStart((double)e.NewValue);
            if (!AreClose(coerced, (double)e.NewValue))
            {
                SetCurrentValue(SelectionStartProperty, coerced);
                return;
            }
        }
        else if (e.Property == SelectionEndProperty)
        {
            var coerced = CoerceSelectionEnd((double)e.NewValue);
            if (!AreClose(coerced, (double)e.NewValue))
            {
                SetCurrentValue(SelectionEndProperty, coerced);
                return;
            }
        }

        if (e.Property == MinimumProperty || e.Property == MaximumProperty)
            EnsureSelectionBounds();

        if (e.Property == SelectionStartProperty
         || e.Property == SelectionEndProperty
         || e.Property == MinimumProperty
         || e.Property == MaximumProperty
         || e.Property == OrientationProperty)
            UpdateRangeThumbs();
    }

    /// <summary>
    /// Resets the slider's value to its default value.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void Thumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            if (sender == _valueThumb)
            {
                ResetToDefaultValue();
                e.Handled = true;
            }
            else if (sender == _rangeStartThumb || sender == _rangeEndThumb)
            {
                ResetToDefaultRange();
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// Resets Value to the default position (middle of the range, snapped to tick).
    /// </summary>
    private void ResetToDefaultValue()
    {
        var center = Minimum + (Maximum - Minimum) / 2.0;
        center = SnapToTick(center);
        SetCurrentValue(ValueProperty, center);
    }

    /// <summary>
    /// Resets the range to the full span [Minimum, Maximum], snapped to ticks.
    /// </summary>
    private void ResetToDefaultRange()
    {
        var start = SnapToTick(Minimum);
        var end = SnapToTick(Maximum);

        SetCurrentValue(SelectionStartProperty, start);
        SetCurrentValue(SelectionEndProperty, end);
        UpdateRangeThumbs();
    }

    /// <summary>
    /// Handles mouse clicks on the slider track to update the SelectionStart and SelectionEnd properties based on the ClickMode.
    /// </summary>
    /// <param name="e">The mouse button event data.</param>
    /// <param name="thumb">The thumb that was clicked, if any.</param>
    private void HandleRangeClick(MouseButtonEventArgs e, Thumb? thumb)
    {
        if (_track == null)
            return;

        if (thumb != null)
            return;

        var pos = e.GetPosition(_track);
        var clickedValueNullable = PointToValue(pos);
        if (clickedValueNullable is not double clickedValue)
            return;

        var step = GetStep(isLargeChange: true);
        var target = SnapToTick(clickedValue);

        if (clickedValue < SelectionStart)
        {
            if (IsMoveToPointEnabled)
            {
                if (target > SelectionEnd)
                {
                    var delta = target - SelectionStart;
                    MoveRange(delta);
                }
                else
                {
                    var newStart = Clamp(target, Minimum, SelectionEnd);
                    SetCurrentValue(SelectionStartProperty, newStart);
                }
            }
            else
            {
                var newStart = SelectionStart - step;
                newStart = Math.Max(Minimum, Math.Min(newStart, SelectionEnd));
                newStart = SnapToTick(newStart);
                SetCurrentValue(SelectionStartProperty, newStart);
            }

            UpdateRangeThumbs();
            e.Handled = true;
            return;
        }

        if (clickedValue > SelectionEnd)
        {
            if (IsMoveToPointEnabled)
            {
                if (target < SelectionStart)
                {
                    var delta = target - SelectionEnd;
                    MoveRange(delta);
                }
                else
                {
                    var newEnd = Clamp(target, SelectionStart, Maximum);
                    SetCurrentValue(SelectionEndProperty, newEnd);
                }
            }
            else
            {
                var newEnd = SelectionEnd + step;
                newEnd = Math.Max(SelectionStart, Math.Min(newEnd, Maximum));
                newEnd = SnapToTick(newEnd);
                SetCurrentValue(SelectionEndProperty, newEnd);
            }

            UpdateRangeThumbs();
            e.Handled = true;
            return;
        }

        var center = (SelectionStart + SelectionEnd) / 2.0;
        var distToStart = Math.Abs(target - SelectionStart);
        var distToEnd = Math.Abs(target - SelectionEnd);
        var nearestIsStart = distToStart <= distToEnd;

        if (IsMoveToPointEnabled)
        {
            if (nearestIsStart)
            {
                var delta = target - SelectionStart;

                if (target > SelectionEnd)
                {
                    MoveRange(delta);
                }
                else
                {
                    var newStart = Clamp(target, Minimum, SelectionEnd);
                    newStart = SnapToTick(newStart);
                    SetCurrentValue(SelectionStartProperty, newStart);
                }
            }
            else
            {
                var delta = target - SelectionEnd;

                if (target < SelectionStart)
                {
                    MoveRange(delta);
                }
                else
                {
                    var newEnd = Clamp(target, SelectionStart, Maximum);
                    newEnd = SnapToTick(newEnd);
                    SetCurrentValue(SelectionEndProperty, newEnd);
                }
            }
        }
        else
        {
            var stepTowardsCenter = step;

            if (nearestIsStart)
            {
                var direction = Math.Sign(center - SelectionStart);
                if (direction == 0)
                {
                    e.Handled = true;
                    return;
                }

                var delta = direction * stepTowardsCenter;
                var newStart = SelectionStart + delta;

                if (newStart > SelectionEnd)
                {
                    MoveRange(delta);
                }
                else
                {
                    newStart = SnapToTick(newStart);
                    newStart = Math.Max(Minimum, Math.Min(newStart, SelectionEnd));
                    SetCurrentValue(SelectionStartProperty, newStart);
                }
            }
            else
            {
                var direction = Math.Sign(center - SelectionEnd);
                if (direction == 0)
                {
                    e.Handled = true;
                    return;
                }

                var delta = direction * stepTowardsCenter;
                var newEnd = SelectionEnd + delta;

                if (newEnd < SelectionStart)
                {
                    MoveRange(delta);
                }
                else
                {
                    newEnd = SnapToTick(newEnd);
                    newEnd = Math.Max(SelectionStart, Math.Min(newEnd, Maximum));
                    SetCurrentValue(SelectionEndProperty, newEnd);
                }
            }
        }

        UpdateRangeThumbs();
        e.Handled = true;
    }
    #endregion

    #region Range handling methods
    /// <summary>
    /// Handles the DragDelta event of the range start thumb to update the SelectionStart property.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void RangeStartThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_track == null)
            return;

        var deltaPixels = Orientation == Orientation.Horizontal ? e.HorizontalChange : -e.VerticalChange;
        var deltaValue = PixelsToValue(deltaPixels, _track);
        deltaValue = QuantizeDeltaToTick(deltaValue);

        var newStart = SelectionStart + deltaValue;
        if (newStart > SelectionEnd)
        {
            MoveRange(deltaValue);
        }
        else
        {
            newStart = SnapToTick(newStart);
            newStart = Math.Max(Minimum, Math.Min(newStart, SelectionEnd));
            SetCurrentValue(SelectionStartProperty, newStart);
        }

        UpdateRangeThumbs();
        e.Handled = true;
    }

    /// <summary>
    /// Handles the DragDelta event of the range end thumb to update the SelectionEnd property.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void RangeEndThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_track == null)
            return;

        var deltaPixels = Orientation == Orientation.Horizontal ? e.HorizontalChange : -e.VerticalChange;
        var deltaValue = PixelsToValue(deltaPixels, _track);
        deltaValue = QuantizeDeltaToTick(deltaValue);

        var newEnd = SelectionEnd + deltaValue;
        if (newEnd < SelectionStart)
        {
            MoveRange(deltaValue);
        }
        else
        {
            newEnd = SnapToTick(newEnd);
            newEnd = Math.Max(SelectionStart, Math.Min(newEnd, Maximum));
            SetCurrentValue(SelectionEndProperty, newEnd);
        }

        UpdateRangeThumbs();
        e.Handled = true;
    }

    /// <summary>
    /// Handles the SizeChanged event of the Track control to update the positions of the range thumbs.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void Track_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateRangeThumbs();
    }

    /// <summary>
    /// Determines whether two double values are close to each other within a small tolerance.
    /// </summary>
    /// <param name="value1">The first value to compare.</param>
    /// <param name="value2">The second value to compare.</param>
    /// <returns><see langword="true"/> if the values are close; otherwise, <see langword="false"/>.</returns>
    private static bool AreClose(double value1, double value2) => Math.Abs(value1 - value2) < 0.0001;

    /// <summary>
    /// Clamps a value between a specified minimum and maximum range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum allowable value.</param>
    /// <param name="max">The maximum allowable value.</param>
    /// <returns>The clamped value.</returns>
    private static double Clamp(double value, double min, double max) => Math.Max(min, Math.Min(value, max));

    /// <summary>
    /// Coerces the SelectionStart value to ensure it is within valid bounds.
    /// </summary>
    /// <param name="value">The value to coerce.</param>
    /// <returns>The coerced value.</returns>
    private double CoerceSelectionStart(double value)
    {
        var maxValue = Math.Min(Maximum, SelectionEnd);
        return Clamp(value, Minimum, maxValue);
    }

    /// <summary>
    /// Coerces the SelectionEnd value to ensure it is within valid bounds.
    /// </summary>
    /// <param name="value">The value to coerce.</param>
    /// <returns>The coerced value.</returns>
    private double CoerceSelectionEnd(double value)
    {
        var minValue = Math.Max(Minimum, SelectionStart);
        return Clamp(value, minValue, Maximum);
    }

    /// <summary>
    /// Ensures that the SelectionStart and SelectionEnd properties are within valid bounds.
    /// </summary>
    private void EnsureSelectionBounds()
    {
        var start = CoerceSelectionStart(SelectionStart);
        if (!AreClose(start, SelectionStart))
            SetCurrentValue(SelectionStartProperty, start);

        var end = CoerceSelectionEnd(SelectionEnd);
        if (!AreClose(end, SelectionEnd))
            SetCurrentValue(SelectionEndProperty, end);
    }

    /// <summary>
    /// Gets half of the actual height of a FrameworkElement.
    /// </summary>
    /// <param name="element">The FrameworkElement to get the half height of.</param>
    /// <returns>The half height of the element.</returns>
    private static double GetHalfHeight(FrameworkElement element)
    {
        var height = element.ActualHeight;
        if (double.IsNaN(height) || height <= 0)
            height = element.Height;

        return height / 2;
    }

    /// <summary>
    /// Gets half of the actual width of a FrameworkElement.
    /// </summary>
    /// <param name="element">The FrameworkElement to get the half width of.</param>
    /// <returns>The half width of the element.</returns>
    private static double GetHalfWidth(FrameworkElement element)
    {
        var width = element.ActualWidth;
        if (double.IsNaN(width) || width <= 0)
            width = element.Width;

        return width / 2;
    }

    /// <summary>
    /// Converts a pixel delta to a value delta based on the track length and the slider's range.
    /// </summary>
    /// <param name="deltaPixels">The pixel delta to convert.</param>
    /// <param name="track">The track control used for calculations.</param>
    /// <returns>The corresponding value delta.</returns>
    private double PixelsToValue(double deltaPixels, Track track)
    {
        var range = Maximum - Minimum;
        if (range <= 0)
            return 0;

        var trackLength = Orientation == Orientation.Horizontal ? track.ActualWidth : track.ActualHeight;
        if (trackLength <= 0)
            return 0;

        return deltaPixels / trackLength * range;
    }

    /// <summary>
    /// Updates the slider mode and adjusts related properties accordingly.
    /// </summary>
    /// <param name="mode">The new slider mode to apply.</param>
    private void UpdateMode(StswSliderMode mode)
    {
        if (mode == StswSliderMode.Range)
        {
            _storedSelectionRangeEnabled ??= IsSelectionRangeEnabled;

            if (!IsSelectionRangeEnabled)
                SetCurrentValue(IsSelectionRangeEnabledProperty, true);

            if (_valueThumb != null)
                _valueThumb.IsHitTestVisible = false;

            EnsureSelectionBounds();
        }
        else
        {
            if (_storedSelectionRangeEnabled.HasValue)
            {
                SetCurrentValue(IsSelectionRangeEnabledProperty, _storedSelectionRangeEnabled.Value);
                _storedSelectionRangeEnabled = null;
            }

            if (_valueThumb != null)
                _valueThumb.IsHitTestVisible = true;
        }
    }

    /// <summary>
    /// Updates the positions of the range start and end thumbs based on the current SelectionStart and SelectionEnd values.
    /// </summary>
    private void UpdateRangeThumbs()
    {
        if (SliderMode != StswSliderMode.Range)
            return;

        if (_track == null || _rangeStartThumb == null || _rangeEndThumb == null || _canvas == null)
            return;

        var trackLength = Orientation == Orientation.Horizontal ? _track.ActualWidth : _track.ActualHeight;
        if (trackLength <= 0)
            return;

        var startOffset = ValueToPixels(SelectionStart, trackLength);
        var endOffset = ValueToPixels(SelectionEnd, trackLength);

        if (Orientation == Orientation.Horizontal)
        {
            var startLeft = startOffset - GetHalfWidth(_rangeStartThumb);
            var endLeft = endOffset - GetHalfWidth(_rangeEndThumb);

            startLeft = Clamp(startLeft, 0, trackLength - _rangeStartThumb.ActualWidth);
            endLeft = Clamp(endLeft, 0, trackLength - _rangeEndThumb.ActualWidth);

            Canvas.SetLeft(_rangeStartThumb, startLeft);
            Canvas.SetLeft(_rangeEndThumb, endLeft);

            var canvasHeight = _canvas.ActualHeight;
            var startTop = Clamp((canvasHeight - _rangeStartThumb.ActualHeight) / 2, 0, Math.Max(canvasHeight - _rangeStartThumb.ActualHeight, 0));
            var endTop = Clamp((canvasHeight - _rangeEndThumb.ActualHeight) / 2, 0, Math.Max(canvasHeight - _rangeEndThumb.ActualHeight, 0));
            Canvas.SetTop(_rangeStartThumb, startTop);
            Canvas.SetTop(_rangeEndThumb, endTop);

            if (_selectionRangeElement != null)
            {
                var left = Math.Min(startOffset, endOffset);
                var width = Math.Abs(endOffset - startOffset);
                Canvas.SetLeft(_selectionRangeElement, left);
                _selectionRangeElement.Width = width;
                Canvas.SetTop(_selectionRangeElement, startTop);
            }
        }
        else
        {
            var startTop = startOffset - GetHalfHeight(_rangeStartThumb);
            var endTop = endOffset - GetHalfHeight(_rangeEndThumb);

            startTop = Clamp(startTop, 0, trackLength - _rangeStartThumb.ActualHeight);
            endTop = Clamp(endTop, 0, trackLength - _rangeEndThumb.ActualHeight);

            Canvas.SetTop(_rangeStartThumb, startTop);
            Canvas.SetTop(_rangeEndThumb, endTop);

            var canvasWidth = _canvas.ActualWidth;
            var startLeftCentered = Clamp((canvasWidth - _rangeStartThumb.ActualWidth) / 2, 0, Math.Max(canvasWidth - _rangeStartThumb.ActualWidth, 0));
            var endLeftCentered = Clamp((canvasWidth - _rangeEndThumb.ActualWidth) / 2, 0, Math.Max(canvasWidth - _rangeEndThumb.ActualWidth, 0));
            Canvas.SetLeft(_rangeStartThumb, startLeftCentered);
            Canvas.SetLeft(_rangeEndThumb, endLeftCentered);

            if (_selectionRangeElement != null)
            {
                var top = Math.Min(startOffset, endOffset);
                var height = Math.Abs(endOffset - startOffset);
                Canvas.SetTop(_selectionRangeElement, top);
                _selectionRangeElement.Height = height;
                Canvas.SetLeft(_selectionRangeElement, startLeftCentered);
            }
        }
    }

    /// <summary>
    /// Converts a value within the slider's range to a pixel offset along the track.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="trackLength">The length of the track in pixels.</param>
    /// <returns>The corresponding pixel offset.</returns>
    private double ValueToPixels(double value, double trackLength)
    {
        var range = Maximum - Minimum;
        if (range <= 0)
            return 0;

        var normalized = (value - Minimum) / range;
        normalized = Clamp(normalized, 0, 1);

        if (Orientation == Orientation.Vertical)
            normalized = 1 - normalized;

        return normalized * trackLength;
    }

    /// <summary>
    /// Gets the step size for value changes based on whether it's a large or small change.
    /// </summary>
    /// <param name="isLargeChange">Indicates whether to get the large change step size.</param>
    /// <returns>The step size for value changes.</returns>
    private double GetStep(bool isLargeChange)
    {
        if (TickFrequency > 0)
            return TickFrequency;

        var step = isLargeChange ? LargeChange : SmallChange;
        if (step <= 0)
            step = 1;

        return step;
    }

    /// <summary>
    /// Converts a point on the track to a corresponding value within the slider's range.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    /// <returns>The corresponding value, or <see langword="null"/> if conversion is not possible.</returns>
    private double? PointToValue(Point point)
    {
        if (_track == null)
            return null;

        var range = Maximum - Minimum;
        if (range <= 0)
            return null;

        double trackLength;
        double ratio;

        if (Orientation == Orientation.Horizontal)
        {
            trackLength = _track.ActualWidth;
            if (trackLength <= 0)
                return null;

            ratio = point.X / trackLength;
        }
        else
        {
            trackLength = _track.ActualHeight;
            if (trackLength <= 0)
                return null;

            ratio = 1 - (point.Y / trackLength);
        }

        ratio = Clamp(ratio, 0, 1);
        return Minimum + ratio * range;
    }

    /// <summary>
    /// Moves the selected range by a specified delta, ensuring it stays within the slider's bounds.
    /// </summary>
    /// <param name="delta">The amount to move the range by.</param>
    private void MoveRange(double delta)
    {
        var width = SelectionEnd - SelectionStart;
        if (width < 0)
            width = 0;

        var newStart = SelectionStart + delta;
        var newEnd = SelectionEnd + delta;

        if (newStart < Minimum)
        {
            newStart = Minimum;
            newEnd = Minimum + width;
        }
        else if (newEnd > Maximum)
        {
            newEnd = Maximum;
            newStart = Maximum - width;
        }

        newStart = Clamp(newStart, Minimum, Maximum);
        newEnd = Clamp(newEnd, Minimum, Maximum);

        if (newEnd < newStart)
            newEnd = newStart;

        SetCurrentValue(SelectionStartProperty, newStart);
        SetCurrentValue(SelectionEndProperty, newEnd);
    }

    /// <summary>
    /// Snaps a value to the nearest tick mark based on TickFrequency.
    /// </summary>
    /// <param name="value">The value to snap.</param>
    /// <returns>The snapped value.</returns>
    private double SnapToTick(double value)
    {
        if (TickFrequency > 0)
        {
            var steps = Math.Round((value - Minimum) / TickFrequency);
            value = Minimum + steps * TickFrequency;
        }

        return Clamp(value, Minimum, Maximum);
    }

    /// <summary>
    /// Quantizes a delta value to the nearest tick increment based on TickFrequency.
    /// </summary>
    /// <param name="delta">The delta value to quantize.</param>
    /// <returns>The quantized delta value.</returns>
    private double QuantizeDeltaToTick(double delta)
    {
        if (TickFrequency <= 0)
            return delta;

        if (Math.Abs(delta) < double.Epsilon)
            return 0;

        var steps = Math.Round(delta / TickFrequency);
        return steps * TickFrequency;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the mode of the slider, determining whether it operates in single value mode or range selection mode.
    /// </summary>
    public StswSliderMode SliderMode
    {
        get => (StswSliderMode)GetValue(SliderModeProperty);
        set => SetValue(SliderModeProperty, value);
    }
    public static readonly DependencyProperty SliderModeProperty
        = DependencyProperty.Register(
            nameof(SliderMode),
            typeof(StswSliderMode),
            typeof(StswSlider),
            new FrameworkPropertyMetadata(StswSliderMode.Value, OnSliderModeChanged)
        );
    private static void OnSliderModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswSlider stsw)
            return;

        stsw.UpdateMode((StswSliderMode)e.NewValue);
        stsw.UpdateRangeThumbs();
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
            typeof(StswSlider),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the border around the slider's thumb.
    /// Controls the outline width of the draggable element.
    /// </summary>
    public double ThumbBorderThickness
    {
        get => (double)GetValue(ThumbBorderThicknessProperty);
        set => SetValue(ThumbBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty ThumbBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(ThumbBorderThickness),
            typeof(double),
            typeof(StswSlider),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the icon displayed inside the slider thumb.
    /// Allows adding a visual representation, such as a symbol or indicator, within the draggable element.
    /// </summary>
    public Geometry? ThumbIcon
    {
        get => (Geometry?)GetValue(ThumbIconProperty);
        set => SetValue(ThumbIconProperty, value);
    }
    public static readonly DependencyProperty ThumbIconProperty
        = DependencyProperty.Register(
            nameof(ThumbIcon),
            typeof(Geometry),
            typeof(StswSlider),
            new FrameworkPropertyMetadata(default(Geometry?), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the size of the slider thumb.
    /// Defines the dimensions of the draggable element, affecting usability and visual prominence.
    /// </summary>
    public double ThumbSize
    {
        get => (double)GetValue(ThumbSizeProperty);
        set => SetValue(ThumbSizeProperty, value);
    }
    public static readonly DependencyProperty ThumbSizeProperty
        = DependencyProperty.Register(
            nameof(ThumbSize),
            typeof(double),
            typeof(StswSlider),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the size (height or width) of the slider track.
    /// Adjusts the thickness of the track where the thumb moves.
    /// </summary>
    public double TrackSize
    {
        get => (double)GetValue(TrackSizeProperty);
        set => SetValue(TrackSizeProperty, value);
    }
    public static readonly DependencyProperty TrackSizeProperty
        = DependencyProperty.Register(
            nameof(TrackSize),
            typeof(double),
            typeof(StswSlider),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
