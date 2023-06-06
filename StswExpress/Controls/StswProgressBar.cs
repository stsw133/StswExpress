using System.ComponentModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

public class StswProgressBar : ProgressBar
{
    public StswProgressBar()
    {
        OnTextChanged(this, EventArgs.Empty);
        DependencyPropertyDescriptor.FromProperty(MaximumProperty, typeof(ProgressBar)).AddValueChanged(this, OnTextChanged);
        DependencyPropertyDescriptor.FromProperty(MinimumProperty, typeof(ProgressBar)).AddValueChanged(this, OnTextChanged);
        DependencyPropertyDescriptor.FromProperty(TextInPercentsProperty, typeof(ProgressBar)).AddValueChanged(this, OnTextChanged);
        DependencyPropertyDescriptor.FromProperty(ValueProperty, typeof(ProgressBar)).AddValueChanged(this, OnTextChanged);
    }
    static StswProgressBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswProgressBar), new FrameworkPropertyMetadata(typeof(StswProgressBar)));
    }

    #region Events
    /// OnTextChanged
    private void OnTextChanged(object? sender, EventArgs e)
    {
        if (Maximum != Minimum)
        {
            if (TextInPercents == true)
            {
                Text = $"{(int)((Value - Minimum) / (Maximum - Minimum) * 100)} %";
                return;
            }
            else if (TextInPercents == false)
            {
                Text = $"{Value - Minimum} / {Maximum - Minimum}";
                return;
            }
        }
        Text = string.Empty;
    }
    #endregion

    #region Main properties
    /// State
    public enum States
    {
        Ready,
        Running,
        Paused,
        Error
    }
    public static readonly DependencyProperty StateProperty
        = DependencyProperty.Register(
            nameof(State),
            typeof(States),
            typeof(StswProgressBar)
        );
    public States State
    {
        get => (States)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    /// Text
    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StswProgressBar)
        );
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        private set => SetValue(TextProperty, value);
    }

    /// TextInPercents
    public static readonly DependencyProperty TextInPercentsProperty
        = DependencyProperty.Register(
            nameof(TextInPercents),
            typeof(bool?),
            typeof(StswProgressBar)
        );
    public bool? TextInPercents
    {
        get => (bool?)GetValue(TextInPercentsProperty);
        set => SetValue(TextInPercentsProperty, value);
    }
    #endregion

    #region Spatial properties
    /// > CornerRadius ...
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswProgressBar)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion

    #region Style properties
    /// > Foreground ...
    /// ForegroundReady
    public static readonly DependencyProperty ForegroundReadyProperty
        = DependencyProperty.Register(
            nameof(ForegroundReady),
            typeof(Brush),
            typeof(StswProgressBar)
        );
    public Brush ForegroundReady
    {
        get => (Brush)GetValue(ForegroundReadyProperty);
        set => SetValue(ForegroundReadyProperty, value);
    }
    /// ForegroundRunning
    public static readonly DependencyProperty ForegroundRunningProperty
        = DependencyProperty.Register(
            nameof(ForegroundRunning),
            typeof(Brush),
            typeof(StswProgressBar)
        );
    public Brush ForegroundRunning
    {
        get => (Brush)GetValue(ForegroundRunningProperty);
        set => SetValue(ForegroundRunningProperty, value);
    }
    /// ForegroundPaused
    public static readonly DependencyProperty ForegroundPausedProperty
        = DependencyProperty.Register(
            nameof(ForegroundPaused),
            typeof(Brush),
            typeof(StswProgressBar)
        );
    public Brush ForegroundPaused
    {
        get => (Brush)GetValue(ForegroundPausedProperty);
        set => SetValue(ForegroundPausedProperty, value);
    }
    /// ForegroundError
    public static readonly DependencyProperty ForegroundErrorProperty
        = DependencyProperty.Register(
            nameof(ForegroundError),
            typeof(Brush),
            typeof(StswProgressBar)
        );
    public Brush ForegroundError
    {
        get => (Brush)GetValue(ForegroundErrorProperty);
        set => SetValue(ForegroundErrorProperty, value);
    }

    /// > Opacity ...
    /// OpacityDisabled
    public static readonly DependencyProperty OpacityDisabledProperty
        = DependencyProperty.Register(
            nameof(OpacityDisabled),
            typeof(double),
            typeof(StswProgressBar)
        );
    public double OpacityDisabled
    {
        get => (double)GetValue(OpacityDisabledProperty);
        set => SetValue(OpacityDisabledProperty, value);
    }
    #endregion
}
