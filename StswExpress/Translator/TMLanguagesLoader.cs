using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class TMLanguagesLoader
{
    private static TMLanguagesLoader? instance = null;
    public static TMLanguagesLoader Instance => instance ??= new TMLanguagesLoader(TM.Instance);

    readonly TM? tmInstance = null;

    public TMLanguagesLoader(TM tmInstance)
    {
        this.tmInstance = tmInstance;
    }

    /// <summary>
    /// 
    /// </summary>
    public List<ITMFileLanguageLoader> FileLanguageLoaders { get; set; } = new List<ITMFileLanguageLoader>() { new TMJsonFileLanguageLoader() };

    /// <summary>
    /// Add a new translation in the language dictionaries.
    /// </summary>
    /// <param name="textID">Text to translate identifier.</param>
    /// <param name="languageID">Language identifier of the translation.</param>
    /// <param name="value">Value of the translated text.</param>
    public void AddTranslation(string textID, string languageID, string value, string source = "")
    {
        if (tmInstance != null)
        {

            if (!tmInstance.TranslationsDictionary.ContainsKey(textID))
                tmInstance.TranslationsDictionary[textID] = new SortedDictionary<string, TMTranslation>();

            if (!tmInstance.AvailableLanguages.Contains(languageID))
                tmInstance.AvailableLanguages.Add(languageID);

            tmInstance.TranslationsDictionary[textID][languageID] = new TMTranslation()
            {
                TextID = textID,
                LanguageID = languageID,
                TranslatedText = value,
                Source = source
            };
        }
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
            tmInstance.TranslationsDictionary.Keys.ToList().ForEach(textId =>
            {
                tmInstance.TranslationsDictionary[textId].Values.ToList().ForEach(translation =>
                {
                    if (translation?.Source?.Equals(source) == true)
                        tmInstance.TranslationsDictionary[textId].Remove(translation?.LanguageID ?? string.Empty);
                });

                if (tmInstance.TranslationsDictionary[textId].Count == 0)
                    tmInstance.TranslationsDictionary.Remove(textId);
            });
        }
    }

    /// <summary>
    /// Empty all dictionnaries.
    /// </summary>
    public void ClearAllTranslations()
    {
        TM.Instance.TranslationsDictionary.Clear();
        TM.Instance.AvailableLanguages.Clear();
        TM.Instance.MissingTranslations.Clear();
    }
}
