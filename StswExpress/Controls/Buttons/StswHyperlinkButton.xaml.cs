using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Represents a button styled as a hyperlink, allowing navigation to a specified URI when clicked.
/// This control provides a visually distinct link-like appearance while maintaining button behavior.
/// </summary>
[Stsw("0.5.0", Changes = StswPlannedChanges.None)]
public class StswHyperlinkButton : ButtonBase, IStswCornerControl
{
    static StswHyperlinkButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswHyperlinkButton), new FrameworkPropertyMetadata(typeof(StswHyperlinkButton)));
    }

    #region Events & methods
    /// <inheritdoc/>
    protected override void OnClick()
    {
        base.OnClick();

        if (NavigateUri != null && !string.IsNullOrEmpty(NavigateUri.AbsoluteUri))
        {
            StswFn.OpenPath(NavigateUri.AbsoluteUri);
            WasClicked = true;
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the URI to which the hyperlink button navigates when clicked. 
    /// If the URI is valid, it is opened in the default web browser.
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
    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswHyperlinkButton),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswHyperlinkButton),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the button has been clicked at least once.
    /// This can be used to track user interaction with the hyperlink button.
    /// </summary>
    public bool WasClicked
    {
        get => (bool)GetValue(WasClickedProperty);
        set => SetValue(WasClickedProperty, value);
    }
    public static readonly DependencyProperty WasClickedProperty
        = DependencyProperty.Register(
            nameof(WasClicked),
            typeof(bool),
            typeof(StswHyperlinkButton)
        );
    #endregion
}

/* usage:

<se:StswHyperlinkButton Content="Open Website" NavigateUri="https://example.com"/>

*/
