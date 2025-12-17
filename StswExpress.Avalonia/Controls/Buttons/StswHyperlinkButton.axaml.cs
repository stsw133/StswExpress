using Avalonia;
using Avalonia.Controls;

namespace StswExpress.Avalonia;
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
public class StswHyperlinkButton : Button
{
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

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsVisitedProperty)
            PseudoClasses.Set(":visited", IsVisited);
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the URI to which the hyperlink button navigates when clicked.
    /// If the URI is valid, it is opened in the default web browser.
    /// </summary>
    public Uri? NavigateUri
    {
        get => GetValue(NavigateUriProperty);
        set => SetValue(NavigateUriProperty, value);
    }
    public static readonly StyledProperty<Uri?> NavigateUriProperty = AvaloniaProperty.Register<StswHyperlinkButton, Uri?>(nameof(NavigateUri));
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether the button has been clicked at least once.
    /// This can be used to track user interaction with the hyperlink button.
    /// </summary>
    public bool IsVisited
    {
        get => GetValue(IsVisitedProperty);
        set => SetValue(IsVisitedProperty, value);
    }
    public static readonly StyledProperty<bool> IsVisitedProperty = AvaloniaProperty.Register<StswHyperlinkButton, bool>(nameof(IsVisited));
    #endregion
}
