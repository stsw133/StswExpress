using System;
using System.ComponentModel;

namespace StswExpress;
/// <summary>
/// Provides data for the event that occurs after the language has been changed in the translator.
/// </summary>
public class TranslatorLanguageChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TranslatorLanguageChangedEventArgs"/> class.
    /// </summary>
    /// <param name="oldLanguageId">The language ID before the change.</param>
    /// <param name="newLanguageId">The language ID after the change.</param>
    public TranslatorLanguageChangedEventArgs(string oldLanguageId, string newLanguageId)
    {
        OldLanguageId = oldLanguageId;
        NewLanguageId = newLanguageId;
    }

    /// <summary>
    /// Gets the language ID before the change.
    /// </summary>
    public string OldLanguageId { get; private set; }

    /// <summary>
    /// Gets the language ID after the change.
    /// </summary>
    public string NewLanguageId { get; private set; }
}

/// <summary>
/// Provides data for the event that occurs when the language is about to change in the translator.
/// </summary>
public class TranslatorLanguageChangingEventArgs : CancelEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TranslatorLanguageChangingEventArgs"/> class.
    /// </summary>
    /// <param name="oldLanguageId">The language ID before the change.</param>
    /// <param name="newLanguageId">The language ID after the change.</param>
    public TranslatorLanguageChangingEventArgs(string oldLanguageId, string newLanguageId)
    {
        OldLanguageId = oldLanguageId;
        NewLanguageId = newLanguageId;
    }

    /// <summary>
    /// Gets the language ID before the change.
    /// </summary>
    public string OldLanguageId { get; private set; }

    /// <summary>
    /// Gets the language ID after the change.
    /// </summary>
    public string NewLanguageId { get; private set; }
}
