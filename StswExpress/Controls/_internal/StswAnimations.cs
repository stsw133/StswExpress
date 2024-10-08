using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace StswExpress;
internal static class StswAnimations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="isSelected"></param>
    internal static void AnimateClick(object? sender, Border target, bool isSelected)
    {
        if (!StswSettings.Default.EnableAnimations)
            return;

        Color fromBackgroundColor, toBackgroundColor;
        Color fromBorderBrushColor, toBorderBrushColor;

        switch (sender)
        {
            case StswCheckBox:
            case StswRadioBox:
                fromBackgroundColor = ((SolidColorBrush)target.FindResource(isSelected ? "StswCheck.Static.Background" : "StswCheck.Checked.Static.Background")).Color;
                toBackgroundColor = ((SolidColorBrush)target.FindResource(isSelected ? "StswCheck.Checked.Static.Background" : "StswCheck.Static.Background")).Color;
                fromBorderBrushColor = ((SolidColorBrush)target.FindResource(isSelected ? "StswCheck.Static.Border" : "StswCheck.Checked.Static.Border")).Color;
                toBorderBrushColor = ((SolidColorBrush)target.FindResource(isSelected ? "StswCheck.Checked.Static.Border" : "StswCheck.Static.Border")).Color;
                break;
                
            case ButtonBase:
                fromBackgroundColor = ((SolidColorBrush)target.FindResource(isSelected ? "StswButton.Static.Background" : "StswButton.Checked.Static.Background")).Color;
                toBackgroundColor = ((SolidColorBrush)target.FindResource(isSelected ? "StswButton.Checked.Static.Background" : "StswButton.Static.Background")).Color;
                fromBorderBrushColor = ((SolidColorBrush)target.FindResource(isSelected ? "StswButton.Static.Border" : "StswButton.Checked.Static.Border")).Color;
                toBorderBrushColor = ((SolidColorBrush)target.FindResource(isSelected ? "StswButton.Checked.Static.Border" : "StswButton.Static.Border")).Color;
                break;

            case ItemsControl:
                fromBackgroundColor = ((SolidColorBrush)target.FindResource(isSelected ? "StswItem.Static.Background" : "StswItem.Checked.Static.Background")).Color;
                toBackgroundColor = ((SolidColorBrush)target.FindResource(isSelected ? "StswItem.Checked.Static.Background" : "StswItem.Static.Background")).Color;
                fromBorderBrushColor = ((SolidColorBrush)target.FindResource(isSelected ? "StswItem.Static.Border" : "StswItem.Checked.Static.Border")).Color;
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
