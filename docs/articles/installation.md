# Installation

This guide explains how to install and configure StswExpress in your project.

---

## Requirements

- .NET 8.0 or newer
- For WPF: Windows desktop project targeting `net8.0-windows`
- For Avalonia: Avalonia project targeting .NET 8

---

## NuGet Installation

### Using .NET CLI

Install the package that matches your project:

### StswExpress.Commons


dotnet add package StswExpress.Commons


### StswExpress.Wpf


dotnet add package StswExpress.Wpf


### StswExpress.Avalonia


dotnet add package StswExpress.Avalonia


---

### Using Visual Studio

1. Right-click your project.
2. Select **Manage NuGet Packages**.
3. Add the configured StswExpress package source (if using a private feed).
4. Search for the desired package.
5. Click **Install**.

---

## Private Package Source (Commercial Version)

If StswExpress is distributed through a private NuGet feed:

1. Open **Tools → NuGet Package Manager → Package Sources**
2. Add a new source:
   - Name: `StswExpress`
   - Source: `<YOUR_PRIVATE_FEED_URL>`
3. Authenticate using your provided credentials or token.

After adding the source, install packages as usual.

---

## Basic Usage Example

After installation, you can reference StswExpress types directly in your code:

```csharp
using StswExpress.Commons;

public class MainViewModel : StswObservableObject
{
}
```
For WPF:
```
xmlns:se="clr-namespace:StswExpress.Wpf"
```
Refer to **Getting Started** for a full walkthrough.

---

## Versioning

StswExpress follows Semantic Versioning:
- `MAJOR` – breaking changes
- `MINOR` – new features (backwards compatible)
- `PATCH` – bug fixes

Pre-release versions use suffixes such as:
- `-preview.x`
- `-rc.x`

Only active subscribers receive access to new releases.
