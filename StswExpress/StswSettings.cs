using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace StswExpress;

public sealed partial class StswSettings
{
    public StswSettings()
    {
        // PropertyChanged += StswSettings_PropertyChanged;
        SettingChanging += this.StswSettings_SettingChanging;
        // SettingsLoaded += this.StswSettings_SettingsLoaded;
        // SettingsSaving += this.StswSettings_SettingsSaving;
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
                    Translator.Instance.CurrentLanguage = string.IsNullOrEmpty((string?)e.NewValue) ? CultureInfo.InstalledUICulture.TwoLetterISOLanguageName : (string)e.NewValue;
                    break;
                }
            case nameof(Default.Theme):
                {
                    if (Application.Current.Resources.MergedDictionaries.FirstOrDefault(x => x is StswResources) is StswResources theme)
                        theme.Theme = (int)e.NewValue < 0 ? StswFn.GetWindowsTheme() : (StswTheme)e.NewValue;
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
