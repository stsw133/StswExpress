using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class TrStringFormatMultiValuesConverter : MarkupExtension, IMultiValueConverter
{
    public TrStringFormatMultiValuesConverter()
    {
        WeakEventManager<TM, TMLanguageChangedEventArgs>.AddHandler(TM.Instance, nameof(TM.Instance.CurrentLanguageChanged), CurrentLanguageChanged);
    }
    public TrStringFormatMultiValuesConverter(string textId)
    {
        TextID = textId;
        WeakEventManager<TM, TMLanguageChangedEventArgs>.AddHandler(TM.Instance, nameof(TM.Instance.CurrentLanguageChanged), CurrentLanguageChanged);
    }

    /// <summary>
    /// Text to return if no text correspond to textId in the current language.
    /// </summary>
    public string? DefaultText { get; set; } = null;

    /// <summary>
    /// Language ID in which to get the translation. To Specify if not CurrentLanguage.
    /// </summary>
    public string? LanguageID { get; set; } = null;

    /// <summary>
    /// To force the use of a specific identifier.
    /// </summary>
    [ConstructorArgument("textID")]
    public string? TextID { get; set; } = null;

    /// <summary>
    /// Provides a prefix to add at the begining of the translated text.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Provides a suffix to add at the end of the translated text.
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => $"{Prefix}{string.Format(string.IsNullOrEmpty(TextID) ? "" : TM.Tr(TextID, DefaultText?.Replace("[apos]", "'"), LanguageID), values)}{Suffix}";
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();

    FrameworkElement? xamlTargetObject;
    DependencyProperty? xamlDependencyProperty;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        try
        {
            var xamlContext = serviceProvider.GetType()?.GetRuntimeFields()?.ToList()?.Find(f => f.Name.Equals("_xamlContext"))?.GetValue(serviceProvider);
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

    private void CurrentLanguageChanged(object? sender, TMLanguageChangedEventArgs e)
    {
        if (xamlTargetObject != null && xamlDependencyProperty != null)
            xamlTargetObject.GetBindingExpression(xamlDependencyProperty)?.UpdateTarget();
    }

    ~TrStringFormatMultiValuesConverter()
    {
        WeakEventManager<TM, TMLanguageChangedEventArgs>.RemoveHandler(TM.Instance, nameof(TM.Instance.CurrentLanguageChanged), CurrentLanguageChanged);
    }
}
