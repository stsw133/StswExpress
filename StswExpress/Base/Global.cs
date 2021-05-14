using System.Reflection;
using System.Windows;

namespace StswExpress
{
    public static class Global
    {
        /// App version
        public static string AppVersion() => Assembly.GetEntryAssembly().GetName().Version.ToString().TrimEnd(".0".ToCharArray());

        /// App name + version
        public static string AppName => $"{Assembly.GetEntryAssembly().GetName().Name} {AppVersion()}";

        /// Chosen database
        public static M_Database AppDatabase { get; set; } = new M_Database();

        /// Logged user
        public static M_User AppUser { get; set; } = new M_User();

        #region Proxy
        public class BindingProxy : Freezable
        {
            protected override Freezable CreateInstanceCore() => new BindingProxy();

            public object Data
            {
                get => GetValue(DataProperty);
                set => SetValue(DataProperty, value);
            }

            public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
        }
        #endregion
    }
}
