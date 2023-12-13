using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Represents a control to display media element with additional features such as management panel.
/// </summary>
internal class StswMediaPlayer : Control
{
    static StswMediaPlayer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswMediaPlayer), new FrameworkPropertyMetadata(typeof(StswMediaPlayer)));
    }

    #region Events & methods
    private MediaElement? mediaElement;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// MediaElement
        if (GetTemplateChild("PART_MediaElement") is MediaElement mediaElement)
            this.mediaElement = mediaElement;
        /// Button: repeat
        if (GetTemplateChild("PART_BtnRepeat") is ButtonBase btnRepeat)
            btnRepeat.Click += (s, e) => this.mediaElement?.Play();
        /// Button: stop
        if (GetTemplateChild("PART_BtnStop") is ButtonBase btnStop)
            btnStop.Click += (s, e) => this.mediaElement?.Stop();
        /// Button: pause
        if (GetTemplateChild("PART_BtnPause") is ButtonBase btnPause)
            btnPause.Click += (s, e) => this.mediaElement?.Pause();
        /// Button: previous
        if (GetTemplateChild("PART_BtnPrevious") is ButtonBase btnPrevious)
            btnPrevious.Click += (s, e) => this.mediaElement?.Play();
        /// Button: play
        if (GetTemplateChild("PART_BtnPlay") is CheckBox btnPlay)
        {
            btnPlay.Checked += BtnPlay_Checked;
            btnPlay.Unchecked += BtnPlay_Unchecked;
        }
        /// Button: next
        if (GetTemplateChild("PART_BtnNext") is ButtonBase btnNext)
            btnNext.Click += (s, e) => this.mediaElement?.Play();
        /// Button: mute
        if (GetTemplateChild("PART_BtnMute") is CheckBox btnMute)
            btnMute.Click += BtnMute_Click;
    }

    /// Button: play (OnChecked)
    private void BtnPlay_Checked(object sender, RoutedEventArgs e)
    {
        mediaElement?.Play();
    }

    /// Button: play (OnUnchecked)
    private void BtnPlay_Unchecked(object sender, RoutedEventArgs e)
    {
        mediaElement?.Stop();
    }

    /// Button: mute
    private void BtnMute_Click(object sender, RoutedEventArgs e)
    {
        if (mediaElement != null)
        {
            mediaElement.IsMuted = !mediaElement.IsMuted;
            mediaElement.Volume = mediaElement.IsMuted ? 0 : 1;
        }
    }
    #endregion

    #region Main properties
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
            typeof(StswMediaPlayer)
        );
    #endregion
}
