using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Represents an information bar control that can display a description, title, and type information, with an optional close button.
/// </summary>
public class StswInfoBar : Control, IStswCornerControl
{
    static StswInfoBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswInfoBar), new FrameworkPropertyMetadata(typeof(StswInfoBar)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_CloseBarButton") is ButtonBase closeBarButton)
            closeBarButton.Click += PART_CloseBarButton_Click;
    }

    /// <summary>
    /// Handles the click event of the function button, used for closing the info bar if it's placed within an StswInfoPanel.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_CloseBarButton_Click(object sender, RoutedEventArgs e)
    {
        if (StswFn.FindVisualAncestor<StswInfoPanel>(this) is StswInfoPanel panel)
        {
            var item = panel.ItemContainerGenerator.ItemFromContainer(VisualParent);
            if (panel.ItemsSource is IList list)
                list.Remove(item);
            else
                panel.Items?.Remove(item);
        }
        else StswFn.RemoveFromParent(this);
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the description displayed within the control.
    /// </summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    public static readonly DependencyProperty DescriptionProperty
        = DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(StswInfoBar)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the bar is closable and has a close button.
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
    /// Gets or sets the title displayed within the control.
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
    /// Gets or sets the type of information represented by the control.
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
            typeof(StswInfoBar)
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
            typeof(StswInfoBar)
        );
    #endregion
}
