using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// A MarkupExtension that enables binding to translation strings in XAML.
/// Usage example:
/// <TextBlock Text="{local:Translate Key=Config.Confirmation, DefaultValue=Confirmation, Suffix=':'}" />
/// </summary>
[MarkupExtensionReturnType(typeof(string))]
public class StswTranslateExtension : MarkupExtension, INotifyPropertyChanged
{
    public StswTranslateExtension(string key)
    {
        Key = key;
    }

    /// <summary>
    /// The key (ID) of the translation to retrieve.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The default value if the key or language is missing.
    /// </summary>
    public string DefaultValue { get; set; } = string.Empty;

    /// <summary>
    /// Optional prefix to be added to the returned translation string.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Optional suffix to be added to the returned translation string.
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    /// <summary>
    /// Occurs when the translation changes (e.g., language switch).
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// The text to display for the UI element. It is bound in XAML via the Binding returned by ProvideValue.
    /// </summary>
    public string TranslatedText => StswTranslator.GetTranslation(Key, DefaultValue, Prefix, Suffix);

    /// <summary>
    /// Returns the translation string for the specified key, prefix, suffix, etc.
    /// Automatically updates when the current language changes.
    /// </summary>
    /// <param name="serviceProvider">Service provider from WPF.</param>
    /// <returns>Translation string or default value.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            return $"{Prefix}{Key}{Suffix}";

        StswTranslator.PropertyChanged -= TranslationManager_PropertyChanged;
        StswTranslator.PropertyChanged += TranslationManager_PropertyChanged;

        var binding = new Binding(nameof(TranslatedText))
        {
            Source = this,
            Mode = BindingMode.OneWay
        };

        return binding.ProvideValue(serviceProvider);
    }

    private void TranslationManager_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(StswTranslator.CurrentLanguage))
            OnPropertyChanged(nameof(TranslatedText));
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
