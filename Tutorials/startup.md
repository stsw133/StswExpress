# StswExpress.Wpf Startup Tutorial

This tutorial shows how to correctly bootstrap a new WPF application with **StswExpress.Wpf**, based on the setup used in `TestApp.Wpf` (`App.xaml`, `App.xaml.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`).

## 1) Install required NuGet packages

At minimum, add:

- `StswExpress.Wpf`
- `StswExpress.Commons`

You can install them with:

```bash
dotnet add package StswExpress.Wpf
dotnet add package StswExpress.Commons
```

---

## 2) Configure `App.xaml` to use `StswApp`

Replace the default `<Application ...>` root with `<se:StswApp ...>`, and merge `StswResources`.

```xml
<se:StswApp x:Class="YourApp.Wpf.App"
            xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
            xmlns:se="clr-namespace:StswExpress.Wpf;assembly=StswExpress.Wpf"
            AllowMultipleInstances="False"
            StartupUri="Modules/MainWindow.xaml"
            DispatcherUnhandledException="Application_DispatcherUnhandledException">
    <se:StswApp.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <se:StswResources/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </se:StswApp.Resources>
</se:StswApp>
```

### Why this matters

- `StswApp` enables StswExpress-specific startup behavior.
- `StswResources` loads themes, styles, brushes, and shared control resources.
- `AllowMultipleInstances="False"` helps prevent duplicate app instances.
- `DispatcherUnhandledException` gives you one central place for runtime exception handling.

---

## 3) Configure `App.xaml.cs` to inherit from `StswApp`

Create an app class derived from `StswApp` and register global startup logic.

```csharp
global using StswExpress.Commons;
global using StswExpress.Wpf;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace YourApp.Wpf;

public partial class App : StswApp
{
    public static ICommand HelpCommand { get; } =
        new RoutedUICommand("Help", "Help", typeof(StswWindow), [new KeyGesture(Key.F1)]);

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        CommandManager.RegisterClassCommandBinding(
            typeof(StswWindow),
            new CommandBinding(HelpCommand, (_, _) => OpenHelp()));

        // Optional examples:
        // StswSecurity.Key = "myOwnStswHashKey";
        // StswTranslator.AvailableLanguages = new() { { "en", "English" } };
        // StswTranslator.CurrentLanguage = "en";
        // StswResources.AvailableThemes = ["Light", "Dark"];
        // StswResources.CurrentTheme = "Light";
    }

    private async void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        await StswDispatcher.RunWhenUiIsReadyAsync(() =>
            StswMessageDialog.Show(
                e.Exception,
                "Unhandled exception",
                options: new StswMessageDialogShowOptions { SaveLog = true }));

        e.Handled = true;
    }

    private static async void OpenHelp()
    {
        if (Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"Resources\\help_{StswApp.Settings.Language ?? "en"}.pdf")
            is string helpPath && File.Exists(helpPath))
        {
            StswFn.OpenPath(helpPath);
        }
        else if (Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\\help_en.pdf")
                 is string helpPathEN && File.Exists(helpPathEN))
        {
            StswFn.OpenPath(helpPathEN);
        }
        else
        {
            await StswMessageDialog.Show(
                "No help file is available!",
                "Information",
                null,
                StswDialogButtons.OK,
                StswDialogImage.Information,
                options: new StswMessageDialogShowOptions { SaveLog = false });
        }
    }
}
```

### What to keep from this pattern

- Use `StswApp` as your app base class.
- Keep global commands in `App` when they should work everywhere.
- Handle unhandled exceptions and show user-friendly dialogs.
- Optionally define theme/language/security defaults at startup.

---

## 4) Use `StswWindow` as your main window

In your `MainWindow.xaml`, inherit from `StswWindow` instead of standard `Window`.

```xml
<se:StswWindow x:Class="YourApp.Wpf.MainWindow"
               xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
               xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
               xmlns:se="clr-namespace:StswExpress.Wpf;assembly=StswExpress.Wpf"
               xmlns:local="clr-namespace:YourApp.Wpf"
               Title="My App" Height="600" Width="1000" WindowStartupLocation="CenterScreen">
    <se:StswWindow.Components>
        <se:StswButton Command="{x:Static local:App.HelpCommand}" DockPanel.Dock="Right" Style="{DynamicResource StswWindowButtonStyle}">
            <se:StswOutlinedText Text="?"/>
        </se:StswButton>
    </se:StswWindow.Components>

    <Grid>
        <se:StswText Text="Welcome to StswExpress.Wpf" HorizontalAlignment="Center" VerticalAlignment="Center"/>
    </Grid>
</se:StswWindow>
```

### Why this matters

- `StswWindow` provides built-in styling and window-level StswExpress integrations.
- `StswWindow.Components` is a convenient place for custom window-bar actions.

---

## 5) Keep `MainWindow.xaml.cs` simple

```csharp
namespace YourApp.Wpf;

public partial class MainWindow : StswWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
```

This is enough for startup. Add view logic or MVVM bindings later.

---

## 6) Recommended startup checklist

When creating a fresh app, verify all points below:

- [ ] `App.xaml` root is `se:StswApp`.
- [ ] `se:StswResources` is merged in app resources.
- [ ] `App` class inherits from `StswApp`.
- [ ] `MainWindow` root is `se:StswWindow`.
- [ ] `MainWindow` code-behind inherits from `StswWindow`.
- [ ] `StartupUri` points to the correct XAML file.
- [ ] Optional: global command(s), exception handler, theme/language defaults are configured.

---

## 7) Common mistakes

1. **Keeping `<Application>` instead of `<se:StswApp>`**
   - Result: missing StswExpress app-level functionality.

2. **Not merging `<se:StswResources/>`**
   - Result: styles/resources not applied correctly.

3. **Using `Window` instead of `StswWindow`**
   - Result: loss of StswExpress window features and visual consistency.

4. **Incorrect `StartupUri`**
   - Result: app starts with runtime XAML loading errors.

---

## 8) Minimal working startup structure

```text
YourApp.Wpf/
├─ App.xaml
├─ App.xaml.cs
└─ Modules/
   ├─ MainWindow.xaml
   └─ MainWindow.xaml.cs
```

If you follow this structure and configuration, your StswExpress.Wpf app should start correctly with theme resources, a custom window base, and centralized startup behavior.
