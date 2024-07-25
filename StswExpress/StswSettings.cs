using System.Configuration;
using System.Globalization;

namespace StswExpress;
public sealed partial class StswSettings
{
    public StswSettings()
    {
        // PropertyChanged += StswSettings_PropertyChanged;
        SettingChanging += StswSettings_SettingChanging;
        SettingsLoaded += StswSettings_SettingsLoaded;
        // SettingsSaving += StswSettings_SettingsSaving;
    }

    //private void StswSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    //{
    //
    //}

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

    //private void StswSettings_SettingsSaving(object sender, CancelEventArgs e)
    //{
    //    
    //}
}
