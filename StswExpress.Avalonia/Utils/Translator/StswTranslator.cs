using Avalonia;
using Avalonia.Threading;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace StswExpress.Avalonia;

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
    private static bool _languageSyncInProgress;

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
                var savedLanguage = StswApp.Settings.Language;
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

                if (!_languageSyncInProgress)
                {
                    try
                    {
                        _languageSyncInProgress = true;
                        StswApp.Settings.SyncLanguageFromTranslator(_currentLanguage);
                    }
                    finally { _languageSyncInProgress = false; }
                }

                Task.Run(async () =>
                {
                    await LoadTranslationsForCurrentLanguageAsync();
                    Dispatcher.UIThread.Post(() => OnPropertyChanged(nameof(CurrentLanguage)));
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
    /// Synchronizes the current language setting with the specified language value from the settings.
    /// </summary>
    /// <param name="language">The language code to synchronize with, or null to clear the current language setting.</param>
    internal static void SyncLanguageFromSettings(string? language)
    {
        if (_languageSyncInProgress)
            return;

        try
        {
            _languageSyncInProgress = true;
            CurrentLanguage = language ?? string.Empty;
        }
        finally { _languageSyncInProgress = false; }
    }

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
                var data = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(new MemoryStream(Encoding.UTF8.GetBytes(json)));
                if (data != null)
                    foreach (var kvp in data)
                        AddOrUpdateTranslation(kvp.Key, language, kvp.Value);
            }
            else
            {
                var data = await JsonSerializer.DeserializeAsync<Dictionary<string, Dictionary<string, string>>>(new MemoryStream(Encoding.UTF8.GetBytes(json)));
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
    /// <param name="translation">Translated string value.</param>
    public static void AddOrUpdateTranslation(string key, string language, string translation)
    {
        var languageDict = _translations.GetOrAdd(key, _ => new ConcurrentDictionary<string, string>());
        languageDict[language] = translation;
    }

    /// <summary>
    /// Adds or updates multiple translations from a nested dictionary.
    /// </summary>
    /// <param name="translations">Dictionary containing translation keys and their corresponding language-value pairs.</param>
    public static void AddOrUpdateTranslations(Dictionary<string, Dictionary<string, string>> translations)
    {
        foreach (var kvp in translations)
            foreach (var langPair in kvp.Value)
                AddOrUpdateTranslation(kvp.Key, langPair.Key, langPair.Value);
    }

    /// <summary>
    /// Clears translations for a specific language.
    /// </summary>
    /// <param name="language">Language code to clear translations for.</param>
    private static void ClearTranslationsForLanguage(string language)
    {
        foreach (var key in _translations.Keys)
            if (_translations.TryGetValue(key, out var languageDict))
                languageDict.TryRemove(language, out _);
    }

    /// <summary>
    /// Clears all translations.
    /// </summary>
    public static void ClearTranslations()
    {
        _translations.Clear();
    }

    /// <summary>
    /// Event triggered before loading default translations, allowing custom translations to be added.
    /// </summary>
    public static event Func<string, Task<string?>>? CustomTranslationLoader;

    /// <summary>
    /// Imports translations from a JSON string.
    /// </summary>
    /// <param name="json">A valid JSON string containing translations.</param>
    public static void ImportTranslationsFromJson(string json)
    {
        try
        {
            var deserializedData = JsonSerializer.Deserialize<ConcurrentDictionary<string, ConcurrentDictionary<string, string>>>(json);
            if (deserializedData != null)
                _translations = deserializedData;
        }
        catch
        {
            // Handle deserialization errors as needed.
        }
    }

    /// <summary>
    /// Asynchronously imports translations from a JSON file.
    /// </summary>
    /// <param name="filePath">Path to the JSON file.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ImportTranslationsFromJsonFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        using var stream = File.OpenRead(filePath);
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        ImportTranslationsFromJson(json);
    }

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
    /// 
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="language"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void LoadEmbeddedTranslations(Assembly assembly, string? language = null)
    {
        var languageToUse = language ?? (!string.IsNullOrEmpty(CurrentLanguage) ? CurrentLanguage : "en");

        var resourceNames = assembly.GetManifestResourceNames()
            .Where(r => r.EndsWith($".{languageToUse}.json"));

        foreach (var resourceName in resourceNames)
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream ?? throw new InvalidOperationException("Resource stream is null.")))
            {
                var json = reader.ReadToEnd();
                LoadTranslationsFromJsonString(json, languageToUse);
            }
    }

    /// <summary>
    /// Loads translations from an embedded resource JSON file.
    /// </summary>
    /// <param name="resourceName">Name of the embedded resource.</param>
    /// <param name="assembly">Assembly containing the embedded resource.</param>
    /// <param name="language">Optional language to load directly as a flat dicitonary.</param>
    /// <exception cref="InvalidOperationException">Thrown if the resource stream is <see langword="null"/>.</exception>
    public static void LoadEmbeddedTranslations(string resourceName, Assembly assembly, string? language = null)
    {
        var languageToUse = language ?? (!string.IsNullOrEmpty(CurrentLanguage) ? CurrentLanguage : "en");

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream ?? throw new InvalidOperationException("Resource stream is null."));
        var json = reader.ReadToEnd();
        LoadTranslationsFromJsonString(json, languageToUse);
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
