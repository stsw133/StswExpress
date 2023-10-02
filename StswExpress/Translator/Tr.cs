using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class Tr : MarkupExtension
{
    private DependencyObject? targetObject;
    private DependencyProperty? targetProperty;
    private string? defaultText = null;

    /// <summary>
    /// Translate the current property in the current language.
    /// Default TextID is "CurrentNamespace.CurrentClass.CurrentProperty".
    /// </summary>
    public Tr()
    {

    }

    /// <summary>
    /// Translate the current property in the current language.
    /// Default TextID is "CurrentNamespace.CurrentClass.CurrentProperty".
    /// </summary>
    /// <param name="textID">To force the use of a specific identifier</param>
    public Tr(string textID)
    {
        TextID = textID;
    }

    /// <summary>
    /// Translate in the current language the given textID.
    /// </summary>
    /// <param name="textID">To force the use of a specific identifier.</param>
    /// <param name="defaultText">The text to return if no text correspond to textID in the current language.</param>
    public Tr(string textID, string defaultText) : base()
    {
        TextID = textID;
        DefaultText = defaultText;
    }

    /// <summary>
    /// To force the use of a specific identifier.
    /// </summary>
    [ConstructorArgument("textID")]
    public virtual string? TextID { get; set; } = null;

    /// <summary>
    /// Text to return if no text correspond to textID in the current language.
    /// </summary>
    public string? DefaultText
    {
        get => defaultText;
        set => defaultText = value?.Replace("[apos]", "'");
    }

    /// <summary>
    /// Language ID in which to get the translation. To Specify if not CurrentLanguage.
    /// </summary>
    public string? LanguageID { get; set; } = null;

    /// <summary>
    /// If set to true, the text will automatically be updated when current language change (use Binding).
    /// If not, the property must be updated manually (use single string value).
    /// By default is set to true.
    /// </summary>
    public bool IsDynamic { get; set; } = true;

    /// <summary>
    /// Provides a prefix to add at the begining of the translated text.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Provides a suffix to add at the end of the translated text.
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    /// <summary>
    /// Converter to apply on the translated text.
    /// </summary>
    public IValueConverter? Converter { get; set; } = null;

    /// <summary>
    /// The parameter to pass to the converter.
    /// </summary>
    public object? ConverterParameter { get; set; } = null;

    /// <summary>
    /// The culture to pass to the converter.
    /// </summary>
    public CultureInfo? ConverterCulture { get; set; } = null;

    /// <summary>
    /// Translation In Xaml.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget service)
            return this;

        targetProperty = service.TargetProperty as DependencyProperty;
        targetObject = service.TargetObject as DependencyObject;
        if (targetObject == null || targetProperty == null)
        {
            return this;
        }

        try
        {
            if (string.IsNullOrEmpty(TextID))
            {
                if (targetObject != null && targetProperty != null)
                {
                    string context = targetObject.GetContextByName();
                    string obj = targetObject.FormatForTextId();
                    string property = targetProperty.ToString();

                    TextID = $"{context}.{obj}.{property}";
                }
                else if (!string.IsNullOrEmpty(DefaultText))
                {
                    TextID = DefaultText;
                }
            }
        }
        catch (InvalidCastException)
        {
            /// for Xaml Design Time
            TextID = Guid.NewGuid().ToString();
        }

        if (IsDynamic)
        {
            var binding = new Binding("TranslatedText")
            {
                Source = new TrData()
                {
                    TextID = TextID,
                    DefaultText = DefaultText,
                    LanguageID = LanguageID,
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
            object result = Prefix + TM.Tr(TextID, DefaultText, LanguageID) + Suffix;

            if (Converter != null)
                result = Converter.Convert(result, targetProperty.PropertyType, ConverterParameter, ConverterCulture);

            return result;
        }
    }
}
