using System;
using System.Globalization;
using System.Linq;
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
            return this;

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
            object result = Prefix + Translator.Tr(TextID, DefaultText, LanguageID) + Suffix;

            if (Converter != null)
                result = Converter.Convert(result, targetProperty.PropertyType, ConverterParameter, ConverterCulture);

            return result;
        }
    }
}

/// <summary>
/// To use an enum as ItemSource for ComboBox, ListBox... and translate the displayedText.
/// Manage the language changes.
/// </summary>
public class TrEnumAsItemSource : MarkupExtension
{
    private DependencyObject? targetObject;
    private DependencyProperty? targetProperty;

    public TrEnumAsItemSource()
    {

    }

    /// <summary>
    /// Type of enum to convert to a translated itemSource.
    /// </summary>
    [ConstructorArgument("enumType")]
    public Type? EnumType { get; set; }

    /// <summary>
    /// Specify a string format from the enum value to calculate the TextID for the translation.
    /// By default "EnumType{0}".
    /// </summary>
    public string? TextIdStringFormat { get; set; } = null;

    /// <summary>
    /// Provides a prefix to add at the begining of the translated text.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Provides a suffix to add at the end of the translated text.
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    /// <summary>
    /// 
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
            return this;

        try
        {
            targetObject.GetType().GetProperty("SelectedValuePath")?.SetValue(targetObject, "Data");
            targetObject.GetType().GetProperty("DisplayMemberPath")?.SetValue(targetObject, "TranslatedText");
        }
        catch { }

        return Enum.GetValues(EnumType)
            .Cast<object>()
            .ToList()
            .ConvertAll(e => new TrData()
            {
                Data = e,
                TextID = string.Format(TextIdStringFormat ?? EnumType.Name + "{0}", e.ToString()),
                DefaultText = e.ToString(),
                Prefix = Prefix,
                Suffix = Suffix,
            });
    }
}

/// <summary>
/// This class is used as ViewModel to bind to DependencyProperties.
/// Is used by Tr MarkupExtension to dynamically update the translation when current language changed.
/// </summary>
public class TrData : StswObservableObject
{
    public TrData()
    {
        WeakEventManager<Translator, TranslatorLanguageChangedEventArgs>.AddHandler(Translator.Instance, nameof(CurrentLanguageChanged), CurrentLanguageChanged);
    }

    /// <summary>
    /// To force the use of a specific identifier.
    /// </summary>
    public string? TextID { get; set; }

    /// <summary>
    /// Text to return if no text correspond to textID in the current language.
    /// </summary>
    public string? DefaultText { get; set; }

    /// <summary>
    /// Language ID in which to get the translation. To Specify if not CurrentLanguage.
    /// </summary>
    public string? LanguageID { get; set; }

    /// <summary>
    /// Provides a prefix to add at the begining of the translated text.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Provides a suffix to add at the end of the translated text.
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    /// <summary>
    /// An optional object use as data that is represented by this translation.
    /// (Example used for Enum values translation).
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Updates the binding when the current language changed (calls OnPropertyChanged).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CurrentLanguageChanged(object? sender, TranslatorLanguageChangedEventArgs e) => NotifyPropertyChanged(nameof(TranslatedText));

    /// <summary>
    /// 
    /// </summary>
    public string TranslatedText => $"{Prefix}{Translator.Tr(TextID, DefaultText, LanguageID)}{Suffix}";

    ~TrData()
    {
        WeakEventManager<Translator, TranslatorLanguageChangedEventArgs>.RemoveHandler(Translator.Instance, nameof(CurrentLanguageChanged), CurrentLanguageChanged);
    }
}
