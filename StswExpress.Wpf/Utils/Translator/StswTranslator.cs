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

namespace StswExpress.Wpf;

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
    private static bool _languageSyncInProgress;
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _translations = [];
    private static readonly Dictionary<string, CustomLanguageRegistration> _customLanguages = new(StringComparer.OrdinalIgnoreCase);
    private static readonly string[] _builtInLanguageCodes = ["de", "en", "es", "fr", "ja", "ko", "pl", "ru", "zh-cn"];

    /// <summary>
    /// Gets the list of available languages.
    /// </summary>
    public static StswObservableDictionary<string, string> AvailableLanguages { get; set; } = [];

    static StswTranslator() => RebuildAvailableLanguages();

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
                    Application.Current.Dispatcher.Invoke(() => OnPropertyChanged(nameof(CurrentLanguage)));
                });
            }
        }
    }
    private static string? _currentLanguage;

    /// <summary>
    /// Registers a custom language source so that translations can be loaded from application resources.
    /// </summary>
    /// <param name="name">Language code (e.g. "pt").</param>
    /// <param name="source">URI to translation JSON resource (e.g. /MyApp;component/Translations/pt.json).</param>
    /// <param name="fallbackTranslation">Optional fallback language used when a key is missing in the custom language.</param>
    public static void RegisterCustomLanguage(string name, Uri source, string? fallbackTranslation = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var languageCode = name.Trim().ToLowerInvariant();
        _customLanguages[languageCode] = new CustomLanguageRegistration(source, fallbackTranslation?.Trim().ToLowerInvariant());

        RebuildAvailableLanguages();
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
    /// <param name="language">Optional language to use instead of the current one.</param>
    /// <param name="prefix">Optional prefix to be added to the translated value.</param>
    /// <param name="suffix">Optional suffix to be added to the translated value.</param>
    /// <returns>Translated string with optional prefix and suffix.</returns>
    public static string GetTranslation(string key, string? defaultValue = null, string? language = null, string? prefix = null, string? suffix = null)
    {
        var languageToUse = language ?? (!string.IsNullOrEmpty(CurrentLanguage) ? CurrentLanguage : "en");
        var translation = GetTranslationWithFallback(key, languageToUse, []);
        return $"{prefix}{translation ?? defaultValue ?? key}{suffix}";
    }

    /// <summary>
    /// Loads translations for the current language asynchronously.
    /// </summary>
    internal static async Task LoadTranslationsForCurrentLanguageAsync()
    {
        var language = string.IsNullOrEmpty(CurrentLanguage) ? "en" : CurrentLanguage;
        await LoadLanguageChainAsync(language, []);

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
    /// Event triggered before loading default translations, allowing custom translations to be added.
    /// </summary>
    public static event Func<string, Task<string?>>? CustomTranslationLoader;

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
    /// Loads translation resources for the specified language and its fallback chain asynchronously.
    /// </summary>
    /// <remarks>This method loads translations for the specified language and then recursively loads translations for any fallback languages defined in custom language registrations. It uses a set of visited languages to prevent infinite loops in case of circular fallback references.</remarks>
    /// <param name="language">The language identifier for which translation resources are to be loaded. Cannot be null or empty.</param>
    /// <param name="visitedLanguages">A set of language identifiers that have already been processed. Used to prevent loading the same language multiple times.</param>
    /// <returns>A task that represents the asynchronous operation of loading translation resources.</returns>
    private static async Task LoadLanguageChainAsync(string language, HashSet<string> visitedLanguages)
    {
        if (!visitedLanguages.Add(language))
            return;

        var resourcePath = $"Utils/Translator/Translations/{language}.json";
        var json = StswFnUI.GetResourceAsText(Assembly.GetExecutingAssembly().FullName!, resourcePath);
        if (json != null)
            await LoadTranslationsFromJsonStringAsync(json, language);

        if (_customLanguages.TryGetValue(language, out var registration))
        {
            var customJson = LoadJsonFromUri(registration.Source);
            if (!string.IsNullOrWhiteSpace(customJson))
                await LoadTranslationsFromJsonStringAsync(StripMetadata(customJson), language);

            if (!string.IsNullOrWhiteSpace(registration.FallbackTranslation))
                await LoadLanguageChainAsync(registration.FallbackTranslation!, visitedLanguages);
        }
    }

    /// <summary>
    /// Recursively retrieves the translation for a given key and language, following fallback chains defined in custom language registrations if necessary.
    /// </summary>
    /// <param name="key">The translation key for which to retrieve the translation. This should be a valid key that may have a corresponding translation in the loaded data.</param>
    /// <param name="language">The language code for which to retrieve the translation. This should be a valid language code that may have a corresponding translation in the loaded data or a registered custom language.</param>
    /// <param name="visitedLanguages">A set of languages that have already been visited during the lookup process to prevent infinite loops in fallback chains.</param>
    /// <returns>A string containing the translation for the specified key and language if found; otherwise, <see langword="null"/>.</returns>
    private static string? GetTranslationWithFallback(string key, string language, HashSet<string> visitedLanguages)
    {
        if (!visitedLanguages.Add(language))
            return null;

        if (_translations.TryGetValue(key, out var langDict) && langDict.TryGetValue(language, out var translation))
            return translation;

        if (_customLanguages.TryGetValue(language, out var registration) && !string.IsNullOrWhiteSpace(registration.FallbackTranslation))
            return GetTranslationWithFallback(key, registration.FallbackTranslation!, visitedLanguages);

        return null;
    }

    /// <summary>
    /// Rebuilds the list of available languages by combining built-in languages and custom registered languages, ensuring that display names are retrieved from metadata when available and that the list is sorted alphabetically by display name. This method is called whenever a new custom language is registered to ensure that the AvailableLanguages property reflects all current options.
    /// </summary>
    private static void RebuildAvailableLanguages()
    {
        AvailableLanguages ??= [];

        var languageNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var languageCode in _builtInLanguageCodes)
            languageNames[languageCode] = GetBuiltInLanguageDisplayName(languageCode) ?? languageCode;

        foreach (var (languageCode, registration) in _customLanguages)
            languageNames[languageCode] = GetLanguageDisplayName(registration.Source) ?? languageCode;

        var orderedLanguages = languageNames
             .OrderBy(x => GetLanguageSortName(x.Value), StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();

        AvailableLanguages.Clear();
        foreach (var (key, value) in orderedLanguages)
            AvailableLanguages[key] = value;
    }

    /// <summary>
    /// Retrieves the display name for a built-in language based on its language code. This method uses a predefined mapping of language codes to their corresponding display names, which can be extended as needed. If the provided language code does not have a corresponding display name in the mapping, the method returns the original language code as a fallback.
    /// </summary>
    /// <param name="languageDisplayName">The language code for which to retrieve the display name (e.g., "en", "pl").</param>
    /// <returns>A string containing the display name for the specified language code if found; otherwise, the original language code.</returns>
    private static string GetLanguageSortName(string languageDisplayName)
    {
        for (var i = 0; i < languageDisplayName.Length; i++)
            if (char.IsLetterOrDigit(languageDisplayName[i]))
                return languageDisplayName[i..];

        return languageDisplayName;
    }

    /// <summary>
    /// Retrieves the display name of a built-in language from its corresponding JSON resource. The method attempts to load the JSON file associated with the given language code and extract the "_language" property, which is expected to contain the display name. If the JSON file cannot be found, read, or does not contain a valid "_language" property, the method returns <see langword="null"/>. This allows for built-in languages to have user-friendly display names defined within their translation resources while falling back to the language code if necessary.
    /// </summary>
    /// <param name="languageCode">The language code for which to retrieve the display name (e.g., "en", "pl").</param>
    /// <returns>A string containing the display name for the specified built-in language if found; otherwise, <see langword="null"/>.</returns>
    private static string? GetBuiltInLanguageDisplayName(string languageCode)
    {
        var resourcePath = $"Utils/Translator/Translations/{languageCode}.json";
        var json = StswFnUI.GetResourceAsText(Assembly.GetExecutingAssembly().FullName!, resourcePath);

        return string.IsNullOrWhiteSpace(json) ? null : GetLanguageDisplayNameFromJson(json);
    }

    /// <summary>
    /// Extracts the display name of the language from the provided JSON string by looking for a property named "_language". If the property exists and is a string, its value is returned as the display name. If the JSON is malformed, does not contain the "_language" property, or if the property is not a string, the method returns <see langword="null"/>. This allows for flexible retrieval of language display names from custom translation metadata while ensuring that errors in the JSON structure do not cause exceptions to be thrown.
    /// </summary>
    /// <param name="json">A JSON string that may contain a "_language" property representing the display name of the language. The JSON is expected to be an object at the root level.</param>
    /// <returns>A string containing the display name of the language if the "_language" property is found and valid; otherwise, <see langword="null"/>.</returns>
    private static string? GetLanguageDisplayNameFromJson(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty("_language", out var property) && property.ValueKind == JsonValueKind.String)
                return property.GetString();
        }
        catch
        {
            // Ignore malformed custom translation metadata.
        }

        return null;
    }


    /// <summary>
    /// Retrieves the display name of the language from the custom translation metadata at the specified URI.
    /// </summary>
    /// <remarks>Returns <see langword="null"/> if the metadata is missing, malformed, or does not contain a valid language display name.</remarks>
    /// <param name="source">The URI of the source containing the custom translation metadata. Cannot be <see langword="null"/>.</param>
    /// <returns>A string containing the language display name if found; otherwise, <see langword="null"/>.</returns>
    private static string? GetLanguageDisplayName(Uri source)
    {
        var json = LoadJsonFromUri(source);
        return string.IsNullOrWhiteSpace(json) ? null : GetLanguageDisplayNameFromJson(json);
    }

    /// <summary>
    /// Loads a JSON string from the specified URI, which is expected to point to an application resource. If the resource cannot be found or read, returns <see langword="null"/>.
    /// </summary>
    /// <param name="source">The URI of the resource to load. This should be a valid pack URI pointing to an embedded resource within the application.</param>
    /// <returns>A JSON string loaded from the specified URI, or <see langword="null"/> if the resource cannot be found or read.</returns>
    private static string? LoadJsonFromUri(Uri source)
    {
        try
        {
            var resourceInfo = Application.GetResourceStream(source);
            if (resourceInfo == null)
                return null;

            using var reader = new StreamReader(resourceInfo.Stream);
            return reader.ReadToEnd();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Strips metadata properties (those starting with an underscore) from the given JSON string and returns a new JSON string containing only the string-valued properties.
    /// </summary>
    /// <remarks>Properties with names beginning with an underscore are considered metadata and are not included in the resulting JSON. Only properties with string values are retained; other types of values are ignored. If the input JSON is invalid, the original string is returned.</remarks>
    /// <param name="json">The JSON string to process. Must represent a JSON object.</param>
    /// <returns>A JSON string containing only the non-metadata string properties from the original JSON, or the original string if parsing fails.</returns>
    private static string StripMetadata(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var translations = new Dictionary<string, string>();

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Name.StartsWith('_'))
                    continue;

                if (property.Value.ValueKind == JsonValueKind.String)
                    translations[property.Name] = property.Value.GetString() ?? string.Empty;
            }

            return JsonSerializer.Serialize(translations);
        }
        catch
        {
            return json;
        }
    }

    /// <summary>
    /// Represents a registration for a custom language source, including an optional fallback translation.
    /// </summary>
    /// <param name="Source">The URI identifying the source of the custom language registration. Cannot be <see langword="null"/>.</param>
    /// <param name="FallbackTranslation">An optional fallback translation to use if the source does not provide a translation. May be <see langword="null"/>.</param>
    private sealed record CustomLanguageRegistration(Uri Source, string? FallbackTranslation);

    /// <summary>
    /// Synchronizes the current language setting with the specified language value from the settings.
    /// </summary>
    /// <param name="language">The language code to synchronize with, or <see langword="null"/> to clear the current language setting.</param>
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
    /// Occurs when a property of the TranslationManager changes (e.g., CurrentLanguage).
    /// Used to notify the UI that translations need to be refreshed.
    /// </summary>
    public static event PropertyChangedEventHandler? PropertyChanged;
    private static void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
}
