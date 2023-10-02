using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Converter to translate a the binding textID (in CurrentLanguage or if specified in LanguageID).
/// If translation doesn't exist return DefaultText.
/// Not usable in TwoWay binding mode.
/// </summary>
public class TrTextIdConverter : MarkupExtension, IValueConverter
{
    public TrTextIdConverter()
    {
        WeakEventManager<TM, TMLanguageChangedEventArgs>.AddHandler(TM.Instance, nameof(TM.Instance.CurrentLanguageChanged), CurrentLanguageChanged);
    }

    /// <summary>
    /// Text to return if no text correspond to textID in the current language.
    /// </summary>
    public string? DefaultText { get; set; } = null;

    /// <summary>
    /// Language ID in which to get the translation. To Specify if not CurrentLanguage.
    /// </summary>
    public string? LanguageID { get; set; } = null;

    /// <summary>
    /// A string format where will be injected the binding.
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
        var textID = value?.ToString();
        return $"{Prefix}{(string.IsNullOrEmpty(textID) ? "" : TM.Tr(string.Format(TextIdStringFormat, textID), DefaultText?.Replace("[apos]", "'"), LanguageID))}{Suffix}";
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

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
        }
        catch { }

        return this;
    }

    private void CurrentLanguageChanged(object? sender, TMLanguageChangedEventArgs e)
    {
        if (xamlTargetObject != null && xamlDependencyProperty != null)
            xamlTargetObject.GetBindingExpression(xamlDependencyProperty)?.UpdateTarget();
    }

    ~TrTextIdConverter()
    {
        WeakEventManager<TM, TMLanguageChangedEventArgs>.RemoveHandler(TM.Instance, nameof(TM.Instance.CurrentLanguageChanged), CurrentLanguageChanged);
    }
}
