namespace StswExpress;

/// <summary>
/// 
/// </summary>
public interface ITranslatorFileLanguageLoader
{
    bool CanLoadFile(string fileName);
    void LoadFile(string fileName, TranslatorLanguagesLoader mainLoader);
}
