using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace StswExpress;
/// <summary>
/// Translator's most important class.
/// For translating something just type StswTranslator.Tr(...).
/// </summary>
public class StswTranslator : StswObservableObject
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="StswTranslator"/> class.
    /// </summary>
    public static StswTranslator Instance => instance ??= new StswTranslator();
    private static StswTranslator? instance = null;

    /// <summary>
    /// Cache to store already translated texts to improve performance.
    /// </summary>
    private static readonly Dictionary<string, string> _translationCache = [];

    /// <summary>
    /// List of all available languages where at least one translation is present.
    /// </summary>
    public static ObservableCollection<string> AvailableLanguages = [null];

    /// <summary>
    /// Current language used for displaying texts with the translator.
    /// By default, it is set to "en" (English) if not set manually.
    /// </summary>
    internal static string CurrentLanguage
    {
        get => currentLanguage;
        set
        {
            if (!AvailableLanguages.Contains(value))
                AvailableLanguages.Add(value);

            if (!currentLanguage.Equals(value))
            {
                var changingArgs = new TranslatorLanguageChangingEventArgs(currentLanguage, value);
                var changedArgs = new TranslatorLanguageChangedEventArgs(currentLanguage, value);

                CurrentLanguageChanging?.Invoke(Instance, changingArgs);

                if (!changingArgs.Cancel)
                {
                    currentLanguage = value;
                    CurrentLanguageChanged?.Invoke(Instance, changedArgs);
                    Instance.OnPropertyChanged();
                    _translationCache.Clear(); /// clear cache when language changes
                }
            }
        }
    }
    private static string currentLanguage = "en";

    /// <summary>
    /// Event that occurs when the current language is changing.
    /// </summary>
    public static event EventHandler<TranslatorLanguageChangingEventArgs>? CurrentLanguageChanging;
    
    /// <summary>
    /// Event that occurs after the current language has changed.
    /// </summary>
    public static event EventHandler<TranslatorLanguageChangedEventArgs>? CurrentLanguageChanged;

    /// <summary>
    /// Dictionary containing all translations, organized by text ID and language.
    /// </summary>
    public static SortedDictionary<string, SortedDictionary<string, StswTranslatorTranslation>> TranslationsDictionary { get; private set; } = [];

    /// <summary>
    /// Translates the given textID into the current language.
    /// </summary>
    /// <param name="textID">The identifier for the text to translate.</param>
    /// <param name="defaultText">The text to return if no translation is found for the textID in the current language.</param>
    /// <param name="languageID">The language ID in which to get the translation. If not specified, the current language is used.</param>
    /// <returns>The translated text.</returns>
    /// <exception cref="InvalidOperationException">Thrown when textID is null or empty.</exception>
    public static string Tr(string? textID, string? defaultText = null, string? languageID = null)
    {
        if (string.IsNullOrEmpty(textID))
            throw new InvalidOperationException($"The {nameof(textID)} argument cannot be null or empty");

        if (string.IsNullOrEmpty(defaultText))
            defaultText = textID;

        if (string.IsNullOrEmpty(languageID))
            languageID = CurrentLanguage;

        var cacheKey = $"{languageID}_{textID}";
        if (_translationCache.TryGetValue(cacheKey, out var cachedTranslation))
            return cachedTranslation;

        LogMissingTranslation(textID, defaultText);

        var result = defaultText;

        if (TranslationsDictionary.TryGetValue(textID, out var value1) && value1.TryGetValue(languageID, out var value2))
        {
            result = value2.TranslatedText!;
            _translationCache[cacheKey] = result; /// store result in cache
        }

        return result;
    }

    /// <summary>
    /// Indicates whether to log missing translations to a file during development or debugging.
    /// If set to <see langword="true">, logs all text IDs that are requested to be translated but are missing.
    /// </summary>
    public static bool LogOutMissingTranslations { get; set; } = false;

    /// <summary>
    /// Dictionary containing missing translations, organized by text ID and language.
    /// </summary>
    public static SortedDictionary<string, SortedDictionary<string, string>> MissingTranslations { get; } = [];

    /// <summary>
    /// Logs missing translations for a given text ID and default text.
    /// </summary>
    /// <param name="textID">The identifier for the text that is missing a translation.</param>
    /// <param name="defaultText">The default text to log.</param>
    private static void LogMissingTranslation(string textID, string defaultText)
    {
        if (LogOutMissingTranslations)
        {
            AvailableLanguages.ToList().ForEach(delegate (string languageId)
            {
                if (!TranslationsDictionary.TryGetValue(textID, out var value) || !value.ContainsKey(languageId))
                {
                    if (!MissingTranslations.ContainsKey(textID))
                        MissingTranslations.Add(textID, []);

                    MissingTranslations[textID][languageId] = $"default text : {defaultText}";
                }
            });

            var missingTranslationsFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "TranslatorMissingTranslations.json");
            File.WriteAllText(missingTranslationsFileName, JsonSerializer.Serialize(MissingTranslations, new JsonSerializerOptions() { WriteIndented = true }));
        }
    }
}
