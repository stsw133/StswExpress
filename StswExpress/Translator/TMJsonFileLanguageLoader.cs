﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class TMJsonFileLanguageLoader : ITMFileLanguageLoader
{
    public bool CanLoadFile(string fileName) => fileName.ToLower().EndsWith(".tm.json");
    public void LoadFile(string fileName, TMLanguagesLoader mainLoader)
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
