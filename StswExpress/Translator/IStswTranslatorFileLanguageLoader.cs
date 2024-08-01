using System.Threading.Tasks;namespace StswExpress;
/// <summary>
/// Defines the methods required for a file language loader in the StswTranslator system.
/// Implementations of this interface should provide functionality to determine if a file can be loaded
/// and to load translations from the file.
/// </summary>
public interface IStswTranslatorFileLanguageLoader
{
    /// <summary>
    /// Determines if the specified file can be loaded by the loader based on the file's characteristics.
    /// </summary>
    /// <param name="fileName">The name of the file to check.</param>
    /// <returns><see langword="true"/> if the file can be loaded; otherwise, <see langword="false"/>.</returns>
    bool CanLoadFile(string fileName);

    /// <summary>
    ///  Asynchronously loads translations from the specified file and adds them to the main translation loader.
    /// </summary>
    /// <param name="fileName">The name of the file to load translations from.</param>
    /// <param name="mainLoader">The main translation loader to which translations will be added.</param>
    Task LoadFileAsync(string fileName, StswTranslatorLanguagesLoader mainLoader);
}
