using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StswExpress;
/// <summary>
/// 
/// </summary>
public class StswTranslatorJsonFileLanguageLoader : IStswTranslatorFileLanguageLoader
{
    public bool CanLoadFile(string fileName) => fileName.ToLower().EndsWith(".json");
    public void LoadFile(string fileName, StswTranslatorLanguagesLoader mainLoader)
    {
        string json = File.ReadAllText(fileName);

        var fileDictionary = JsonSerializer.Deserialize<SortedDictionary<string, SortedDictionary<string, string>>>(json);
        fileDictionary?.Keys.ToList().ForEach((string textId) =>
            fileDictionary[textId].Keys.ToList().ForEach((string languageId) =>
                mainLoader.AddTranslation(textId, languageId, fileDictionary[textId][languageId], fileName)
            )
        );
    }
}
