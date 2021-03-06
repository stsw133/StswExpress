using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress.Translate
{
    /// <summary>
    /// Translate With TranslateMe
    /// </summary>
    public class Tr : MarkupExtension
    {
        private DependencyObject targetObject;
        private DependencyProperty targetProperty;
        private string defaultText = null;

        /// <summary>
        /// Translate the current Property in the current language
        /// The Default TextId is "CurrentNamespace.CurrentClass.CurrentProperty"
        /// </summary>
        public Tr()
        { }

        /// <summary>
        /// Translate the current Property in the current language
        /// The Default TextId is "CurrentNamespace.CurrentClass.CurrentProperty"
        /// </summary>
        /// <param name="textId">To force the use of a specific identifier</param>
        public Tr(string textId)
        {
            TextId = textId;
        }

        /// <summary>
        /// Translate Me : Translate in the current language the given textId
        /// </summary>
        /// <param name="textId">To force the use of a specific identifier</param>
        /// <param name="defaultText">The text to return if no text correspond to textId in the current language</param>
        public Tr(string textId, string defaultText) : base()
        {
            TextId = textId;
            DefaultText = defaultText;
        }

        /// <summary>
        /// To force the use of a specific identifier
        /// </summary>
        [ConstructorArgument("textId")]
        public virtual string TextId { get; set; } = null;

        /// <summary>
        /// The text to return if no text correspond to textId in the current language
        /// </summary>
        public string DefaultText
        {
            get => defaultText;
            set => defaultText = value?.Replace("[apos]", "'");
        }

        /// <summary>
        /// The language id in which to get the translation. To Specify if not CurrentLanguage
        /// </summary>
        public string LanguageId { get; set; } = null;

        /// <summary>
        /// If set to true, The text will automatically be update when Current Language Change. (use Binding)
        /// If not the property must be updated manually (use single string value).
        /// By default is set to true.
        /// </summary>
        public bool IsDynamic { get; set; } = true;

        /// <summary>
        /// To provide a prefix to add at the begining of the translated text.
        /// </summary>
        public string Prefix { get; set; } = string.Empty;

        /// <summary>
        /// To provide a suffix to add at the end of the translated text.
        /// </summary>
        public string Suffix { get; set; } = string.Empty;

        /// <summary>
        /// Converter to apply on the translated text
        /// </summary>
        public IValueConverter Converter { get; set; } = null;

        /// <summary>
        /// The parameter to pass to the converter
        /// </summary>
        public object ConverterParameter { get; set; } = null;

        /// <summary>
        /// The culture to pass to the converter
        /// </summary>
        public CultureInfo ConverterCulture { get; set; } = null;

        /// <summary>
        /// Translation In Xaml
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget service = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (service == null)
                return this;

            targetProperty = service.TargetProperty as DependencyProperty;
            targetObject = service.TargetObject as DependencyObject;
            if (targetObject == null || targetProperty == null)
                return this;

            try
            {
                if (string.IsNullOrEmpty(TextId))
                {
                    if (targetObject != null && targetProperty != null)
                    {
                        string context = targetObject.GetContextByName();
                        string obj = targetObject.FormatForTextId();
                        string property = targetProperty.ToString();

                        TextId = $"{context}.{obj}.{property}";
                    }
                    else if (!string.IsNullOrEmpty(DefaultText))
                    {
                        TextId = DefaultText;
                    }
                }
            }
            catch (InvalidCastException)
            {
                // For Xaml Design Time
                TextId = Guid.NewGuid().ToString();
            }

            if (IsDynamic)
            {
                Binding binding = new Binding("TranslatedText")
                {
                    Source = new TrData()
                    {
                        TextId = TextId,
                        DefaultText = DefaultText,
                        LanguageId = LanguageId,
                        Prefix = Prefix,
                        Suffix = Suffix
                    }
                };

                if (Converter != null)
                {
                    binding.Converter = Converter;
                    binding.ConverterParameter = ConverterParameter;
                    binding.ConverterCulture = ConverterCulture;
                }

                BindingOperations.SetBinding(targetObject, targetProperty, binding);

                return binding.ProvideValue(serviceProvider);
            }
            else
            {
                object result = Prefix + TM.Tr(TextId, DefaultText, LanguageId) + Suffix;

                if (Converter != null)
                    result = Converter.Convert(result, targetProperty.PropertyType, ConverterParameter, ConverterCulture);

                return result;
            }
        }
    }
}
