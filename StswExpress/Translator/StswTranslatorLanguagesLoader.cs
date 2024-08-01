using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StswExpress;

/// <summary>
/// Provides functionality for loading and managing language translations in the StswTranslator system.
/// </summary>
/// <param name="tmInstance">The instance of <see cref="StswTranslator"/> to be used.</param>
public class StswTranslatorLanguagesLoader(StswTranslator tmInstance)
{
    readonly StswTranslator? _tmInstance = tmInstance;

    /// <summary>
    /// Gets the singleton instance of the <see cref="StswTranslatorLanguagesLoader"/> class.
    /// </summary>
    public static StswTranslatorLanguagesLoader Instance => instance ??= new StswTranslatorLanguagesLoader(StswTranslator.Instance);
    private static StswTranslatorLanguagesLoader? instance = null;

    /// <summary>
    /// Gets or sets the list of file loaders that handle different translation file formats.
    /// </summary>
    internal List<IStswTranslatorFileLanguageLoader> FileLanguageLoaders { get; set; } = [new StswTranslatorJsonFileLanguageLoader()];

    /// <summary>
    /// Adds a new translation to the language dictionaries.
    /// </summary>
    /// <param name="textID">The identifier for the text to translate.</param>
    /// <param name="languageID">The language identifier of the translation.</param>
    /// <param name="value">The translated text value.</param>
    /// <param name="source">The source of the translation (optional).</param>
    public void AddTranslation(string textID, string languageID, string value, string source = "")
    {
        if (!StswTranslator.TranslationsDictionary.ContainsKey(textID))
            StswTranslator.TranslationsDictionary[textID] = [];

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
    /// Asynchronously loads the specified file into the language dictionaries.
    /// </summary>
    /// <param name="fileName">The name of the file to load.</param>
    public async Task AddFileAsync(string fileName)
    {
        var loader = FileLanguageLoaders.Find(loader => loader.CanLoadFile(fileName));
        if (loader != null && loader is StswTranslatorJsonFileLanguageLoader jsonLoader)
        {
            await jsonLoader.LoadFileAsync(fileName, this);
        }
    }

    /// <summary>
    /// Asynchronously loads all language files from the specified directory into the language dictionaries.
    /// </summary>
    /// <param name="path">The path of the directory to load.</param>
    public async Task AddDirectoryAsync(string path)
    {
        var files = Directory.GetFiles(path);
        var tasks = files.Select(AddFileAsync);
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Removes all translations that originate from the specified source.
    /// </summary>
    /// <param name="source">The filename or source of the translation.</param>
    public void RemoveAllFromSource(string source)
    {
        if (_tmInstance != null)
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
    /// Clears all translations and available languages from the dictionaries.
    /// </summary>
    public void ClearAllTranslations()
    {
        StswTranslator.TranslationsDictionary.Clear();
        StswTranslator.AvailableLanguages.Clear();
        //StswTranslator.MissingTranslations.Clear();
    }
}
