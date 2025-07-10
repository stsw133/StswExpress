using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress;
/// <summary>
/// An information bar control for displaying messages with optional close and copy buttons.
/// Supports different message types such as info, warning, and error.
/// </summary>
/// <remarks>
/// This control provides a compact way to display notifications with additional functionality like copying text 
/// to the clipboard, expanding for more details, and dismissing messages.
/// </remarks>
[StswInfo("0.5.0")]
public class StswInfoBar : Control, IStswCornerControl
{
    static StswInfoBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswInfoBar), new FrameworkPropertyMetadata(typeof(StswInfoBar)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: copy to clipboard
        if (GetTemplateChild("PART_ButtonCopyToClipboard") is ButtonBase btnCopyToClipboard)
            btnCopyToClipboard.Click += PART_ButtonCopyToClipboard_Click;
        /// Button: close
        if (GetTemplateChild("PART_ButtonClose") is ButtonBase btnClose)
            btnClose.Click += PART_ButtonClose_Click;
    }

    /// <summary>
    /// Copies the content of the information bar (title and text) to the clipboard.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    [StswInfo("0.9.0")]
    private void PART_ButtonCopyToClipboard_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText($"{Title}{Environment.NewLine}{Text}");
    }

    /// <summary>
    /// Handles the close button click event.
    /// Removes the information bar from its parent container if it is placed inside an <see cref="StswInfoPanel"/>.
    /// Otherwise, it is removed from its visual parent.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    private void PART_ButtonClose_Click(object sender, RoutedEventArgs e)
    {
        if (StswFnUI.FindVisualAncestor<StswInfoPanel>(this) is StswInfoPanel stsw)
        {
            var item = stsw.ItemContainerGenerator.ItemFromContainer(VisualParent);
            if (stsw.ItemsSource is IList list)
                list.Remove(item);
            else
                stsw.Items?.Remove(item);
        }
        else StswFnUI.RemoveFromParent(this);
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the info bar can be closed by the user.
    /// When enabled, a close button is displayed.
    /// </summary>
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }
    public static readonly DependencyProperty IsClosableProperty
        = DependencyProperty.Register(
            nameof(IsClosable),
            typeof(bool),
            typeof(StswInfoBar)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the info bar content can be copied to the clipboard.
    /// When enabled, a copy button is displayed.
    /// </summary>
    [StswInfo("0.13.0")]
    public bool IsCopyable
    {
        get => (bool)GetValue(IsCopyableProperty);
        set => SetValue(IsCopyableProperty, value);
    }
    public static readonly DependencyProperty IsCopyableProperty
        = DependencyProperty.Register(
            nameof(IsCopyable),
            typeof(bool),
            typeof(StswInfoBar)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the info bar can be expanded.
    /// When enabled, an expand button is displayed to show additional content.
    /// </summary>
    [StswInfo("0.13.0")]
    public bool IsExpandable
    {
        get => (bool)GetValue(IsExpandableProperty);
        set => SetValue(IsExpandableProperty, value);
    }
    public static readonly DependencyProperty IsExpandableProperty
        = DependencyProperty.Register(
            nameof(IsExpandable),
            typeof(bool),
            typeof(StswInfoBar)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the info bar is currently expanded.
    /// </summary>
    [StswInfo("0.13.0")]
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }
    public static readonly DependencyProperty IsExpandedProperty
        = DependencyProperty.Register(
            nameof(IsExpanded),
            typeof(bool),
            typeof(StswInfoBar)
        );

    /// <summary>
    /// Gets or sets the main text content displayed inside the information bar.
    /// </summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StswInfoBar)
        );

    /// <summary>
    /// Gets or sets the title displayed at the top of the information bar.
    /// </summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public static readonly DependencyProperty TitleProperty
        = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(StswInfoBar)
        );

    /// <summary>
    /// Gets or sets the type of information displayed in the info bar, such as "Info," "Warning," or "Error."
    /// </summary>
    public StswInfoType Type
    {
        get => (StswInfoType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }
    public static readonly DependencyProperty TypeProperty
        = DependencyProperty.Register(
            nameof(Type),
            typeof(StswInfoType),
            typeof(StswInfoBar)
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
            typeof(StswInfoBar),
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
            typeof(StswInfoBar),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswInfoBar Title="Info" Text="Update available" Type="Info" IsCopyable="True"/>

*/
