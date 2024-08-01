using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StswExpress;
/// <summary>
/// Provides functionality for loading translations from JSON files into the StswTranslator system.
/// </summary>
public class StswTranslatorJsonFileLanguageLoader : IStswTranslatorFileLanguageLoader
{
    /// <summary>
    /// Determines if the specified file can be loaded based on its extension.
    /// </summary>
    /// <param name="fileName">The name of the file to check.</param>
    /// <returns><see langword="true"/> if the file is a JSON file; otherwise, <see langword="false"/>.</returns>
    public bool CanLoadFile(string fileName) => fileName.ToLower().EndsWith(".json");

    /// <summary>
    /// Asynchronously loads translations from the specified JSON file into the main translation loader.
    /// </summary>
    /// <param name="fileName">The name of the JSON file to load translations from.</param>
    /// <param name="mainLoader">The main translation loader to add translations to.</param>
    public async Task LoadFileAsync(string fileName, StswTranslatorLanguagesLoader mainLoader)
    {
        string json = await File.ReadAllTextAsync(fileName);

        var fileDictionary = JsonSerializer.Deserialize<SortedDictionary<string, SortedDictionary<string, string>>>(json);
        fileDictionary?.Keys.ToList().ForEach((string textId) =>
            fileDictionary[textId].Keys.ToList().ForEach((string languageId) =>
                mainLoader.AddTranslation(textId, languageId, fileDictionary[textId][languageId], fileName)
            )
        );
    }
}
