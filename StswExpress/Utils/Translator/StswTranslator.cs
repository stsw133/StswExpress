using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace StswExpress;

/// <summary>
/// Provides static methods and properties to handle translations in the application.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// StswTranslator.CurrentLanguage = "pl";
/// 
/// StswTranslator.AddOrUpdateTranslation("Hello", "en", "Hello");
/// StswTranslator.AddOrUpdateTranslation("Hello", "pl", "Cześć");
/// var translatedText = StswTranslator.GetTranslation("Hello", prefix: "[", suffix: "]");
/// 
/// StswTranslator.CustomTranslationLoader += async(language) =>
/// {
///     var customFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CustomTranslations", $"{language}.json");
/// 
///     if (File.Exists(customFilePath))
///         return await File.ReadAllTextAsync(customFilePath);
/// 
///     return null;
/// };
/// </code>
/// </example>
public static class StswTranslator
{
    private static ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _translations = [];

    /// <summary>
    /// Gets the list of available languages.
    /// </summary>
    public static Dictionary<string, string> AvailableLanguages { get; set; } = new()
    {
        { "de", "Deutsch" },
        { "en", "English" },
        { "es", "Español" },
        { "fr", "Français" },
        { "ja", "日本語" },
        { "ko", "한국어" },
        { "pl", "Polski" },
        { "ru", "Русский" },
        { "zh-cn", "中文" }
    };

    /// <summary>
    /// Gets or sets the current language used for translations.
    /// If this is empty, the system language is used.
    /// </summary>
    public static string CurrentLanguage
    {
        get
        {
            if (string.IsNullOrEmpty(_currentLanguage))
            {
                var savedLanguage = StswSettings.Default.Language;
                if (string.IsNullOrEmpty(savedLanguage))
                {
                    var systemLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                    _currentLanguage = AvailableLanguages.ContainsKey(systemLanguage) ? systemLanguage : "en";
                }
                else
                {
                    _currentLanguage = savedLanguage;
                }
            }

            return _currentLanguage;
        }
        set
        {
            if (_currentLanguage != value)
            {
                ClearTranslationsForLanguage(_currentLanguage ?? "en");
                _currentLanguage = string.IsNullOrEmpty(value) ? null : value;

                Task.Run(async () =>
                {
                    await LoadTranslationsForCurrentLanguageAsync();
                    Application.Current.Dispatcher.Invoke(() => OnPropertyChanged(nameof(CurrentLanguage)));
                });
            }
        }
    }
    private static string? _currentLanguage;

    /// <<summary>
    /// Occurs when a property of the TranslationManager changes (e.g., CurrentLanguage).
    /// Used to notify the UI that translations need to be refreshed.
    /// </summary>>
    public static event PropertyChangedEventHandler? PropertyChanged;
    private static void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Loads translations from a JSON file.
    /// The JSON should be in the format:
    /// {
    ///   "Key1": { "en": "Value1", "pl": "Wartość1", ... },
    ///   "Key2": { "en": "Value2", "pl": "Wartość2", ... }
    /// }
    /// </summary>
    /// <param name="filePath">Path to the JSON file.</param>
    /// <param name="language">Optional language to load directly as a flat dicitonary.</param>
    public static void LoadTranslationsFromJsonFile(string filePath, string? language = null)
    {
        if (!File.Exists(filePath))
            return;

        var json = File.ReadAllText(filePath);
        LoadTranslationsFromJsonString(json, language);
    }

    /// <summary>
    /// Asynchronously loads translations from a JSON file.
    /// </summary>
    /// <param name="filePath">Path to the JSON file.</param>
    /// <param name="language">Optional language to load directly as a flat dicitonary.</param>
    public static async Task LoadTranslationsFromJsonFileAsync(string filePath, string? language = null)
    {
        if (!File.Exists(filePath))
            return;

        using var stream = File.OpenRead(filePath);
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        await LoadTranslationsFromJsonStringAsync(json, language);
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
    /// <param name="language">Optional language to load directly as a flat dicitonary.</param>
    public static void LoadTranslationsFromJsonString(string json, string? language = null)
    {
        if (string.IsNullOrWhiteSpace(json))
            return;

        try
        {
            if (!string.IsNullOrEmpty(language))
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (data != null)
                    foreach (var kvp in data)
                        AddOrUpdateTranslation(kvp.Key, language, kvp.Value);
            }
            else
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
                if (data != null)
                    foreach (var kvp in data)
                        foreach (var langPair in kvp.Value)
                            AddOrUpdateTranslation(kvp.Key, langPair.Key, langPair.Value);
            }
        }
        catch
        {
            // Handle deserialization errors as needed.
        }
    }

    /// <summary>
    /// Asynchronously loads translations from a JSON string.
    /// </summary>
    /// <param name="json">A valid JSON string containing translations.</param>
    /// <param name="language">Optional language to load directly as a flat dicitonary.</param>
    public static async Task LoadTranslationsFromJsonStringAsync(string json, string? language = null)
    {
        if (string.IsNullOrWhiteSpace(json))
            return;

        try
        {
            if (!string.IsNullOrEmpty(language))
            {
                var data = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));
                if (data != null)
                    foreach (var kvp in data)
                        AddOrUpdateTranslation(kvp.Key, language, kvp.Value);
            }
            else
            {
                var data = await JsonSerializer.DeserializeAsync<Dictionary<string, Dictionary<string, string>>>(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));
                if (data != null)
                    foreach (var kvp in data)
                        foreach (var langPair in kvp.Value)
                            AddOrUpdateTranslation(kvp.Key, langPair.Key, langPair.Value);
            }
        }
        catch
        {
            // Handle deserialization errors as needed.
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
        var langDict = _translations.GetOrAdd(key, _ => new());
        langDict[language] = value;
    }

    /// <summary>
    /// Clears translations for a specific language.
    /// </summary>
    /// <param name="language">Language code to clear translations for.</param>
    private static void ClearTranslationsForLanguage(string language)
    {
        var keysToRemove = _translations.Keys
            .Where(k => k.StartsWith("Stsw") && _translations[k].ContainsKey(language))
            .ToList();

        foreach (var key in keysToRemove)
        {
            if (_translations.TryGetValue(key, out var langDict))
            {
                langDict.TryRemove(language, out _);

                if (langDict.IsEmpty)
                    _translations.TryRemove(key, out _);
            }
        }
    }

    /// <summary>
    /// Event triggered before loading default translations, allowing custom translations to be added.
    /// </summary>
    public static event Func<string, Task<string?>>? CustomTranslationLoader;

    /// <summary>
    /// Loads translations for the current language asynchronously.
    /// </summary>
    internal static async Task LoadTranslationsForCurrentLanguageAsync()
    {
        var language = string.IsNullOrEmpty(CurrentLanguage) ? "en" : CurrentLanguage;
        var resourcePath = $"Utils/Translator/Translations/{language}.json";

        var json = StswFnUI.GetResourceAsText(Assembly.GetExecutingAssembly().FullName!, resourcePath);
        if (json == null)
            return;

        await LoadTranslationsFromJsonStringAsync(json, language);

        if (CustomTranslationLoader != null)
        {
            foreach (var handler in CustomTranslationLoader.GetInvocationList().Cast<Func<string, Task<string?>>>())
            {
                var customJson = await handler.Invoke(language);
                if (!string.IsNullOrEmpty(customJson))
                    await LoadTranslationsFromJsonStringAsync(customJson, language);
            }
        }
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
            // Handle deserialization errors as needed.
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
    public static string GetTranslation(string key, string? defaultValue = null, string? language = null, string? prefix = null, string? suffix = null)
    {
        var languageToUse = language ?? (!string.IsNullOrEmpty(CurrentLanguage) ? CurrentLanguage : "en");

        if (_translations.TryGetValue(key, out var langDict))
            if (langDict.TryGetValue(languageToUse, out var translation))
                return $"{prefix}{translation}{suffix}";

        return $"{prefix}{defaultValue ?? key}{suffix}";
    }
}
