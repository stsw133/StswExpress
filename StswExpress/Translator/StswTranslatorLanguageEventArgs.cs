﻿using System;
using System.ComponentModel;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class TranslatorLanguageChangedEventArgs : EventArgs
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="oldLanguageId">Language ID before the change.</param>
    /// <param name="newLanguageId">Language ID after the change.</param>
    public TranslatorLanguageChangedEventArgs(string oldLanguageId, string newLanguageId)
    {
        OldLanguageId = oldLanguageId;
        NewLanguageId = newLanguageId;
    }

    /// <summary>
    /// Language ID before the change.
    /// </summary>
    public string OldLanguageId { get; private set; }

    /// <summary>
    /// Language ID after the change.
    /// </summary>
    public string NewLanguageId { get; private set; }
}

/// <summary>
/// 
/// </summary>
public class TranslatorLanguageChangingEventArgs : CancelEventArgs
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="oldLanguageId">Language ID before the change.</param>
    /// <param name="newLanguageId">Language ID after the change.</param>
    public TranslatorLanguageChangingEventArgs(string oldLanguageId, string newLanguageId)
    {
        OldLanguageId = oldLanguageId;
        NewLanguageId = newLanguageId;
    }

    /// <summary>
    /// Language ID before the change.
    /// </summary>
    public string OldLanguageId { get; private set; }

    /// <summary>
    /// Language ID after the change.
    /// </summary>
    public string NewLanguageId { get; private set; }
}
