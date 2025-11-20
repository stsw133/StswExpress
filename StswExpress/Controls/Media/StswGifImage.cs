using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace StswExpress;

/// <summary>
/// An Image control that supports animated GIFs.
/// </summary>
public class StswGifImage : Image
{
    private BitmapDecoder? _decoder;
    private readonly List<TimeSpan> _delays = [];
    private int _frameIndex;
    private DispatcherTimer? _timer;
    private int _loopCount;     // 0 = infinite loops
    private int _loopsPlayed;

    public StswGifImage()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }
    static StswGifImage()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswGifImage), new FrameworkPropertyMetadata(typeof(StswGifImage)));
    }

    #region Events & commands
    /// <summary>
    /// Handles the Loaded event to start the animation if AutoStart is true.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (AutoStart && _decoder != null)
            StartInternal();
    }

    /// <summary>
    /// Handles the Unloaded event to stop the animation.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopInternal();
    }

    /// <summary>
    /// Starts the GIF animation.
    /// </summary>
    public void Start() => IsPlaying = true;

    /// <summary>
    /// Stops the GIF animation.
    /// </summary>
    public void Stop() => IsPlaying = false;

    /// <summary>
    /// Resets the GIF animation to the first frame.
    /// </summary>
    public void Reset()
    {
        Stop();
        _frameIndex = 0;
        _loopsPlayed = 0;
        if (_decoder != null && _decoder.Frames.Count > 0)
            base.Source = _decoder.Frames[0];
    }

    /// <summary>
    /// Loads the GIF from the Source property.
    /// </summary>
    private void LoadGif()
    {
        _decoder = null;
        _delays.Clear();
        _frameIndex = 0;
        _loopCount = 0;
        _loopsPlayed = 0;
        base.Source = null;

        if (Source == null)
            return;

        Stream stream;
        if (Source.IsAbsoluteUri && Source.Scheme != "pack")
        {
            stream = File.OpenRead(Source.LocalPath);
        }
        else
        {
            var sri = Application.GetResourceStream(Source);
            if (sri == null)
                return;
            stream = sri.Stream;
        }

        using (stream)
        {
            BitmapDecoder decoder;

            try
            {
                decoder = BitmapDecoder.Create(
                    stream,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad);
            }
            catch (Exception ex) when (ex is NotSupportedException || ex is FileFormatException)
            {
                StopInternal();
                _decoder = null;
                base.Source = null;
                return;
            }

            if (decoder.Frames.Count == 0)
                return;

            var mime = decoder.CodecInfo?.MimeTypes ?? string.Empty;
            var isGif = mime.Contains("image/gif", StringComparison.OrdinalIgnoreCase);

            if (!isGif)
            {
                StopInternal();
                _decoder = null;
                base.Source = decoder.Frames[0];
                return;
            }

            _decoder = decoder;
            base.Source = _decoder.Frames[0];

            foreach (var f in _decoder.Frames)
            {
                ushort delayCs = 0;
                if (f.Metadata is BitmapMetadata meta && meta.ContainsQuery("/grctlext/Delay"))
                    delayCs = (ushort)(meta.GetQuery("/grctlext/Delay") ?? (ushort)0);

                var ms = Math.Max(10, delayCs * 10);
                _delays.Add(TimeSpan.FromMilliseconds(ms));
            }

            try
            {
                if (_decoder.Metadata is BitmapMetadata containerMeta && containerMeta.ContainsQuery("/appext/Application"))
                {
                    if (containerMeta.ContainsQuery("/appext/Data"))
                    {
                        var bytes = (byte[])containerMeta.GetQuery("/appext/Data");
                        if (bytes != null && bytes.Length >= 3 && bytes[0] == 0x01)
                            _loopCount = bytes[1] | (bytes[2] << 8); // 0 = infinite loops
                    }
                }
            }
            catch
            {
                _loopCount = 0;
            }
        }
    }

    /// <summary>
    /// Starts the GIF animation internally.
    /// </summary>
    private void StartInternal()
    {
        if (_decoder == null || _decoder.Frames.Count == 0)
            return;

        _timer ??= new DispatcherTimer(DispatcherPriority.Render);
        _timer.Tick -= OnTick;
        _timer.Tick += OnTick;

        ScheduleNextTick();
        _timer.Start();
        IsPlaying = true;
    }

    /// <summary>
    /// Stops the GIF animation internally.
    /// </summary>
    private void StopInternal()
    {
        _timer?.Stop();
        IsPlaying = false;
    }

    /// <summary>
    /// Handles the timer tick to update the GIF frame.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OnTick(object? sender, EventArgs e)
    {
        if (_decoder == null)
            return;

        _frameIndex++;
        if (_frameIndex >= _decoder.Frames.Count)
        {
            _frameIndex = 0;
            _loopsPlayed++;

            if (_loopCount > 0 && _loopsPlayed >= _loopCount)
            {
                StopInternal();
                return;
            }
        }

        base.Source = _decoder.Frames[_frameIndex];
        ScheduleNextTick();
    }

    /// <summary>
    /// Schedules the next timer tick based on the current frame's delay and speed ratio.
    /// </summary>
    private void ScheduleNextTick()
    {
        if (_timer == null || _decoder == null) return;
        var delay = _delays.Count > 0 ? _delays[_frameIndex % _delays.Count] : TimeSpan.FromMilliseconds(100);
        if (SpeedRatio <= 0) SpeedRatio = 0.0001; // avoid division by zero
        _timer.Interval = TimeSpan.FromMilliseconds(delay.TotalMilliseconds / SpeedRatio);
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Indicates whether the GIF animation should start automatically when loaded.
    /// </summary>
    public bool AutoStart
    {
        get => (bool)GetValue(AutoStartProperty);
        set => SetValue(AutoStartProperty, value);
    }
    public static readonly DependencyProperty AutoStartProperty
        = DependencyProperty.Register(
            nameof(AutoStart),
            typeof(bool),
            typeof(StswGifImage),
            new PropertyMetadata(true)
        );

    /// <summary>
    /// Indicates whether the GIF animation is currently playing.
    /// </summary>
    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }
    public static readonly DependencyProperty IsPlayingProperty
        = DependencyProperty.Register(
            nameof(IsPlaying),
            typeof(bool),
            typeof(StswGifImage),
            new PropertyMetadata(false, OnIsPlayingChanged)
        );
    private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswGifImage stsw)
            return;

        if ((bool)e.NewValue)
            stsw.StartInternal();
        else
            stsw.StopInternal();
    }

    /// <summary>
    /// The source URI of the GIF image.
    /// </summary>
    public new Uri Source
    {
        get => (Uri)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
    public static new readonly DependencyProperty SourceProperty
        = DependencyProperty.Register(
            nameof(Source),
            typeof(Uri),
            typeof(StswGifImage),
            new PropertyMetadata(null, OnSourceChanged)
        );
    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswGifImage stsw)
            return;

        stsw.StopInternal();
        stsw.LoadGif();
        if (stsw.AutoStart)
            stsw.Start();
    }

    /// <summary>
    /// The speed ratio of the GIF animation. 1.0 = normal speed, 2.0 = double speed, 0.5 = half speed.
    /// </summary>
    public double SpeedRatio
    {
        get => (double)GetValue(SpeedRatioProperty);
        set => SetValue(SpeedRatioProperty, value);
    }
    public static readonly DependencyProperty SpeedRatioProperty
        = DependencyProperty.Register(
            nameof(SpeedRatio),
            typeof(double),
            typeof(StswGifImage),
            new PropertyMetadata(1.0, OnSpeedRatioChanged)
        );
    private static void OnSpeedRatioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswGifImage stsw)
            return;

        if (stsw._timer != null)
            stsw.ScheduleNextTick();
    }
    #endregion
}