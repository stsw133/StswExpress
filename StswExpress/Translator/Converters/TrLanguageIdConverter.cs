using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Converter that translates a specific TextID based on the binding LanguageID.
/// If the translation does not exist, it returns the DefaultText.
/// This converter is not usable in TwoWay binding mode.
/// </summary>
public class TrLanguageIdConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets or sets the text identifier to use for translation.
    /// </summary>
    [ConstructorArgument("textID")]
    public virtual string? TextID { get; set; } = null;

    /// <summary>
    /// Gets or sets the text to return if no translation corresponds to the TextID in the current language.
    /// </summary>
    public string? DefaultText { get; set; } = null;

    /// <summary>
    /// Gets or sets a prefix to add at the beginning of the translated text.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a suffix to add at the end of the translated text.
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    /// <summary>
    /// Converts the specified TextID and LanguageID into the translated text with optional prefix and suffix.
    /// </summary>
    /// <param name="value">The value to be used as the LanguageID for translation.</param>
    /// <param name="targetType">The target type of the binding.</param>
    /// <param name="parameter">An optional parameter (not used).</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <returns>The translated text with prefix and suffix.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"{Prefix}{StswTranslator.Tr(TextID, DefaultText?.Replace("[apos]", "'"), value as string)}{Suffix}";

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
    /// <returns>The <see cref="TrLanguageIdConverter"/> instance.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        try
        {
            var xamlContext = serviceProvider.GetType()?.GetRuntimeFields()?.ToList()?.Find(f => f.Name.Equals("_xamlContext"))?.GetValue(serviceProvider);
            xamlTargetObject = xamlContext?.GetPropertyValue("GrandParentInstance") as FrameworkElement;
            var xamlProperty = xamlContext?.GetPropertyValue("GrandParentProperty");
            xamlDependencyProperty = xamlProperty?.GetPropertyValue("DependencyProperty") as DependencyProperty;

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
}
