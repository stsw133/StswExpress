using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace StswExpress.Wpf;

/// <summary>
/// Represents a button styled as a hyperlink, allowing navigation to a specified URI when clicked.
/// This control provides a visually distinct link-like appearance while maintaining button behavior.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswHyperlinkButton Content="Open Website" NavigateUri="https://example.com"/&gt;
/// </code>
/// </example>
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
        if (NavigateUri is { IsAbsoluteUri: true } uri && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            StswFn.OpenPath(NavigateUri.AbsoluteUri);
            IsVisited = true;
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
            typeof(StswHyperlinkButton)
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
            typeof(StswHyperlinkButton)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the hyperlink button has been visited (clicked).
    /// </summary>
    public bool IsVisited
    {
        get => (bool)GetValue(IsVisitedProperty);
        private set => SetValue(IsVisitedPropertyKey, value);
    }
    private static readonly DependencyPropertyKey IsVisitedPropertyKey
        = DependencyProperty.RegisterReadOnly(
            nameof(IsVisited),
            typeof(bool),
            typeof(StswHyperlinkButton),
            new FrameworkPropertyMetadata(false)
        );
    public static readonly DependencyProperty IsVisitedProperty = IsVisitedPropertyKey.DependencyProperty;
    #endregion
}
