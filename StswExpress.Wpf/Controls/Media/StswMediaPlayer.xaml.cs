using System;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace StswExpress.Wpf;
/// <summary>
/// A media player control that supports playing audio and video files.
/// Includes playback controls, timeline slider, and mute option.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswMediaPlayer Source="C:\Videos\sample.mp4" IsPlaying="True"/&gt;
/// </code>
/// </example>
[StswPlannedChanges(StswPlannedChanges.NewFeatures)]
public class StswMediaPlayer : ItemsControl
{
    static StswMediaPlayer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswMediaPlayer), new FrameworkPropertyMetadata(typeof(StswMediaPlayer)));
    }
    public StswMediaPlayer()
    {
        _timelineDragStartedHandler = TimelineThumbDragStarted;
        _timelineDragCompletedHandler = TimelineThumbDragCompleted;

        _timer.Interval = 100;
        _timer.Elapsed += Timer_Elapsed;

        Loaded += (_, _) => OnIsPlayingChanged(this, new DependencyPropertyChangedEventArgs(IsPlayingProperty, null, IsPlaying));
        Unloaded += (_, _) => _timer.Stop();
    }

    #region Dependency properties
    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswMediaPlayer)
        );

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswMediaPlayer)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the media is muted.
    /// </summary>
    public bool IsMuted
    {
        get => (bool)GetValue(IsMutedProperty);
        internal set => SetValue(IsMutedProperty, value);
    }
    public static readonly DependencyProperty IsMutedProperty
        = DependencyProperty.Register(
            nameof(IsMuted),
            typeof(bool),
            typeof(StswMediaPlayer),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsMutedChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnIsMutedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswMediaPlayer)d;
        if (stsw._mediaElement != null)
            stsw._mediaElement.IsMuted = stsw.IsMuted;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the media is currently playing.
    /// <see langword="null"/> represents a stopped state.
    /// </summary>
    public bool? IsPlaying
    {
        get => (bool?)GetValue(IsPlayingProperty);
        internal set => SetValue(IsPlayingProperty, value);
    }
    public static readonly DependencyProperty IsPlayingProperty
        = DependencyProperty.Register(
            nameof(IsPlaying),
            typeof(bool?),
            typeof(StswMediaPlayer),
            new FrameworkPropertyMetadata(default(bool?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsPlayingChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswMediaPlayer)d;
        if (stsw._mediaElement == null)
            return;

        if (stsw.IsPlaying == true)
        {
            stsw._timer.Start();
            stsw._mediaElement.Play();
        }
        else if (stsw.IsPlaying == false)
        {
            stsw._timer.Stop();
            stsw._mediaElement.Pause();
        }
        else
        {
            stsw._mediaElement.Position = new TimeSpan(0);
            stsw._timer.Stop();
            stsw._mediaElement.Stop();
        }
    }

    /// <summary>
    /// Gets or sets the source URI of the media file being played.
    /// </summary>
    public Uri Source
    {
        get => (Uri)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
    public static readonly DependencyProperty SourceProperty
        = DependencyProperty.Register(
            nameof(Source),
            typeof(Uri),
            typeof(StswMediaPlayer)
        );

    /// <summary>
    /// Gets or sets the current playback time of the media.
    /// </summary>
    public TimeSpan TimeCurrent
    {
        get => (TimeSpan)GetValue(TimeCurrentProperty);
        internal set => SetValue(TimeCurrentProperty, value);
    }
    public static readonly DependencyProperty TimeCurrentProperty
        = DependencyProperty.Register(
            nameof(TimeCurrent),
            typeof(TimeSpan),
            typeof(StswMediaPlayer)
        );

    /// <summary>
    /// Gets or sets the total duration of the media file.
    /// </summary>
    public TimeSpan TimeMax
    {
        get => (TimeSpan)GetValue(TimeMaxProperty);
        internal set => SetValue(TimeMaxProperty, value);
    }
    public static readonly DependencyProperty TimeMaxProperty
        = DependencyProperty.Register(
            nameof(TimeMax),
            typeof(TimeSpan),
            typeof(StswMediaPlayer)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between the media element and the control panel.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty
        = DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswMediaPlayer),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion

    #region Template
    private readonly Timer _timer = new() { AutoReset = true };
    private readonly DragStartedEventHandler _timelineDragStartedHandler;
    private readonly DragCompletedEventHandler _timelineDragCompletedHandler;

    private ButtonBase? _btnNext;
    private CheckBox? _btnPlay;
    private ButtonBase? _btnPrevious;
    private ButtonBase? _btnStop;
    private CheckBox? _btnMute;
    private MediaElement? _mediaElement;
    private Slider? _timelineSlider;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        DetachTemplateEvents();
        //_btnShuffle = GetTemplateChild("PART_ButtonShuffle") as ButtonBase;
        _btnStop = GetTemplateChild("PART_ButtonStop") as ButtonBase;
        _btnPrevious = GetTemplateChild("PART_ButtonPrevious") as ButtonBase;
        _btnPlay = GetTemplateChild("PART_ButtonPlay") as CheckBox;
        _btnNext = GetTemplateChild("PART_ButtonNext") as ButtonBase;
        //_btnRepeat = GetTemplateChild("PART_ButtonRepeat") as ButtonBase;
        _btnMute = GetTemplateChild("PART_ButtonMute") as CheckBox;
        AttachTemplateEvents();

        /// Slider: timeline
        if (_timelineSlider != null)
        {
            _timelineSlider.PreviewMouseDown -= Timeline_PreviewMouseDown;
            _timelineSlider.PreviewMouseUp -= Timeline_PreviewMouseUp;
            _timelineSlider.ValueChanged -= Timeline_ValueChanged;
            _timelineSlider.RemoveHandler(Thumb.DragStartedEvent, _timelineDragStartedHandler);
            _timelineSlider.RemoveHandler(Thumb.DragCompletedEvent, _timelineDragCompletedHandler);
        }

        if (GetTemplateChild("PART_Timeline") is Slider timeline)
        {
            _timelineSlider = timeline;
            timeline.PreviewMouseDown += Timeline_PreviewMouseDown;
            timeline.PreviewMouseUp += Timeline_PreviewMouseUp;
            timeline.ValueChanged += Timeline_ValueChanged;
            timeline.AddHandler(Thumb.DragStartedEvent, _timelineDragStartedHandler, true);
            timeline.AddHandler(Thumb.DragCompletedEvent, _timelineDragCompletedHandler, true);
        }
        else
        {
            _timelineSlider = null;
        }

        /// MediaElement
        if (_mediaElement != null)
        {
            _mediaElement.MediaEnded -= MediaElement_MediaEnded;
            _mediaElement.MediaOpened -= MediaElement_MediaOpened;
        }

        if (GetTemplateChild("PART_MediaElement") is MediaElement mediaElement)
        {
            _mediaElement = mediaElement;
            _mediaElement.MediaEnded += MediaElement_MediaEnded;
            _mediaElement.MediaOpened += MediaElement_MediaOpened;
            _mediaElement.IsMuted = IsMuted;
        }
        else
        {
            _mediaElement = null;
        }
    }

    /// <summary>
    /// Attaches event handlers to the template parts.
    /// </summary>
    private void AttachTemplateEvents()
    {
        //if (GetTemplateChild("PART_ButtonShuffle") is CheckBox btnShuffle)
        //    btnShuffle.Click += ButtonShuffle_Click;
        if (_btnStop != null)
            _btnStop.Click += BtnStop_Click;
        if (_btnPrevious != null)
            _btnPrevious.Click += BtnPrevious_Click;
        if (_btnPlay != null)
            _btnPlay.Click += BtnPlay_Click;
        if (_btnNext != null)
            _btnNext.Click += BtnNext_Click;
        //if (GetTemplateChild("PART_ButtonRepeat") is CheckBox btnRepeat)
        //    btnRepeat.Click += BtnRepeat_Click;
        if (_btnMute != null)
            _btnMute.Click += BtnMute_Click;
    }

    /// <summary>
    /// Detaches event handlers from the template parts.
    /// </summary>
    private void DetachTemplateEvents()
    {
        if (_btnStop != null)
            _btnStop.Click -= BtnStop_Click;
        if (_btnPrevious != null)
            _btnPrevious.Click -= BtnPrevious_Click;
        if (_btnPlay != null)
            _btnPlay.Click -= BtnPlay_Click;
        if (_btnNext != null)
            _btnNext.Click -= BtnNext_Click;
        if (_btnMute != null)
            _btnMute.Click -= BtnMute_Click;
    }
    #endregion

    #region Logic
    private bool _isUserChangingTimeline;

    /// <summary>
    /// Handles the click event of the mute button.
    /// </summary>
    /// <param name="sender">The sender object (button)</param>
    /// <param name="e">The event arguments</param>
    private void BtnMute_Click(object sender, RoutedEventArgs e) => IsMuted = _btnMute?.IsChecked == true;

    /// <summary>
    /// Handles the click event of the next button.
    /// </summary>
    /// <param name="sender">The sender object (button)</param>
    /// <param name="e">The event arguments</param>
    private void BtnNext_Click(object sender, RoutedEventArgs e) => ShiftBy(1);

    /// <summary>
    /// Handles the click event of the play button.
    /// </summary>
    /// <param name="sender">The sender object (button)</param>
    /// <param name="e">The event arguments</param>
    private void BtnPlay_Click(object sender, RoutedEventArgs e) => IsPlaying = IsPlaying != true;

    /// <summary>
    /// Handles the click event of the previous button.
    /// </summary>
    /// <param name="sender">The sender object (button)</param>
    /// <param name="e">The event arguments</param>
    private void BtnPrevious_Click(object sender, RoutedEventArgs e) => ShiftBy(-1);

    /// <summary>
    /// Handles the click event of the stop button.
    /// </summary>
    /// <param name="sender">The sender object (button)</param>
    /// <param name="e">The event arguments</param>
    private void BtnStop_Click(object sender, RoutedEventArgs e) => IsPlaying = null;

    /// <summary>
    /// Handles the mouse down event on the timeline slider.
    /// </summary>
    /// <param name="sender">The sender object (slider)</param>
    /// <param name="e">The event arguments</param>
    private void Timeline_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _isUserChangingTimeline = true;

            if (sender is Slider slider)
                TimeCurrent = TimeSpan.FromMilliseconds(slider.Value);
        }
    }

    /// <summary>
    /// Handles the mouse up event on the timeline slider.
    /// </summary>
    /// <param name="sender">The sender object (slider)</param>
    /// <param name="e">The event arguments</param>
    private void Timeline_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is Slider slider)
            CommitTimelineValue(slider.Value);

        _isUserChangingTimeline = false;
    }

    /// <summary>
    /// Handles the drag started event on the timeline slider thumb.
    /// </summary>
    /// <param name="sender">The sender object (thumb)</param>
    /// <param name="e">The event arguments</param>
    private void TimelineThumbDragStarted(object sender, DragStartedEventArgs e)
    {
        _isUserChangingTimeline = true;
    }

    /// <summary>
    /// Handles the drag completed event on the timeline slider thumb.
    /// </summary>
    /// <param name="sender">The sender object (thumb)</param>
    /// <param name="e">The event arguments</param>
    private void TimelineThumbDragCompleted(object sender, DragCompletedEventArgs e)
    {
        if (_timelineSlider != null)
            CommitTimelineValue(_timelineSlider.Value);

        _isUserChangingTimeline = false;
    }

    /// <summary>
    /// Handles the value changed event on the timeline slider.
    /// </summary>
    /// <param name="sender">The sender object (slider)</param>
    /// <param name="e">The event arguments</param>
    private void Timeline_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isUserChangingTimeline)
            TimeCurrent = TimeSpan.FromMilliseconds(e.NewValue);
    }

    /// <summary>
    /// Commits the new timeline value to the media element.
    /// </summary>
    /// <param name="value">The new timeline value in milliseconds</param>
    private void CommitTimelineValue(double value)
    {
        var newPosition = TimeSpan.FromMilliseconds(value);

        if (_mediaElement != null)
            _mediaElement.Position = newPosition;

        TimeCurrent = newPosition;
    }

    /// <summary>
    /// Handles the media ended event.
    /// </summary>
    /// <param name="sender">The sender object (media element)</param>
    /// <param name="e">The event arguments</param>
    private void MediaElement_MediaEnded(object? sender, RoutedEventArgs e)
    {
        if (CanShiftBy(1))
            ShiftBy(1);
        else
            IsPlaying = null;
    }

    /// <summary>
    /// Handles the media opened event.
    /// </summary>
    /// <param name="sender">The sender object (media element)</param>
    /// <param name="e">The event arguments</param>
    private void MediaElement_MediaOpened(object? sender, RoutedEventArgs e)
    {
        if (_mediaElement != null && _mediaElement.NaturalDuration.HasTimeSpan)
            TimeMax = _mediaElement.NaturalDuration.TimeSpan;
        else
            TimeMax = TimeSpan.Zero;

        TimeCurrent = TimeSpan.Zero;
        OnIsPlayingChanged(this, new DependencyPropertyChangedEventArgs(IsPlayingProperty, null, IsPlaying));
    }

    /// <summary>
    /// Determines whether the media source can be shifted forward or backward in the playlist.
    /// </summary>
    /// <param name="step">The step value for shifting through items</param>
    /// <returns>True if shifting is possible, otherwise false.</returns>
    private bool CanShiftBy(int step)
    {
        if (HasItems)
        {
            if (Items.IndexOf(Source.OriginalString) is int index && (index + step).Between(0, Items.Count - 1))
                return true;
        }
        else if (Source != null && Directory.GetParent(Source.OriginalString) is DirectoryInfo info)
        {
            var files = Directory.GetFiles(info.FullName).ToList();
            if (files.IndexOf(Source.OriginalString) is int index && (index + step).Between(0, files.Count - 1))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Changes the currently playing media to the next or previous item in the playlist.
    /// </summary>
    /// <param name="step">The step value for shifting through items</param>
    private void ShiftBy(int step)
    {
        if (_mediaElement != null)
        {
            if (HasItems)
            {
                if (Items.IndexOf(Source.OriginalString) is int index && (index + step).Between(0, Items.Count - 1))
                {
                    Source = new Uri(Items[index + step].ToString()!);
                    _mediaElement.Play();
                }
            }
            else if (Source != null && Directory.GetParent(Source.OriginalString) is DirectoryInfo info)
            {
                var files = Directory.GetFiles(info.FullName).ToList();
                if (files.IndexOf(Source.OriginalString) is int index && (index + step).Between(0, files.Count - 1))
                {
                    Source = new Uri(files[index + step]);
                    _mediaElement.Play();
                }
            }
        }
    }

    /// <summary>
    /// Updates the current playback position of the media periodically.
    /// </summary>
    /// <param name="sender">The timer triggering the update</param>
    /// <param name="e">The event arguments</param>
    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.InvokeAsync(() =>
        {
            if (_mediaElement != null && !_isUserChangingTimeline)
                TimeCurrent = _mediaElement.Position;
        }, DispatcherPriority.Background);
    }
    #endregion
}

