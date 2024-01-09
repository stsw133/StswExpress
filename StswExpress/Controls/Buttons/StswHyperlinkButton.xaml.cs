using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Represents a button control that functions as a hyperlink, allowing navigation to a specified URI when clicked.
/// </summary>
public class StswHyperlinkButton : ButtonBase, IStswCornerControl
{
    static StswHyperlinkButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswHyperlinkButton), new FrameworkPropertyMetadata(typeof(StswHyperlinkButton)));
    }

    #region Events & methods
    /// <summary>
    /// Handles the click event of the hyperlink button to open the specified URI if available.
    /// </summary>
    protected override void OnClick()
    {
        base.OnClick();

        if (NavigateUri?.AbsoluteUri != null)
            StswFn.OpenFile(NavigateUri.AbsoluteUri);
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the URI source that the hyperlink button navigates to when clicked.
    /// </summary>
    public Uri NavigateUri
    {
        get => (Uri)GetValue(NavigateUriProperty);
        set => SetValue(NavigateUriProperty, value);
    }
    public static readonly DependencyProperty NavigateUriProperty
        = DependencyProperty.Register(
            nameof(NavigateUri),
            typeof(Uri),
            typeof(StswHyperlinkButton)
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
            typeof(StswHyperlinkButton)
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
            typeof(StswHyperlinkButton)
        );
    #endregion
}
