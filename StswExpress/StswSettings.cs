using System.Configuration;
using System.Globalization;

namespace StswExpress;

public sealed partial class StswSettings
{
    public StswSettings()
    {
        // PropertyChanged += StswSettings_PropertyChanged;
        SettingChanging += StswSettings_SettingChanging;
        // SettingsLoaded += StswSettings_SettingsLoaded;
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
                    if (StswResources.GetInstance() is StswResources theme and not null)
                        theme.Theme = (StswTheme)e.NewValue;
                    break;
                }
        }
    }
    
    //private void StswSettings_SettingsLoaded(object sender, CancelEventArgs e)
    //{
    //    
    //}
    
    //private void StswSettings_SettingsSaving(object sender, CancelEventArgs e)
    //{
    //    
    //}
}
