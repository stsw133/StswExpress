using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Represents a control to display media element with additional features such as management panel.
/// </summary>
public class StswMediaElement : Control
{
    static StswMediaElement()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswMediaElement), new FrameworkPropertyMetadata(typeof(StswMediaElement)));
    }

    #region Events & methods
    private MediaElement? mediaElement;
    //private Slider? timeline;
    //private Timer timer = new Timer(1000);

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Slider: timeline
        /*if (GetTemplateChild("PART_Timeline") is Slider timeline)
        {
            timeline.ValueChanged += Timeline_ValueChanged;
            this.timeline = timeline;
        }*/
        /// Button: repeat
        if (GetTemplateChild("PART_BtnRepeat") is ButtonBase btnRepeat)
            btnRepeat.Click += BtnReplay_Click;
        /// Button: stop
        if (GetTemplateChild("PART_BtnStop") is ButtonBase btnStop)
            btnStop.Click += BtnStop_Click;
        /// Button: previous
        if (GetTemplateChild("PART_BtnPrevious") is ButtonBase btnPrevious)
            btnPrevious.Click += BtnPrevious_Click;
        /// Button: play
        if (GetTemplateChild("PART_BtnPlay") is CheckBox btnPlay)
            btnPlay.Click += BtnPlay_Click;
        /// Button: next
        if (GetTemplateChild("PART_BtnNext") is ButtonBase btnNext)
            btnNext.Click += BtnNext_Click;
        /// Button: mute
        if (GetTemplateChild("PART_BtnMute") is CheckBox btnMute)
            btnMute.Click += BtnMute_Click;

        /// MediaElement
        if (GetTemplateChild("PART_MediaElement") is MediaElement mediaElement)
        {
            mediaElement.MediaEnded += BtnStop_Click;
            mediaElement.SourceUpdated += (s, e) => { IsPlaying = false; BtnPlay_Click(this, new RoutedEventArgs()); };
            //mediaElement.MediaOpened += (s, e) => this.timeline.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            this.mediaElement = mediaElement;
        }

        //timer.Elapsed += Timer_Ellapsed;
        //timer.Start();
    }

    /// Timer
    /*private void Timer_Ellapsed(object? sender, ElapsedEventArgs e)
    {
        if (timeline != null && mediaElement != null)
            timeline.Value = mediaElement.Position.TotalMilliseconds;
    }*/

    /// Slider: timeline
    /*private void Timeline_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
    {
        if (timeline != null && mediaElement != null)
            mediaElement.Position = new TimeSpan(0, 0, 0, 0, (int)timeline.Value);
    }*/

    /// Button: replay
    private void BtnReplay_Click(object sender, RoutedEventArgs e)
    {
        if (mediaElement != null)
        {
            mediaElement.Stop();
            mediaElement.Play();
        }
    }

    /// Button: stop
    private void BtnStop_Click(object sender, RoutedEventArgs e)
    {
        if (mediaElement != null)
        {
            mediaElement.Position = new TimeSpan(0);
            mediaElement.Stop();
            IsPlaying = false;
        }
    }

    /// Button: previous
    private void BtnPrevious_Click(object sender, RoutedEventArgs e)
    {
        if (mediaElement != null)
        {
            if (Source != null && Directory.GetParent(Source.AbsolutePath) is DirectoryInfo info and not null)
            {
                var files = Directory.GetFiles(info.FullName).ToList();
                var currentIndex = files.IndexOf(Source.LocalPath);
                if (currentIndex - 1 >= 0)
                {
                    Source = new Uri(files[currentIndex - 1]);
                    mediaElement.Play();
                }
            }
        }
    }

    /// Button: play
    private void BtnPlay_Click(object sender, RoutedEventArgs e)
    {
        if (mediaElement != null)
        {
            if (IsPlaying)
            {
                mediaElement.Pause();
                IsPlaying = false;
            }
            else
            {
                mediaElement.Play();
                IsPlaying = true;
            }
        }
    }

    /// Button: next
    private void BtnNext_Click(object sender, RoutedEventArgs e)
    {
        if (mediaElement != null)
        {
            if (Source != null && Directory.GetParent(Source.AbsolutePath) is DirectoryInfo info and not null)
            {
                var files = Directory.GetFiles(info.FullName).ToList();
                var currentIndex = files.IndexOf(Source.LocalPath);
                if (files.Count > currentIndex + 1)
                {
                    Source = new Uri(files[currentIndex + 1]);
                    mediaElement.Play();
                }
            }
        }
    }

    /// Button: mute
    private void BtnMute_Click(object sender, RoutedEventArgs e)
    {
        if (mediaElement != null)
        {
            if (sender is CheckBox btn)
            {
                mediaElement.IsMuted = !mediaElement.IsMuted;
                btn.IsChecked = mediaElement.IsMuted;
            }
            //mediaElement.Volume = mediaElement.IsMuted ? 0 : 1;
        }
    }
    #endregion

    #region Main properties
    /// <summary>
    /// 
    /// </summary>
    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        internal set => SetValue(IsPlayingProperty, value);
    }
    public static readonly DependencyProperty IsPlayingProperty
        = DependencyProperty.Register(
            nameof(IsPlaying),
            typeof(bool),
            typeof(StswMediaElement)
        );

    /// <summary>
    /// Gets or sets the source of the control.
    /// </summary>
    public Uri? Source
    {
        get => (Uri?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
    public static readonly DependencyProperty SourceProperty
        = DependencyProperty.Register(
            nameof(Source),
            typeof(Uri),
            typeof(StswMediaElement)
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
            typeof(StswMediaElement)
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
            typeof(StswMediaElement)
        );
    #endregion
}
