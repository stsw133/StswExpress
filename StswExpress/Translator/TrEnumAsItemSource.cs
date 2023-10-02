using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

namespace StswExpress;

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
