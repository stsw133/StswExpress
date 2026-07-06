# Getting Started

This guide walks you through setting up StswExpress in a minimal WPF project and using a basic component.

By the end, you will:

- Install a StswExpress package
- Create a simple ViewModel
- Use a StswExpress control in XAML

---

## 1. Choose a Package

Select the package that matches your project type:

- **StswExpress.Commons** – infrastructure and helpers
- **StswExpress.Wpf** – WPF controls and UI extensions
- **StswExpress.Avalonia** – Avalonia UI components

For this example, we’ll use **StswExpress.Wpf**.

---

## 2. Install the Package

Using .NET CLI:
`dotnet add package StswExpress.Wpf`

Or install via Visual Studio → Manage NuGet Packages.

---

## 3. Create a Basic ViewModel

Create a ViewModel using the Commons base class:

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

## 4. Use StswExpress in XAML

Add the namespace:
```
xmlns:se="clr-namespace:StswExpress.Wpf"
```
Example usage inside a Window:
```
<se:StswWindow>
    <StackPanel Margin="20">
        <TextBlock Text="{Binding Title}" FontSize="20"/>
        <se:StswButton Content="Click me"/>
    </StackPanel>
</se:StswWindow>
```

This demonstrates:
- Custom window system
- Styled controls
- MVVM-friendly binding

## 5. Explore Advanced Components

Now you can explore:
- `StswDataGrid` with filtering support
- Charts and visual controls
- Dialog and navigation system
- Theming and localization
- SQL helper utilities

Refer to the API Reference for full type documentation.

## 6. Next Steps

- Read Installation for private feed setup
- Review Changelog for recent updates
- Explore the full control ecosystem in API Reference
