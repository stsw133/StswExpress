using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class TranslatorJsonFileLanguageLoader : ITranslatorFileLanguageLoader
{
    public bool CanLoadFile(string fileName) => fileName.ToLower().EndsWith(".json");
    public void LoadFile(string fileName, TranslatorLanguagesLoader mainLoader)
    {
        string json = File.ReadAllText(fileName);

        var fileDictionnary = JsonConvert.DeserializeObject<SortedDictionary<string, SortedDictionary<string, string>>>(json);
        if (fileDictionnary != null)
        {
            fileDictionnary.Keys.ToList().ForEach(delegate (string textId)
            {
                fileDictionnary[textId].Keys.ToList().ForEach(delegate (string languageId)
                {
                    mainLoader.AddTranslation(textId, languageId, fileDictionnary[textId][languageId], fileName);
                });
            });
        }
    }
}
