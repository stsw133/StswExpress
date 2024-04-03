using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;
public static class StswControl
{
    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty IsArrowlessProperty
        = DependencyProperty.RegisterAttached(
            "IsArrowless",
            typeof(bool),
            typeof(StswControl),
            new PropertyMetadata(false, OnIsArrowlessChanged)
        );
    //public static bool GetIsArrowless(DependencyObject obj) => (bool)obj.GetValue(IsArrowlessProperty);
    public static void SetIsArrowless(DependencyObject obj, bool value) => obj.SetValue(IsArrowlessProperty, value);
    private static void OnIsArrowlessChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is FrameworkElement stsw)
        {
            stsw.Resources.Add("StswDropArrow.Visibility", (bool?)e.NewValue == true ? Visibility.Collapsed : Visibility.Visible);
        }
    }

    /// <summary>
    /// Attached property to control the borderless appearance of controls implementing the <see cref="IStswCornerControl"/> interface.
    /// When set to <see langword="true"/>, it hides the border by setting BorderThickness to <c>0</c>,
    /// CornerClipping to <see langword="false"/>, and CornerRadius to <c>0</c>.
    /// </summary>
    public static readonly DependencyProperty IsBorderlessProperty
        = DependencyProperty.RegisterAttached(
            "IsBorderless",
            typeof(bool),
            typeof(StswControl),
            new PropertyMetadata(false, OnIsBorderlessChanged)
        );
    //public static bool GetIsBorderless(DependencyObject obj) => (bool)obj.GetValue(IsBorderlessProperty);
    public static void SetIsBorderless(DependencyObject obj, bool value) => obj.SetValue(IsBorderlessProperty, value);
    private static void OnIsBorderlessChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswCornerControl stsw)
        {
            if ((bool)e.NewValue)
            {
                stsw.BorderThickness = new(0);
                stsw.CornerClipping = false;
                stsw.CornerRadius = new(0);
            }
        }
    }
}

public static class StswPopupControl
{
    /// <summary>
    /// Gets or sets the background brush for the popup.
    /// </summary>
    public static readonly DependencyProperty BackgroundProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswPopup.Background),
            typeof(Brush),
            typeof(StswPopupControl),
            new PropertyMetadata(default, OnBackgroundChanged)
        );
    public static Brush GetBackground(DependencyObject obj) => (Brush)obj.GetValue(BackgroundProperty);
    public static void SetBackground(DependencyObject obj, Brush value) => obj.SetValue(BackgroundProperty, value);
    private static void OnBackgroundChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswDropControl stsw)
        {
            if (stsw.GetPopup() is StswPopup popup)
                popup.Background = (Brush)e.NewValue;
        }
    }

    /// <summary>
    /// Gets or sets the border brush for the popup.
    /// </summary>
    public static readonly DependencyProperty BorderBrushProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswPopup.BorderBrush),
            typeof(Brush),
            typeof(StswPopupControl),
            new PropertyMetadata(default, OnBorderBrushChanged)
        );
    public static Brush GetBorderBrush(DependencyObject obj) => (Brush)obj.GetValue(BorderBrushProperty);
    public static void SetBorderBrush(DependencyObject obj, Brush value) => obj.SetValue(BorderBrushProperty, value);
    private static void OnBorderBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswDropControl stsw)
        {
            if (stsw.GetPopup() is StswPopup popup)
                popup.BorderBrush = (Brush)e.NewValue;
        }
    }

    /// <summary>
    /// Gets or sets the thickness of the border for the popup.
    /// </summary>
    public static readonly DependencyProperty BorderThicknessProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswPopup.BorderThickness),
            typeof(Thickness),
            typeof(StswPopupControl),
            new PropertyMetadata(new Thickness(2), OnBorderThicknessChanged)
        );
    public static Thickness GetBorderThickness(DependencyObject obj) => (Thickness)obj.GetValue(BorderThicknessProperty);
    public static void SetBorderThickness(DependencyObject obj, Thickness value) => obj.SetValue(BorderThicknessProperty, value);
    private static void OnBorderThicknessChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswDropControl stsw)
        {
            if (stsw.GetPopup() is StswPopup popup)
                popup.BorderThickness = (Thickness)e.NewValue;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the popup.
    /// </summary>
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswPopup.CornerClipping),
            typeof(bool),
            typeof(StswPopupControl),
            new PropertyMetadata(true, OnCornerClippingChanged)
        );
    public static bool GetCornerClipping(DependencyObject obj) => (bool)obj.GetValue(CornerClippingProperty);
    public static void SetCornerClipping(DependencyObject obj, bool value) => obj.SetValue(CornerClippingProperty, value);
    private static void OnCornerClippingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswDropControl stsw)
        {
            if (stsw.GetPopup() is StswPopup popup)
                popup.CornerClipping = (bool)e.NewValue;
        }
    }

    /// <summary>
    /// Gets or sets the corner radius for the popup.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswPopup.CornerRadius),
            typeof(CornerRadius),
            typeof(StswPopupControl),
            new PropertyMetadata(new CornerRadius(10), OnCornerRadiusChanged)
        );
    public static CornerRadius GetCornerRadius(DependencyObject obj) => (CornerRadius)obj.GetValue(CornerRadiusProperty);
    public static void SetCornerRadius(DependencyObject obj, CornerRadius value) => obj.SetValue(CornerRadiusProperty, value);
    private static void OnCornerRadiusChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswDropControl stsw)
        {
            if (stsw.GetPopup() is StswPopup popup)
                popup.CornerRadius = (CornerRadius)e.NewValue;
        }
    }

    /// <summary>
    /// Gets or sets the padding for the popup.
    /// </summary>
    public static readonly DependencyProperty PaddingProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswPopup.Padding),
            typeof(Thickness),
            typeof(StswPopupControl),
            new PropertyMetadata(new Thickness(0), OnPaddingChanged)
        );
    public static Thickness GetPadding(DependencyObject obj) => (Thickness)obj.GetValue(PaddingProperty);
    public static void SetPadding(DependencyObject obj, Thickness value) => obj.SetValue(PaddingProperty, value);
    private static void OnPaddingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswDropControl stsw)
        {
            if (stsw.GetPopup() is StswPopup popup)
                popup.Padding = (Thickness)e.NewValue;
        }
    }
}

public static class StswScrollControl
{
    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty AutoScrollProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswScrollViewer.AutoScroll),
            typeof(bool),
            typeof(StswScrollControl),
            new PropertyMetadata(false, OnAutoScrollChanged)
        );
    public static bool GetAutoScroll(DependencyObject obj) => (bool)obj.GetValue(AutoScrollProperty);
    public static void SetAutoScroll(DependencyObject obj, bool value) => obj.SetValue(AutoScrollProperty, value);
    private static void OnAutoScrollChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                scrollViewer.AutoScroll = (bool)e.NewValue;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the content can be scrolled.
    /// </summary>
    public static readonly DependencyProperty CanContentScrollProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswScrollViewer.CanContentScroll),
            typeof(bool),
            typeof(StswScrollControl),
            new PropertyMetadata(false, OnCanContentScrollChanged)
        );
    public static bool GetCanContentScroll(DependencyObject obj) => (bool)obj.GetValue(CanContentScrollProperty);
    public static void SetCanContentScroll(DependencyObject obj, bool value) => obj.SetValue(CanContentScrollProperty, value);
    private static void OnCanContentScrollChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                scrollViewer.CanContentScroll = (bool)e.NewValue;
        }
    }

    /// <summary>
    /// Gets or sets the command associated with the control.
    /// </summary>
    public static readonly DependencyProperty CommandProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswScrollViewer.Command),
            typeof(ICommand),
            typeof(StswScrollControl),
            new PropertyMetadata(null, OnCommandChanged)
        );
    public static ICommand? GetCommand(DependencyObject obj) => (ICommand?)obj.GetValue(CommandProperty);
    public static void SetCommand(DependencyObject obj, ICommand? value) => obj.SetValue(CommandProperty, value);
    private static void OnCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                scrollViewer.Command = (ICommand?)e.NewValue;
        }
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the command associated with the control.
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswScrollViewer.CommandParameter),
            typeof(object),
            typeof(StswScrollControl),
            new PropertyMetadata(null, OnCommandParameterChanged)
        );
    public static object? GetCommandParameter(DependencyObject obj) => (object?)obj.GetValue(CommandParameterProperty);
    public static void SetCommandParameter(DependencyObject obj, object? value) => obj.SetValue(CommandParameterProperty, value);
    private static void OnCommandParameterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                scrollViewer.CommandParameter = (object?)e.NewValue;
        }
    }

    /// <summary>
    /// Gets or sets the target element on which to execute the command associated with the control.
    /// </summary>
    public static readonly DependencyProperty CommandTargetProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswScrollViewer.CommandTarget),
            typeof(IInputElement),
            typeof(StswScrollControl),
            new PropertyMetadata(null, OnCommandTargetChanged)
        );
    public static IInputElement? GetCommandTarget(DependencyObject obj) => (IInputElement?)obj.GetValue(CommandTargetProperty);
    public static void SetCommandTarget(DependencyObject obj, IInputElement? value) => obj.SetValue(CommandTargetProperty, value);
    private static void OnCommandTargetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                scrollViewer.CommandTarget = (IInputElement?)e.NewValue;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty IsBusyProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswScrollViewer.IsBusy),
            typeof(bool),
            typeof(StswScrollControl),
            new PropertyMetadata(false, OnIsBusyChanged)
        );
    public static bool GetIsBusy(DependencyObject obj) => (bool)obj.GetValue(IsBusyProperty);
    public static void SetIsBusy(DependencyObject obj, bool value) => obj.SetValue(IsBusyProperty, value);
    private static void OnIsBusyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                scrollViewer.IsBusy = (bool)e.NewValue;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the scroll bars are dynamic (automatically hide when are not used).
    /// </summary>
    public static readonly DependencyProperty IsDynamicProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswScrollViewer.IsDynamic),
            typeof(bool),
            typeof(StswScrollControl),
            new PropertyMetadata(false, OnIsDynamicChanged)
        );
    public static bool GetIsDynamic(DependencyObject obj) => (bool)obj.GetValue(IsDynamicProperty);
    public static void SetIsDynamic(DependencyObject obj, bool value) => obj.SetValue(IsDynamicProperty, value);
    private static void OnIsDynamicChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                scrollViewer.IsDynamic = (bool)e.NewValue;
        }
    }

    /// <summary>
    /// Gets or sets the panning mode, determining the allowed direction(s) for panning.
    /// </summary>
    public static readonly DependencyProperty PanningModeProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswScrollViewer.PanningMode),
            typeof(PanningMode),
            typeof(StswScrollControl),
            new PropertyMetadata(PanningMode.Both, OnPanningModeChanged)
        );
    public static PanningMode GetPanningMode(DependencyObject obj) => (PanningMode)obj.GetValue(PanningModeProperty);
    public static void SetPanningMode(DependencyObject obj, PanningMode value) => obj.SetValue(PanningModeProperty, value);
    private static void OnPanningModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                scrollViewer.PanningMode = (PanningMode)e.NewValue;
        }
    }

    /// <summary>
    /// Gets or sets the visibility of the horizontal scroll bar.
    /// </summary>
    public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswScrollViewer.HorizontalScrollBarVisibility),
            typeof(ScrollBarVisibility),
            typeof(StswScrollControl),
            new PropertyMetadata(ScrollBarVisibility.Auto, OnHorizontalScrollBarVisibilityChanged)
        );
    public static ScrollBarVisibility GetHorizontalScrollBarVisibility(DependencyObject obj) => (ScrollBarVisibility)obj.GetValue(HorizontalScrollBarVisibilityProperty);
    public static void SetHorizontalScrollBarVisibility(DependencyObject obj, ScrollBarVisibility value) => obj.SetValue(HorizontalScrollBarVisibilityProperty, value);
    private static void OnHorizontalScrollBarVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                scrollViewer.HorizontalScrollBarVisibility = (ScrollBarVisibility)e.NewValue;
        }
    }

    /// <summary>
    /// Gets or sets the visibility of the vertical scroll bar.
    /// </summary>
    public static readonly DependencyProperty VerticalScrollBarVisibilityProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswScrollViewer.VerticalScrollBarVisibility),
            typeof(ScrollBarVisibility),
            typeof(StswScrollControl),
            new PropertyMetadata(ScrollBarVisibility.Auto, OnVerticalScrollBarVisibilityChanged)
        );
    public static ScrollBarVisibility GetVerticalScrollBarVisibility(DependencyObject obj) => (ScrollBarVisibility)obj.GetValue(VerticalScrollBarVisibilityProperty);
    public static void SetVerticalScrollBarVisibility(DependencyObject obj, ScrollBarVisibility value) => obj.SetValue(VerticalScrollBarVisibilityProperty, value);
    private static void OnVerticalScrollBarVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                scrollViewer.VerticalScrollBarVisibility = (ScrollBarVisibility)e.NewValue;
        }
    }
}