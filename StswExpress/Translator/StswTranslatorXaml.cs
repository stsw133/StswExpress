using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A markup extension used to translate text in XAML files.
/// </summary>
public class Tr : MarkupExtension
{
    private DependencyObject? targetObject;
    private DependencyProperty? targetProperty;
    private string? defaultText = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tr"/> class.
    /// </summary>
    public Tr()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tr"/> class with a specified text ID.
    /// </summary>
    /// <param name="textID">The text identifier to use for translation.</param>
    public Tr(string textID)
    {
        TextID = textID;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tr"/> class with a specified text ID and default text.
    /// </summary>
    /// <param name="textID">The text identifier to use for translation.</param>
    /// <param name="defaultText">The text to return if no translation corresponds to the text ID in the current language.</param>
    public Tr(string textID, string defaultText) : base()
    {
        TextID = textID;
        DefaultText = defaultText;
    }

    /// <summary>
    /// Gets or sets the text identifier to use for translation.
    /// </summary>
    [ConstructorArgument("textID")]
    public virtual string? TextID { get; set; } = null;

    /// <summary>
    /// Gets or sets the text to return if no translation corresponds to the text ID in the current language.
    /// </summary>
    public string? DefaultText
    {
        get => defaultText;
        set => defaultText = value?.Replace("[apos]", "'");
    }

    /// <summary>
    /// Gets or sets the language ID in which to get the translation. Specify if not CurrentLanguage.
    /// </summary>
    public string? LanguageID { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether the text will automatically be updated when the current language changes.
    /// By default, it is set to true.
    /// </summary>
    public bool IsDynamic { get; set; } = true;

    /// <summary>
    /// Gets or sets a prefix to add at the beginning of the translated text.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a suffix to add at the end of the translated text.
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the converter to apply to the translated text.
    /// </summary>
    public IValueConverter? Converter { get; set; } = null;

    /// <summary>
    /// Gets or sets the parameter to pass to the converter.
    /// </summary>
    public object? ConverterParameter { get; set; } = null;

    /// <summary>
    /// Gets or sets the culture to pass to the converter.
    /// </summary>
    public CultureInfo? ConverterCulture { get; set; } = null;

    /// <summary>
    /// Translates text in XAML by providing the translated value.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The translated text or binding object.</returns>
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
                Mode = BindingMode.OneWay,
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
            object result = Prefix + StswTranslator.Tr(TextID, DefaultText, LanguageID) + Suffix;

            if (Converter != null)
                result = Converter.Convert(result, targetProperty!.PropertyType, ConverterParameter, ConverterCulture);

            return result;
        }
    }
}

/// <summary>
/// A markup extension used to provide an item source for a ComboBox or ListBox by translating enum values.
/// Manages language changes for dynamic translation updates.
/// </summary>
public class TrEnumAsItemSource : MarkupExtension
{
    private DependencyObject? targetObject;
    private DependencyProperty? targetProperty;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrEnumAsItemSource"/> class.
    /// </summary>
    public TrEnumAsItemSource()
    {

    }

    /// <summary>
    /// Gets or sets the type of enum to convert to a translated item source.
    /// </summary>
    [ConstructorArgument("enumType")]
    public Type? EnumType { get; set; }

    /// <summary>
    /// Gets or sets the string format from the enum value to calculate the TextID for the translation.
    /// By default, it is "EnumType{0}".
    /// </summary>
    public string? TextIdStringFormat { get; set; } = null;

    /// <summary>
    /// Gets or sets a prefix to add at the beginning of the translated text.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a suffix to add at the end of the translated text.
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    /// <summary>
    /// Provides the translated item source for enum values.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>A collection of translated enum values.</returns>
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
/// Acts as a ViewModel to bind to DependencyProperties and update translations dynamically when the current language changes.
/// </summary>
public class TrData : StswObservableObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TrData"/> class and subscribes to language change events.
    /// </summary>
    public TrData()
    {
        WeakEventManager<StswTranslator, TranslatorLanguageChangedEventArgs>.AddHandler(StswTranslator.Instance, nameof(CurrentLanguageChanged), CurrentLanguageChanged);
    }

    /// <summary>
    /// Gets or sets the text identifier to use for translation.
    /// </summary>
    public string? TextID { get; set; }

    /// <summary>
    /// Gets or sets the text to return if no translation corresponds to the text ID in the current language.
    /// </summary>
    public string? DefaultText { get; set; }

    /// <summary>
    /// Gets or sets the language ID in which to get the translation. Specify if not CurrentLanguage.
    /// </summary>
    public string? LanguageID { get; set; }

    /// <summary>
    /// Gets or sets a prefix to add at the beginning of the translated text.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a suffix to add at the end of the translated text.
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional object used as data that is represented by this translation.
    /// For example, it can be used for enum values translation.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Updates the binding when the current language changes by calling OnPropertyChanged.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments containing information about the language change.</param>
    private void CurrentLanguageChanged(object? sender, TranslatorLanguageChangedEventArgs e) => OnPropertyChanged(nameof(TranslatedText));

    /// <summary>
    /// Gets the translated text, including any specified prefix and suffix.
    /// </summary>
    public string TranslatedText => $"{Prefix}{StswTranslator.Tr(TextID, DefaultText, LanguageID)}{Suffix}";

    ~TrData()
    {
        WeakEventManager<StswTranslator, TranslatorLanguageChangedEventArgs>.RemoveHandler(StswTranslator.Instance, nameof(CurrentLanguageChanged), CurrentLanguageChanged);
    }
}
