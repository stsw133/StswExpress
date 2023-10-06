namespace StswExpress;

/// <summary>
/// 
/// </summary>
public interface IStswTranslatorFileLanguageLoader
{
    bool CanLoadFile(string fileName);
    void LoadFile(string fileName, StswTranslatorLanguagesLoader mainLoader);
}
