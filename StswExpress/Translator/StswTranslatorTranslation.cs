namespace StswExpress;
/// <summary>
/// Represents a translation entry in the StswTranslator system, containing information about the source, text ID, language ID, and the translated text.
/// </summary>
public class StswTranslatorTranslation
{
    /// <summary>
    /// Gets or sets the source of the translation, such as a filename or identifier indicating where the translation originated.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the text to be translated.
    /// </summary>
    public string? TextID { get; set; }

    /// <summary>
    /// Gets or sets the language identifier for the translation, indicating the language in which the text is translated.
    /// </summary>
    public string? LanguageID { get; set; }

    /// <summary>
    /// Gets or sets the actual translated text in the specified language.
    /// </summary>
    public string? TranslatedText { get; set; }
}
