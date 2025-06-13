using System;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// A media player control that supports playing audio and video files.
/// Includes playback controls, timeline slider, and mute option.
/// </summary>
public class StswMediaPlayer : ItemsControl
{
    private readonly Timer _timer = new() { AutoReset = true };
    private MediaElement? _mediaElement;

    static StswMediaPlayer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswMediaPlayer), new FrameworkPropertyMetadata(typeof(StswMediaPlayer)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswMediaPlayer), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: shuffle
        //if (GetTemplateChild("PART_ButtonShuffle") is CheckBox btnShuffle)
        //    btnShuffle.Click += ButtonShuffle_Click;
        /// Button: stop
        if (GetTemplateChild("PART_ButtonStop") is ButtonBase btnStop)
            btnStop.Click += (_, _) => IsPlaying = null;
        /// Button: previous
        if (GetTemplateChild("PART_ButtonPrevious") is ButtonBase btnPrevious)
            btnPrevious.Click += (_, _) => ShiftBy(-1);
        /// Button: play
        if (GetTemplateChild("PART_ButtonPlay") is CheckBox btnPlay)
            btnPlay.Click += (_, _) => IsPlaying = IsPlaying != true;
        /// Button: next
        if (GetTemplateChild("PART_ButtonNext") is ButtonBase btnNext)
            btnNext.Click += (_, _) => ShiftBy(1);
        /// Button: repeat
        //if (GetTemplateChild("PART_ButtonRepeat") is CheckBox btnRepeat)
        //    btnRepeat.Click += BtnRepeat_Click;
        /// Button: mute
        if (GetTemplateChild("PART_ButtonMute") is CheckBox btnMute)
            btnMute.Click += (_, _) => IsMuted = btnMute.IsChecked == true;
        /// Slider: timeline
        if (GetTemplateChild("PART_Timeline") is Slider timeline)
            timeline.PreviewMouseUp += PART_Timeline_PreviewMouseUp;

        /// MediaElement
        if (GetTemplateChild("PART_MediaElement") is MediaElement mediaElement)
        {
            mediaElement.MediaEnded += (_, _) => { if (CanShiftBy(1)) ShiftBy(1); else IsPlaying = null; };
            mediaElement.MediaOpened += (_, _) => TimeMax = mediaElement.NaturalDuration.HasTimeSpan ? mediaElement.NaturalDuration.TimeSpan : new();
            mediaElement.SourceUpdated += (_, _) => IsPlaying = true;
            _mediaElement = mediaElement;
        }

        _timer.Elapsed += Timer_Elapsed;
    }

    /// <summary>
    /// Handles the mouse up event on the timeline slider.
    /// Updates the media position based on the user's input.
    /// </summary>
    /// <param name="sender">The sender object (slider)</param>
    /// <param name="e">The event arguments</param>
    private void PART_Timeline_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_mediaElement != null)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                _isUserChangingTimeline = true;
            if (e.LeftButton == MouseButtonState.Released)
            {
                _mediaElement.Position = new TimeSpan((long)((Slider)sender).Value * 10000);
                _isUserChangingTimeline = false;
            }
        }
    }
    private bool _isUserChangingTimeline;

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
            if (files.IndexOf(Source.OriginalString) is int index && (index + step).Between(0, Items.Count - 1))
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
                if (files.IndexOf(Source.OriginalString) is int index && (index + step).Between(0, Items.Count - 1))
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
        Dispatcher.Invoke(() =>
        {
            if (_mediaElement != null && !_isUserChangingTimeline)
                TimeCurrent = _mediaElement.Position;
        });
    }
    #endregion

    #region Logic properties
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
    public static void OnIsMutedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswMediaPlayer stsw)
        {
            if (stsw._mediaElement != null)
                stsw._mediaElement.IsMuted = stsw.IsMuted;
        }
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
    public static void OnIsPlayingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswMediaPlayer stsw)
        {
            if (stsw._mediaElement != null)
            {
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
    #endregion

    #region Style properties
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
            typeof(StswMediaPlayer),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
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
            typeof(StswMediaPlayer),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
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
}

/* usage:

<se:StswMediaPlayer Source="C:\Videos\sample.mp4" IsPlaying="True"/>

*/
