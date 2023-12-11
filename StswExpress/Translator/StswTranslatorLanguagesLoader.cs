using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswTranslatorLanguagesLoader
{
    private static StswTranslatorLanguagesLoader? instance = null;
    public static StswTranslatorLanguagesLoader Instance => instance ??= new StswTranslatorLanguagesLoader(StswTranslator.Instance);

    readonly StswTranslator? tmInstance = null;

    public StswTranslatorLanguagesLoader(StswTranslator tmInstance)
    {
        this.tmInstance = tmInstance;
    }

    /// <summary>
    /// 
    /// </summary>
    internal List<IStswTranslatorFileLanguageLoader> FileLanguageLoaders { get; set; } = new List<IStswTranslatorFileLanguageLoader>() { new StswTranslatorJsonFileLanguageLoader() };

    /// <summary>
    /// Add a new translation in the language dictionaries.
    /// </summary>
    /// <param name="textID">Text to translate identifier.</param>
    /// <param name="languageID">Language identifier of the translation.</param>
    /// <param name="value">Value of the translated text.</param>
    public void AddTranslation(string textID, string languageID, string value, string source = "")
    {
        if (!StswTranslator.TranslationsDictionary.ContainsKey(textID))
            StswTranslator.TranslationsDictionary[textID] = new SortedDictionary<string, StswTranslatorTranslation>();

        if (!StswTranslator.AvailableLanguages.Contains(languageID))
            StswTranslator.AvailableLanguages.Add(languageID);

        StswTranslator.TranslationsDictionary[textID][languageID] = new StswTranslatorTranslation()
        {
            TextID = textID,
            LanguageID = languageID,
            TranslatedText = value,
            Source = source
        };
    }

    /// <summary>
    /// Load the specified file in the Languages dictionaries.
    /// </summary>
    /// <param name="fileName">Filename of the file to load.</param>
    public void AddFile(string fileName) => FileLanguageLoaders.Find(loader => loader.CanLoadFile(fileName))?.LoadFile(fileName, this);

    /// <summary>
    /// Load all language files of the specified directory in the languages dictionnaries.
    /// </summary>
    /// <param name="path">Path of the directory to load.</param>
    public void AddDirectory(string path) => Directory.GetFiles(path).ToList().ForEach(x => AddFile(x));

    /// <summary>
    /// Remove all translations that come from the specified source.
    /// </summary>
    /// <param name="source">Filename or source of the translation.</param>
    public void RemoveAllFromSource(string source)
    {
        if (tmInstance != null)
        {
            StswTranslator.TranslationsDictionary.Keys.ToList().ForEach(textId =>
            {
                StswTranslator.TranslationsDictionary[textId].Values.ToList().ForEach(translation =>
                {
                    if (translation?.Source?.Equals(source) == true)
                        StswTranslator.TranslationsDictionary[textId].Remove(translation?.LanguageID ?? string.Empty);
                });

                if (StswTranslator.TranslationsDictionary[textId].Count == 0)
                    StswTranslator.TranslationsDictionary.Remove(textId);
            });
        }
    }

    /// <summary>
    /// Empty all dictionnaries.
    /// </summary>
    public void ClearAllTranslations()
    {
        StswTranslator.TranslationsDictionary.Clear();
        StswTranslator.AvailableLanguages.Clear();
        //StswTranslator.MissingTranslations.Clear();
    }
}
