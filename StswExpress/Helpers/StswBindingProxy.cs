using System.Windows;

namespace StswExpress;

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
