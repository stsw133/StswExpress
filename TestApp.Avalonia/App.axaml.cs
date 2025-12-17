global using StswExpress.Avalonia;
global using StswExpress.Commons;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace TestApp.Ava;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : StswApp
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
