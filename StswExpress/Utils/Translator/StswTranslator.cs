using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StswExpress;

/// <summary>
/// Provides static methods and properties to handle translations in the application.
/// </summary>
public static class StswTranslator
{
    private static Dictionary<string, Dictionary<string, string>> _translations = [];
    private static string? _currentLanguage;

    /// <summary>
    /// Occurs when a property of the TranslationManager changes (e.g., CurrentLanguage).
    /// Used to notify the UI that translations need to be refreshed.
    /// </summary>
    public static event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Loads translations from a JSON file.
    /// The JSON should be in the format:
    /// {
    ///   "Key1": { "en": "Value1", "pl": "Wartość1", ... },
    ///   "Key2": { "en": "Value2", "pl": "Wartość2", ... }
    /// }
    /// </summary>
    /// <param name="filePath">Path to the JSON file.</param>
    public static void LoadTranslationsFromJsonFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        LoadTranslationsFromJsonString(File.ReadAllText(filePath));
    }

    /// <summary>
    /// Loads translations from a JSON string.
    /// The JSON should be in the format:
    /// {
    ///   "Key1": { "en": "Value1", "pl": "Wartość1", ... },
    ///   "Key2": { "en": "Value2", "pl": "Wartość2", ... }
    /// }
    /// </summary>
    /// <param name="json">A valid JSON string containing translations.</param>
    public static void LoadTranslationsFromJsonString(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return;

        try
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
            if (data != null)
                foreach (var kvp in data)
                    foreach (var langPair in kvp.Value)
                        AddOrUpdateTranslation(kvp.Key, langPair.Key, langPair.Value);
        }
        catch
        {
            
        }
    }

    /// <summary>
    /// Adds or updates a single translation entry for a given key and language.
    /// Example usage: AddOrUpdateTranslation("Config.Confirmation", "en", "Confirmation");
    /// </summary>
    /// <param name="key">Unique translation key.</param>
    /// <param name="language">Language code (e.g., "en", "pl").</param>
    /// <param name="value">Translated string value.</param>
    public static void AddOrUpdateTranslation(string key, string language, string value)
    {
        if (!_translations.ContainsKey(key))
            _translations[key] = [];

        _translations[key][language] = value;
    }

    /// <summary>
    /// Exports the current translations to a JSON file.
    /// The structure is the same as the one used for loading translations:
    /// {
    ///   "Key1": { "en": "Value1", "pl": "Wartość1", ... },
    ///   "Key2": { "en": "Value2", "pl": "Wartość2", ... }
    /// }
    /// </summary>
    /// <param name="filePath">Path to the JSON file for export.</param>
    public static void ExportTranslationsToJson(string filePath)
    {
        try
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var options = jsonSerializerOptions;
            var json = JsonSerializer.Serialize(_translations, options);

            File.WriteAllText(filePath, json);
        }
        catch
        {
            
        }
    }

    /// <summary>
    /// Returns a list of available languages (collected from the translation dictionary).
    /// Includes an empty value for auto language detection (system default).
    /// </summary>
    public static List<string> Languages
    {
        get
        {
            var result = _translations.Values
                                      .SelectMany(x => x.Keys)
                                      .Distinct()
                                      .OrderBy(x => x)
                                      .ToList();

            if (!result.Contains(string.Empty))
                result.Insert(0, string.Empty);

            return result;
        }
    }

    /// <summary>
    /// Gets or sets the current language used for translations.
    /// If this is empty, the system language is used.
    /// </summary>
    public static string CurrentLanguage
    {
        get
        {
            if (string.IsNullOrEmpty(_currentLanguage))
                _currentLanguage = !string.IsNullOrEmpty(StswSettings.Default.Language) ? StswSettings.Default.Language : string.Empty;

            return _currentLanguage;
        }
        set
        {
            if (_currentLanguage != value)
            {
                _currentLanguage = value;

                StswSettings.Default.Language = value;
                StswSettings.Default.Save();

                OnPropertyChanged(nameof(CurrentLanguage));
            }
        }
    }

    /// <summary>
    /// Returns the translated value for the given key, according to the currently selected language.
    /// If the key or the language is missing, returns the defaultValue.
    /// </summary>
    /// <param name="key">Translation key.</param>
    /// <param name="defaultValue">Default value if translation is missing.</param>
    /// <param name="prefix">Optional prefix to be added to the translated value.</param>
    /// <param name="suffix">Optional suffix to be added to the translated value.</param>
    /// <returns>Translated string with optional prefix and suffix.</returns>
    public static string GetTranslation(string key, string? defaultValue = null, string? prefix = "", string? suffix = "")
    {
        var languageToUse = string.IsNullOrEmpty(CurrentLanguage)
            ? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            : CurrentLanguage;

        if (_translations.TryGetValue(key, out var langDict))
            if (langDict.TryGetValue(languageToUse, out var translation))
                return prefix + translation + suffix;

        return prefix + defaultValue ?? key + suffix;
    }

    private static void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    }
}
