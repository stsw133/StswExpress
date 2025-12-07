global using Avalonia;
global using Avalonia.Controls.ApplicationLifetimes;
global using Avalonia.Markup.Xaml;
global using StswExpress.Avalonia;
global using StswExpress.Commons;

namespace TestApp.Avalonia;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        if (StswResources.GetInstance() is StswResources resources)
            resources.CurrentTheme = "Dark"; //or `string.Empty` to get default theme
    }

    /// <inheritdoc/>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();

        base.OnFrameworkInitializationCompleted();
    }
}
