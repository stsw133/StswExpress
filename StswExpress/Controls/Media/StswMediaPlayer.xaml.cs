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
/// Represents a control to display media element with additional features such as management panel.
/// </summary>
public class StswMediaPlayer : ItemsControl
{
    static StswMediaPlayer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswMediaPlayer), new FrameworkPropertyMetadata(typeof(StswMediaPlayer)));
    }

    #region Events & methods
    private MediaElement? _mediaElement;
    private readonly Timer timer = new() { AutoReset = true };

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
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

        timer.Elapsed += Timer_Elapsed;
    }

    /// Slider: timeline
    private void PART_Timeline_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_mediaElement != null)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                isUserChangingTimeline = true;
            if (e.LeftButton == MouseButtonState.Released)
            {
                _mediaElement.Position = new TimeSpan((long)((Slider)sender).Value * 10000);
                isUserChangingTimeline = false;
            }
        }
    }
    private bool isUserChangingTimeline;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="step">The step value for shifting through items</param>
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
    /// 
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
    /// 
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (_mediaElement != null && !isUserChangingTimeline)
                TimeCurrent = _mediaElement.Position;
        });
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// 
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
    /// 
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
                    stsw.timer.Start();
                    stsw._mediaElement.Play();
                }
                else if (stsw.IsPlaying == false)
                {
                    stsw.timer.Stop();
                    stsw._mediaElement.Pause();
                }
                else
                {
                    stsw._mediaElement.Position = new TimeSpan(0);
                    stsw.timer.Stop();
                    stsw._mediaElement.Stop();
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the source of the control.
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
    /// 
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
    /// 
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
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
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
            typeof(StswMediaPlayer)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between media element and option panel.
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
            typeof(StswMediaPlayer)
        );
    #endregion
}
