using System.Windows;

namespace StswExpress;

/// <summary>
/// Allows creating a proxy object for data binding purposes.
/// </summary>
public class StswBindingProxy : Freezable
{
    protected override Freezable CreateInstanceCore() => new StswBindingProxy();

    /// Proxy
    public static readonly DependencyProperty ProxyProperty
        = DependencyProperty.Register(
            nameof(Proxy),
            typeof(object),
            typeof(StswBindingProxy)
        );
    public object Proxy
    {
        get => GetValue(ProxyProperty);
        set => SetValue(ProxyProperty, value);
    }
}
