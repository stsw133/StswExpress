using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

internal class RgsClippingBorder : ContentControl
{
    #region PARTS

    private Border mainBorder;
    private Border visualBorder;
    private ContentPresenter presenter;

    #endregion

    #region OnApplyTemplate

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        mainBorder = GetTemplateChild("PART_MainBorder") as Border;
        visualBorder = GetTemplateChild("PART_VisualBorder") as Border;
        presenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;

        visualBorder.CornerRadius = new CornerRadius(
            Math.Max(mainBorder.CornerRadius.TopLeft - ((mainBorder.BorderThickness.Top + mainBorder.BorderThickness.Left) / 2),0.0),
            Math.Max(mainBorder.CornerRadius.TopRight - ((mainBorder.BorderThickness.Top + mainBorder.BorderThickness.Right) / 2), 0.0),
            Math.Max(mainBorder.CornerRadius.BottomRight - ((mainBorder.BorderThickness.Bottom + mainBorder.BorderThickness.Right) / 2),0.0),
            Math.Max(mainBorder.CornerRadius.TopRight - ((mainBorder.BorderThickness.Bottom + mainBorder.BorderThickness.Left) / 2),0.0)
            );

        presenter.OpacityMask = new VisualBrush(visualBorder);
    }

    #endregion

    #region Dependency Properties


    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }

    // Using a DependencyProperty as the backing store for CornerRadius.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(RgsClippingBorder),
            new FrameworkPropertyMetadata(
                new CornerRadius(10)));


    #endregion
}
