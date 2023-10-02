using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace StswExpress;

/// <summary>
/// TranslateMe most important class.
/// For Translate something just type TM.Tr(...) method and wow... ... magic happen ;)
/// </summary>
public class TM : StswObservableObject
{
    private static TM? instance = null;
    public static TM Instance => instance ??= new TM();

    public TM()
    {

    }

    /// <summary>
    /// List of all availables languages where at least one translation is present.
    /// </summary>
    public ObservableCollection<string> AvailableLanguages => availableLanguages ??= new ObservableCollection<string>();
    private ObservableCollection<string> availableLanguages = new ObservableCollection<string>();

    /// <summary>
    /// Current language used for displaying texts with TranslateMe.
    /// By default is equal to "en" (English) if not set manually.
    /// </summary>
    public string CurrentLanguage
    {
        get => currentLanguage;
        set
        {
            if (!AvailableLanguages.Contains(value))
                AvailableLanguages.Add(value);

            if (!currentLanguage.Equals(value))
            {
                var changingArgs = new TMLanguageChangingEventArgs(currentLanguage, value);
                var changedArgs = new TMLanguageChangedEventArgs(currentLanguage, value);

                CurrentLanguageChanging?.Invoke(this, changingArgs);

                if (!changingArgs.Cancel)
                {
                    currentLanguage = value;
                    CurrentLanguageChanged?.Invoke(this, changedArgs);
                    NotifyPropertyChanged();
                }
            }
        }
    }
    private string currentLanguage = "en";

    public event EventHandler<TMLanguageChangingEventArgs>? CurrentLanguageChanging;
    public event EventHandler<TMLanguageChangedEventArgs>? CurrentLanguageChanged;

    /// <summary>
    /// 
    /// </summary>
    public SortedDictionary<string, SortedDictionary<string, TMTranslation>> TranslationsDictionary => new SortedDictionary<string, SortedDictionary<string, TMTranslation>>();

    /// <summary>
    /// Translate the given textId in current language.
    /// This method is a shortcut to Instance.Translate
    /// </summary>
    /// <param name="textID">Text to translate identifier.</param>
    /// <param name="defaultText">Text to return if no text correspond to textID in the current language.</param>
    /// <param name="languageID">Language ID in which to get the translation. To Specify it if not CurrentLanguage.</param>
    /// <returns>The translated text</returns>
    public static string Tr(string? textID, string? defaultText = null, string? languageID = null) => Instance.Translate(textID, defaultText, languageID);

    /// <summary>
    /// Translate the given textId in current language.
    /// </summary>
    /// <param name="textID">Text to translate identifier</param>
    /// <param name="defaultText">Text to return if no text correspond to textID in the current language</param>
    /// <param name="languageID">Language ID in which to get the translation. To Specify if not CurrentLanguage</param>
    /// <returns>The translated text</returns>
    public string Translate(string? textID, string? defaultText = null, string? languageID = null)
    {
        if (string.IsNullOrEmpty(textID))
            throw new InvalidOperationException("The textID argument cannot be null or empty");

        if (string.IsNullOrEmpty(defaultText))
            defaultText = textID;

        if (string.IsNullOrEmpty(languageID))
            languageID = Instance.CurrentLanguage;

        LogMissingTranslation(textID, defaultText);

        string result = defaultText;

        if (TranslationsDictionary.ContainsKey(textID) && TranslationsDictionary[textID].ContainsKey(languageID))
            result = TranslationsDictionary[textID][languageID].TranslatedText;

        return result;
    }

    /// <summary>
    /// For developpers, for developement and/or debug time.
    /// If set to <c>True</c> Log Out in a file automatically all textId asked to be translate but missing.
    /// </summary>
    public bool LogOutMissingTranslations { get; set; } = false;

    /// <summary>
    /// 
    /// </summary>
    public SortedDictionary<string, SortedDictionary<string, string>> MissingTranslations { get; } = new SortedDictionary<string, SortedDictionary<string, string>>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="textID"></param>
    /// <param name="defaultText"></param>
    private void LogMissingTranslation(string textID, string defaultText)
    {
        if (LogOutMissingTranslations)
        {
            AvailableLanguages.ToList().ForEach(delegate (string languageId)
            {
                if (!TranslationsDictionary.ContainsKey(textID)
                    || !TranslationsDictionary[textID].ContainsKey(languageId))
                {
                    if (!MissingTranslations.ContainsKey(textID))
                        MissingTranslations.Add(textID, new SortedDictionary<string, string>());

                    MissingTranslations[textID][languageId] = $"default text : {defaultText}";
                }
            });

            var missingTranslationsFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "TMMissingTranslations.json");
            File.WriteAllText(missingTranslationsFileName, JsonConvert.SerializeObject(MissingTranslations, Formatting.Indented));
        }
    }
}
