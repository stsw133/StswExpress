using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace StswExpress;

/// <summary>
/// Provides animation utilities for Stsw controls, including click animations for background and border colors.
/// </summary>
/// <remarks>
/// This class defines shared animations for UI elements, particularly for controls that visually respond to selection changes.
/// The animations smoothly transition between colors when an element is selected or deselected.
/// </remarks>
internal static class StswSharedAnimations
{
    /// <summary>
    /// Animates the background and border colors of a target <see cref="Border"/> based on the selected state and control type.
    /// The animation is applied only if animations are enabled in settings.
    /// </summary>
    /// <param name="control">The control initiating the animation, determining the color scheme to use.</param>
    /// <param name="target">The <see cref="Border"/> element to animate.</param>
    /// <param name="isSelected">Indicates whether the target is in the selected state.</param>
    internal static void AnimateClick(Control control, Border? target, bool isSelected)
    {
        if (target == null || !StswSettings.Default.EnableAnimations || !StswControl.GetEnableAnimations(control))
            return;

        Color fromBackgroundColor = ((SolidColorBrush)target.Background).Color;
        Color toBackgroundColor;
        Color fromBorderBrushColor = ((SolidColorBrush) target.BorderBrush).Color;
        Color toBorderBrushColor;

        switch (control)
        {
            case StswCheckBox:
            case StswRadioBox:
                toBackgroundColor = ((SolidColorBrush)target.FindResource(isSelected ? "StswCheck.Checked.Static.Background" : "StswCheck.Static.Background")).Color;
                toBorderBrushColor = ((SolidColorBrush)target.FindResource(isSelected ? "StswCheck.Checked.Static.Border" : "StswCheck.Static.Border")).Color;
                break;
                
            case ButtonBase:
                toBackgroundColor = ((SolidColorBrush)target.FindResource(isSelected ? "StswButton.Checked.Static.Background" : "StswButton.Static.Background")).Color;
                toBorderBrushColor = ((SolidColorBrush)target.FindResource(isSelected ? "StswButton.Checked.Static.Border" : "StswButton.Static.Border")).Color;
                break;

            case ItemsControl:
                toBackgroundColor = ((SolidColorBrush)target.FindResource(isSelected ? "StswItem.Checked.Static.Background" : "StswItem.Static.Background")).Color;
                toBorderBrushColor = ((SolidColorBrush)target.FindResource(isSelected ? "StswItem.Checked.Static.Border" : "StswItem.Static.Border")).Color;
                break;
        }

        if (target.Background is not SolidColorBrush backgroundBrush || backgroundBrush.IsFrozen || !backgroundBrush.CanFreeze)
            target.Background = new SolidColorBrush(fromBackgroundColor);

        if (target.BorderBrush is not SolidColorBrush borderBrush || borderBrush.IsFrozen || !borderBrush.CanFreeze)
            target.BorderBrush = new SolidColorBrush(fromBorderBrushColor);

        var backgroundAnimation = new ColorAnimation
        {
            From = fromBackgroundColor,
            To = toBackgroundColor,
            Duration = TimeSpan.FromSeconds(0.2)
        };

        var borderBrushAnimation = new ColorAnimation
        {
            From = fromBorderBrushColor,
            To = toBorderBrushColor,
            Duration = TimeSpan.FromSeconds(0.2)
        };

        var storyboard = new Storyboard();

        storyboard.Children.Add(backgroundAnimation);
        Storyboard.SetTarget(backgroundAnimation, target);
        Storyboard.SetTargetProperty(backgroundAnimation, new PropertyPath("(Border.Background).(SolidColorBrush.Color)"));

        storyboard.Children.Add(borderBrushAnimation);
        Storyboard.SetTarget(borderBrushAnimation, target);
        Storyboard.SetTargetProperty(borderBrushAnimation, new PropertyPath("(Border.BorderBrush).(SolidColorBrush.Color)"));

        storyboard.Completed += (s, e) =>
        {
            target.ClearValue(Border.BackgroundProperty);
            target.ClearValue(Border.BorderBrushProperty);
        };

        storyboard.Begin();
    }
}
