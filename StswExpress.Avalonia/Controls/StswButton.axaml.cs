using Avalonia;
using Avalonia.Controls;
using StswExpress.Core;

namespace StswExpress.Avalonia;

/// <summary>
/// Avalonia implementation of the shared StswExpress button contract.
/// </summary>
public class StswButton : Button, IStswButton<CornerRadius>
{
    static StswButton()
    {
        FocusAdornerProperty.OverrideDefaultValue<StswButton>(null);
        //PseudoClasses.Add(":default");
    }

    /// <summary>
    /// Defines the <see cref="CornerClipping"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> CornerClippingProperty
        = AvaloniaProperty.Register<StswButton, bool>(
            nameof(CornerClipping)
        );

    /// <summary>
    /// Defines the <see cref="CornerRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty
        = AvaloniaProperty.Register<StswButton, CornerRadius>(
            nameof(CornerRadius)
        );

    /// <inheritdoc />
    public bool CornerClipping
    {
        get => GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }

    /// <inheritdoc />
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
}