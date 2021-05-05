namespace StswExpress.Translate
{
    public interface ITMFileLanguageLoader
    {
        bool CanLoadFile(string fileName);
        void LoadFile(string fileName, TMLanguagesLoader mainLoader);
    }
}
