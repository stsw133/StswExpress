﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace StswExpress;

/// <summary>
/// Provides a visual ripple effect that is applied as an adorner to a UI element.
/// The ripple effect is centered at the click position and is animated within the bounds of a specified border.
/// </summary>
/// <remarks>
/// The ripple effect is created using an expanding and fading <see cref="Ellipse"/>.
/// It is clipped to the provided <see cref="Border"/> to ensure it does not exceed its bounds.
/// The animation scales the ripple from 0 to full size while gradually reducing its opacity.
/// </remarks>
[StswInfo("0.10.0")]
internal class StswRippleAdorner : Adorner
{
    private readonly Border _border;
    private readonly Canvas _canvas;
    private readonly Ellipse _ellipse;
    private readonly ScaleTransform _scaleTransform;
    private readonly TranslateTransform _translateTransform;

    /// <summary>
    /// Initializes a new instance of the <see cref="StswRippleAdorner"/> class.
    /// </summary>
    /// <param name="adornedElement">The UI element to which the ripple effect is applied.</param>
    /// <param name="clickPosition">The position of the mouse click, which serves as the center of the ripple effect.</param>
    /// <param name="size">The initial size of the ripple effect.</param>
    /// <param name="border">The border within which the ripple effect is clipped.</param>
    public StswRippleAdorner(UIElement adornedElement, Point clickPosition, double size, Border border) : base(adornedElement)
    {
        _border = border;

        _canvas = new Canvas
        {
            Height = adornedElement.RenderSize.Height,
            Width = adornedElement.RenderSize.Width
        };

        _translateTransform = new TranslateTransform();
        _scaleTransform = new ScaleTransform(0, 0);

        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(_scaleTransform);
        transformGroup.Children.Add(_translateTransform);

        _ellipse = new Ellipse
        {
            Fill = new SolidColorBrush(Color.FromArgb(70, 120, 120, 120)),
            IsHitTestVisible = false,
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = transformGroup,
            Height = size,
            Width = size
        };

        _canvas.Children.Add(_ellipse);
        AddVisualChild(_canvas);

        Canvas.SetLeft(_ellipse, clickPosition.X - size / 2);
        Canvas.SetTop(_ellipse, clickPosition.Y - size / 2);

        _canvas.Clip = new RectangleGeometry(new Rect(0, 0, _border.RenderSize.Width, _border.RenderSize.Height), _border.CornerRadius.TopLeft, _border.CornerRadius.TopLeft);

        AnimateRipple();
    }

    /// <summary>
    /// Starts the animation of the ripple effect, scaling the ellipse from a small size to its full size
    /// while fading out its opacity.
    /// </summary>
    private void AnimateRipple()
    {
        var sb = new Storyboard();

        var scaleXAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(700))
        {
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTargetProperty(scaleXAnim, new PropertyPath("RenderTransform.Children[0].ScaleX"));
        sb.Children.Add(scaleXAnim);

        var scaleYAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(700))
        {
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTargetProperty(scaleYAnim, new PropertyPath("RenderTransform.Children[0].ScaleY"));
        sb.Children.Add(scaleYAnim);

        var opacityAnim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(900))
        {
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(OpacityProperty));
        sb.Children.Add(opacityAnim);

        sb.Completed += (s, args) => AdornerLayer.GetAdornerLayer(AdornedElement)?.Remove(this);

        sb.Begin(_ellipse);
    }

    /// <inheritdoc/>
    protected override int VisualChildrenCount => 1;
    
    /// <inheritdoc/>
    protected override Visual GetVisualChild(int index) => _canvas;

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        _canvas.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
        return finalSize;
    }
}
