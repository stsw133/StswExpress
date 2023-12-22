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
/// For Translate something just type Translator.Tr(...).
/// </summary>
public class StswTranslator : StswObservableObject
{
    private static StswTranslator? instance = null;
    public static StswTranslator Instance => instance ??= new StswTranslator();

    /// <summary>
    /// List of all availables languages where at least one translation is present.
    /// </summary>
    public static ObservableCollection<string> AvailableLanguages = new() { "" };

    /// <summary>
    /// Current language used for displaying texts with Translator.
    /// By default is equal to "en" (English) if not set manually.
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
                    Instance.NotifyPropertyChanged();
                }
            }
        }
    }
    private static string currentLanguage = "en";

    public static event EventHandler<TranslatorLanguageChangingEventArgs>? CurrentLanguageChanging;
    public static event EventHandler<TranslatorLanguageChangedEventArgs>? CurrentLanguageChanged;

    /// <summary>
    /// 
    /// </summary>
    public static SortedDictionary<string, SortedDictionary<string, StswTranslatorTranslation>> TranslationsDictionary { get; private set; } = new();

    /// <summary>
    /// Translate the given textID in current language.
    /// </summary>
    /// <param name="textID">Text to translate identifier</param>
    /// <param name="defaultText">Text to return if no text correspond to textID in the current language</param>
    /// <param name="languageID">Language ID in which to get the translation. To Specify if not CurrentLanguage</param>
    /// <returns>The translated text</returns>
    public static string Tr(string? textID, string? defaultText = null, string? languageID = null)
    {
        if (string.IsNullOrEmpty(textID))
            throw new InvalidOperationException($"The {nameof(textID)} argument cannot be null or empty");

        if (string.IsNullOrEmpty(defaultText))
            defaultText = textID;

        if (string.IsNullOrEmpty(languageID))
            languageID = CurrentLanguage;

        LogMissingTranslation(textID, defaultText);

        var result = defaultText;

        if (TranslationsDictionary.ContainsKey(textID) && TranslationsDictionary[textID].ContainsKey(languageID))
            result = TranslationsDictionary[textID][languageID].TranslatedText!;

        return result;
    }

    /// <summary>
    /// For developpers, for developement and/or debug time.
    /// If set to <c>True</c> Log Out in a file automatically all textId asked to be translate but missing.
    /// </summary>
    public static bool LogOutMissingTranslations { get; set; } = false;

    /// <summary>
    /// 
    /// </summary>
    public static SortedDictionary<string, SortedDictionary<string, string>> MissingTranslations { get; } = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="textID"></param>
    /// <param name="defaultText"></param>
    private static void LogMissingTranslation(string textID, string defaultText)
    {
        if (LogOutMissingTranslations)
        {
            AvailableLanguages.ToList().ForEach(delegate (string languageId)
            {
                if (!TranslationsDictionary.ContainsKey(textID) || !TranslationsDictionary[textID].ContainsKey(languageId))
                {
                    if (!MissingTranslations.ContainsKey(textID))
                        MissingTranslations.Add(textID, new SortedDictionary<string, string>());

                    MissingTranslations[textID][languageId] = $"default text : {defaultText}";
                }
            });

            var missingTranslationsFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "TranslatorMissingTranslations.json");
            File.WriteAllText(missingTranslationsFileName, JsonSerializer.Serialize(MissingTranslations, new JsonSerializerOptions() { WriteIndented = true }));
        }
    }
}
