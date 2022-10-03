using System.Windows;

namespace StswExpress
{
    public class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore() => new BindingProxy();

        /// Proxy
        public static readonly DependencyProperty ProxyProperty
            = DependencyProperty.Register(
                nameof(Proxy),
                typeof(object),
                typeof(BindingProxy),
                new UIPropertyMetadata(null)
            );
        public object Proxy
        {
            get => GetValue(ProxyProperty);
            set => SetValue(ProxyProperty, value);
        }
    }
}
