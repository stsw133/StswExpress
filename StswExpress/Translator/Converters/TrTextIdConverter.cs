using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Converter to translate the binding TextID using the current language or a specified LanguageID.
/// If the translation does not exist, it returns the DefaultText. This converter is not usable in TwoWay binding mode.
/// </summary>
public class TrTextIdConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TrTextIdConverter"/> class.
    /// </summary>
    public TrTextIdConverter()
    {
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
    /// Gets or sets the string format where the binding value will be injected. Default is "{0}".
    /// </summary>
    public string TextIdStringFormat { get; set; } = "{0}";

    /// <summary>
    /// Gets or sets a prefix to add at the beginning of the translated text.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a suffix to add at the end of the translated text.
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    /// <summary>
    /// Converts the specified TextID into the translated text using the current language or a specified LanguageID with optional prefix and suffix.
    /// </summary>
    /// <param name="value">The value to be used as the TextID for translation.</param>
    /// <param name="targetType">The target type of the binding.</param>
    /// <param name="parameter">An optional parameter (not used).</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <returns>The translated text with prefix and suffix.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var textID = value?.ToString();
        return $"{Prefix}{(string.IsNullOrEmpty(textID) ? "" : StswTranslator.Tr(string.Format(TextIdStringFormat, textID), DefaultText?.Replace("[apos]", "'"), LanguageID))}{Suffix}";
    }

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
    /// <returns>The <see cref="TrTextIdConverter"/> instance.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        try
        {
            var xamlContext = serviceProvider.GetType()?.GetRuntimeFields()?.ToList()?.Find(f => f.Name.Equals("_xamlContext"))?.GetValue(serviceProvider);
            xamlTargetObject = xamlContext?.GetType()?.GetProperty("GrandParentInstance")?.GetValue(xamlContext) as FrameworkElement;
            var xamlProperty = xamlContext?.GetType()?.GetProperty("GrandParentProperty")?.GetValue(xamlContext);
            xamlDependencyProperty = xamlProperty?.GetType()?.GetProperty("DependencyProperty")?.GetValue(xamlProperty) as DependencyProperty;
        }
        catch { }

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

    ~TrTextIdConverter()
    {
        WeakEventManager<StswTranslator, TranslatorLanguageChangedEventArgs>.RemoveHandler(StswTranslator.Instance, nameof(StswTranslator.CurrentLanguageChanged), CurrentLanguageChanged);
    }
}
