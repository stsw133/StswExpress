namespace StswExpress;

/// <summary>
/// 
/// </summary>
public interface ITMFileLanguageLoader
{
    bool CanLoadFile(string fileName);
    void LoadFile(string fileName, TMLanguagesLoader mainLoader);
}
