using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Converter that formats a translated text using a specific TextID and optional string format parameters.
/// Updates automatically when the current language changes.
/// </summary>
public class TrStringFormatConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TrStringFormatConverter"/> class.
    /// </summary>
    public TrStringFormatConverter()
    {
        WeakEventManager<StswTranslator, TranslatorLanguageChangedEventArgs>.AddHandler(StswTranslator.Instance, nameof(StswTranslator.CurrentLanguageChanged), CurrentLanguageChanged);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TrStringFormatConverter"/> class with a specified text ID.
    /// </summary>
    /// <param name="textId">The text identifier to use for translation.</param>
    public TrStringFormatConverter(string textId)
    {
        TextID = textId;
        WeakEventManager<StswTranslator, TranslatorLanguageChangedEventArgs>.AddHandler(StswTranslator.Instance, nameof(StswTranslator.CurrentLanguageChanged), CurrentLanguageChanged);
    }

    /// <summary>
    /// Gets or sets the text to return if no translation corresponds to the TextID in the current language.
    /// </summary>
    public string? DefaultText { get; set; } = null;

    /// <summary>
    /// Gets or sets the language ID in which to get the translation. Specify if not CurrentLanguage.
    /// </summary>
    public string? LanguageID { get; set; } = null;

    /// <summary>
    /// Gets or sets a prefix to add at the beginning of the translated text.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a suffix to add at the end of the translated text.
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the text identifier to use for translation.
    /// </summary>
    [ConstructorArgument("textID")]
    public string? TextID { get; set; } = null;

    /// <summary>
    /// Converts the specified TextID and LanguageID into the translated text using string format with optional prefix and suffix.
    /// </summary>
    /// <param name="value">The value to be formatted into the translation.</param>
    /// <param name="targetType">The target type of the binding.</param>
    /// <param name="parameter">An optional parameter (not used).</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <returns>The formatted translated text with prefix and suffix.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        $"{Prefix}{string.Format(string.IsNullOrEmpty(TextID) ? "" : StswTranslator.Tr(TextID, DefaultText?.Replace("[apos]", "'"), LanguageID), value)}{Suffix}";

    /// <summary>
    /// Throws a NotImplementedException as this converter does not support TwoWay binding.
    /// </summary>
    /// <param name="value">The value produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">An optional parameter.</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <returns>Throws NotImplementedException in all cases.</returns>
    /// <exception cref="NotImplementedException">Always thrown as this method is not implemented.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

    FrameworkElement? xamlTargetObject;
    DependencyProperty? xamlDependencyProperty;

    /// <summary>
    /// Provides the translated text as a value in XAML.
    /// </summary>
    /// <param name="serviceProvider">The service provider that provides access to the target object and property.</param>
    /// <returns>The <see cref="TrStringFormatConverter"/> instance.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        try
        {
            var xamlContext = serviceProvider.GetType()?.GetRuntimeFields().ToList()?.Find(f => f.Name.Equals("_xamlContext"))?.GetValue(serviceProvider);
            xamlTargetObject = xamlContext?.GetType()?.GetProperty("GrandParentInstance")?.GetValue(xamlContext) as FrameworkElement;
            var xamlProperty = xamlContext?.GetType()?.GetProperty("GrandParentProperty")?.GetValue(xamlContext);
            xamlDependencyProperty = xamlProperty?.GetType()?.GetProperty("DependencyProperty")?.GetValue(xamlProperty) as DependencyProperty;

            if (string.IsNullOrEmpty(TextID))
            {
                if (xamlTargetObject != null && xamlDependencyProperty != null)
                {
                    string context = xamlTargetObject.GetContextByName();
                    string obj = xamlTargetObject.FormatForTextId();
                    string property = xamlDependencyProperty.ToString();

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

        return this;
    }

    /// <summary>
    /// Updates the target binding when the current language changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments containing information about the language change.</param>
    private void CurrentLanguageChanged(object? sender, TranslatorLanguageChangedEventArgs e)
    {
        if (xamlTargetObject != null && xamlDependencyProperty != null)
            xamlTargetObject.GetBindingExpression(xamlDependencyProperty)?.UpdateTarget();
    }

    ~TrStringFormatConverter()
    {
        WeakEventManager<StswTranslator, TranslatorLanguageChangedEventArgs>.RemoveHandler(StswTranslator.Instance, nameof(StswTranslator.CurrentLanguageChanged), CurrentLanguageChanged);
    }
}
