using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class TranslatorLanguagesLoader
{
    private static TranslatorLanguagesLoader? instance = null;
    public static TranslatorLanguagesLoader Instance => instance ??= new TranslatorLanguagesLoader(Translator.Instance);

    readonly Translator? tmInstance = null;

    public TranslatorLanguagesLoader(Translator tmInstance)
    {
        this.tmInstance = tmInstance;
    }

    /// <summary>
    /// 
    /// </summary>
    internal List<ITranslatorFileLanguageLoader> FileLanguageLoaders { get; set; } = new List<ITranslatorFileLanguageLoader>() { new TranslatorJsonFileLanguageLoader() };

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
            if (!Translator.TranslationsDictionary.ContainsKey(textID))
                Translator.TranslationsDictionary[textID] = new SortedDictionary<string, TranslatorTranslation>();

            if (!tmInstance.AvailableLanguages.Contains(languageID))
                tmInstance.AvailableLanguages.Add(languageID);

            Translator.TranslationsDictionary[textID][languageID] = new TranslatorTranslation()
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
            Translator.TranslationsDictionary.Keys.ToList().ForEach(textId =>
            {
                Translator.TranslationsDictionary[textId].Values.ToList().ForEach(translation =>
                {
                    if (translation?.Source?.Equals(source) == true)
                        Translator.TranslationsDictionary[textId].Remove(translation?.LanguageID ?? string.Empty);
                });

                if (Translator.TranslationsDictionary[textId].Count == 0)
                    Translator.TranslationsDictionary.Remove(textId);
            });
        }
    }

    /// <summary>
    /// Empty all dictionnaries.
    /// </summary>
    public void ClearAllTranslations()
    {
        Translator.TranslationsDictionary.Clear();
        Translator.Instance.AvailableLanguages.Clear();
        Translator.Instance.MissingTranslations.Clear();
    }
}
