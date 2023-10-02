using System.Windows;

namespace StswExpress;

/// <summary>
/// This class is used as ViewModel to bind to DependencyProperties.
/// Is used by Tr MarkupExtension to dynamically update the translation when current language changed.
/// </summary>
public class TrData : StswObservableObject
{
    public TrData()
    {
        WeakEventManager<TM, TMLanguageChangedEventArgs>.AddHandler(TM.Instance, nameof(CurrentLanguageChanged), CurrentLanguageChanged);
    }

    /// <summary>
    /// To force the use of a specific identifier.
    /// </summary>
    public string? TextID { get; set; }

    /// <summary>
    /// Text to return if no text correspond to textID in the current language.
    /// </summary>
    public string? DefaultText { get; set; }
    
    /// <summary>
    /// Language ID in which to get the translation. To Specify if not CurrentLanguage.
    /// </summary>
    public string? LanguageID { get; set; }

    /// <summary>
    /// Provides a prefix to add at the begining of the translated text.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Provides a suffix to add at the end of the translated text.
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    /// <summary>
    /// An optional object use as data that is represented by this translation.
    /// (Example used for Enum values translation).
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Updates the binding when the current language changed (calls OnPropertyChanged).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CurrentLanguageChanged(object? sender, TMLanguageChangedEventArgs e) => NotifyPropertyChanged(nameof(TranslatedText));

    /// <summary>
    /// 
    /// </summary>
    public string TranslatedText => Prefix + TM.Tr(TextID, DefaultText, LanguageID) + Suffix;

    ~TrData()
    {
        WeakEventManager<TM, TMLanguageChangedEventArgs>.RemoveHandler(TM.Instance, "CurrentLanguageChanged", CurrentLanguageChanged);
    }
}
