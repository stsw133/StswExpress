using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress.Translate
{
    /// <summary>
    /// Converter to Translate a the binding textId (In CurrentLanguage or if specified in LanguageId)
    /// If Translation don't exist return DefaultText
    /// Not usable in TwoWay Binding mode.
    /// </summary>
    public class TrTextIdConverter : MarkupExtension, IValueConverter
    {
        public TrTextIdConverter()
        {
            WeakEventManager<TM, TMLanguageChangedEventArgs>.AddHandler(TM.Instance, nameof(TM.Instance.CurrentLanguageChanged), CurrentLanguageChanged);
        }

        ~TrTextIdConverter()
        {
            WeakEventManager<TM, TMLanguageChangedEventArgs>.RemoveHandler(TM.Instance, nameof(TM.Instance.CurrentLanguageChanged), CurrentLanguageChanged);
        }

        /// <summary>
        /// The text to return if no text correspond to textId in the current language
        /// </summary>
        public string DefaultText { get; set; } = null;

        /// <summary>
        /// The language id in which to get the translation. To Specify if not CurrentLanguage
        /// </summary>
        public string LanguageId { get; set; } = null;

        /// <summary>
        /// A string format where will be injected the binding
        /// by Default => {0}
        /// </summary>
        public string TextIdStringFormat { get; set; } = "{0}";

        /// <summary>
        /// To provide a prefix to add at the begining of the translated text.
        /// </summary>
        public string Prefix { get; set; } = string.Empty;

        /// <summary>
        /// To provide a suffix to add at the end of the translated text.
        /// </summary>
        public string Suffix { get; set; } = string.Empty;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string textId = value?.ToString();
            return Prefix + (string.IsNullOrEmpty(textId) ? "" : TM.Tr(string.Format(TextIdStringFormat, textId), DefaultText?.Replace("[apos]", "'"), LanguageId)) + Suffix;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        FrameworkElement xamlTargetObject;
        DependencyProperty xamlDependencyProperty;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            try
            {
                var xamlContext = serviceProvider.GetType()
                    .GetRuntimeFields().ToList()
                    .Find(f => f.Name.Equals("_xamlContext"))
                    .GetValue(serviceProvider);

                xamlTargetObject = xamlContext?.GetType()
                    .GetProperty("GrandParentInstance")
                    .GetValue(xamlContext) as FrameworkElement;

                var xamlProperty = xamlContext?.GetType()
                    .GetProperty("GrandParentProperty")
                    .GetValue(xamlContext);

                xamlDependencyProperty = xamlProperty?.GetType()
                    .GetProperty("DependencyProperty")
                    .GetValue(xamlProperty) as DependencyProperty;
            }
            catch { }

            return this;
        }

        private void CurrentLanguageChanged(object sender, TMLanguageChangedEventArgs e)
        {
            if (xamlTargetObject != null && xamlDependencyProperty != null)
                xamlTargetObject.GetBindingExpression(xamlDependencyProperty)?.UpdateTarget();
        }
    }
}
