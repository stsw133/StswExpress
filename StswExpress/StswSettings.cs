﻿using System.Configuration;
using System.Globalization;

namespace StswExpress;

/// <summary>
/// Represents a settings handler for the <see cref="StswExpress"/> application, managing application settings and reacting to changes.
/// </summary>
public sealed partial class StswSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StswSettings"/> class and subscribes to events related to settings changes.
    /// </summary>
    public StswSettings()
    {
        // PropertyChanged += StswSettings_PropertyChanged;
        SettingChanging += StswSettings_SettingChanging;
        SettingsLoaded += StswSettings_SettingsLoaded;
        // SettingsSaving += StswSettings_SettingsSaving;
    }

    /*
    /// <summary>
    /// Handles the PropertyChanged event to perform actions when settings properties change.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments containing information about the property change.</param>
    private void StswSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Implement actions when properties change
    }
    */

    /// <summary>
    /// Handles the SettingChanging event to react to changes in specific settings.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments containing information about the changing setting.</param>
    private void StswSettings_SettingChanging(object sender, SettingChangingEventArgs e)
    {
        switch (e.SettingName)
        {
            case nameof(Default.Language):
                {
                    StswTranslator.CurrentLanguage = string.IsNullOrEmpty((string?)e.NewValue) ? CultureInfo.InstalledUICulture.TwoLetterISOLanguageName : (string)e.NewValue;
                    break;
                }
            case nameof(Default.Theme):
                {
                    if (StswResources.GetInstance() is StswResources theme)
                        theme.Theme = (StswTheme)e.NewValue;
                    break;
                }
        }
    }

    /// <summary>
    /// Handles the SettingsLoaded event to perform actions when settings are loaded, such as upgrading settings if required.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments related to the settings loaded event.</param>
    private void StswSettings_SettingsLoaded(object sender, SettingsLoadedEventArgs e)
    {
        try
        {
            if (Default.UpdateRequired)
            {
                Default.Upgrade();
                Default.UpdateRequired = false;
                Default.iSize = (double)(Default.GetPreviousVersion(nameof(Default.iSize)) ?? Default.iSize);
                Default.Language = (string)(Default.GetPreviousVersion(nameof(Default.Language)) ?? Default.Language);
                Default.Theme = (int)(Default.GetPreviousVersion(nameof(Default.Theme)) ?? Default.Theme);
                Default.Save();
            }
        }
        catch
        {
            Default.UpdateRequired = false;
            Default.Save();
        }
    }

    /*
    /// <summary>
    /// Handles the SettingsSaving event to perform actions before settings are saved.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments related to the settings saving event.</param>
    private void StswSettings_SettingsSaving(object sender, CancelEventArgs e)
    {
        // Implement actions before settings are saved
    }
    */
}
