# Getting Started

This guide shows the shortest path to using StswExpress in a WPF application.

By the end, you will:

- install `StswExpress.Wpf`,
- create a simple view model,
- reference the StswExpress WPF namespace,
- use StswExpress controls in XAML.

## 1. Install the WPF package

Using the .NET CLI:

```bash
dotnet add package StswExpress.Wpf
```

`StswExpress.Wpf` references `StswExpress.Commons`, so the shared core package is brought in as a dependency.

If you also need SQL helpers, install the separate SQL package:

```bash
dotnet add package StswExpress.Sql
```

Optional additional WPF themes are available in:

```bash
dotnet add package StswExpress.Wpf.Themes
```

## 2. Create a basic view model

`StswObservableObject` provides `INotifyPropertyChanged` support and the `SetProperty` helper:

```csharp
using StswExpress.Commons;

public class MainViewModel : StswObservableObject
{
    private string _title = "Hello from StswExpress";

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
}
```

## 3. Add the WPF namespace

In an application project that references the NuGet package, use:

```xml
xmlns:se="clr-namespace:StswExpress.Wpf;assembly=StswExpress.Wpf"
```

## 4. Use StswExpress controls

A minimal `StswWindow` can look like this:

```xml
<se:StswWindow x:Class="YourApp.MainWindow"
               xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
               xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
               xmlns:se="clr-namespace:StswExpress.Wpf;assembly=StswExpress.Wpf"
               Title="StswExpress example"
               Width="800"
               Height="500">
    <StackPanel Margin="20">
        <TextBlock Text="{Binding Title}" FontSize="20"/>
        <se:StswButton Content="Click me" Margin="0,12,0,0"/>
    </StackPanel>
</se:StswWindow>
```

For a complete application bootstrap, including `StswApp` and `StswResources`, continue with the [Startup tutorial](tutorials/startup.md).

## 5. Explore the main areas

Useful next steps include:

- [StswDataGrid](tutorials/data-grid.md) for advanced grids and filtering,
- [Identifier-based controls](tutorials/identifier-controls.md) for dialogs, navigation, tabs, tray notifications, and toasts,
- [StswDatabaseHelper](tutorials/database-helpers.md) for SQL access,
- [API Reference](../api/index.md) for generated type and member documentation.

## Next steps

- Read [Installation](installation.md) for package and target-framework details.
- Read the [Changelog](changelog/index.md) before upgrading between versions.
- Browse the generated [API Reference](../api/index.md) for exact signatures and XML documentation.
