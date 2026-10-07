# Installation

StswExpress is split into multiple NuGet packages. Install only the packages required by your application.

## Target frameworks

The current projects in the repository target:

| Package | Target frameworks |
| --- | --- |
| `StswExpress.Commons` | `net8.0`, `netstandard2.0` |
| `StswExpress.Sql` | `net8.0`, `netstandard2.0` |
| `StswExpress.Wpf` | `net8.0-windows7.0` |
| `StswExpress.Wpf.Themes` | `net8.0-windows7.0` |
| `StswExpress.Analyzers` | `netstandard2.0` analyzer package |

For a new WPF application, target .NET 8 for Windows or a compatible newer Windows target framework.

## Install with the .NET CLI

### WPF controls

```bash
dotnet add package StswExpress.Wpf
```

### Shared utilities only

```bash
dotnet add package StswExpress.Commons
```

### SQL helpers

```bash
dotnet add package StswExpress.Sql
```

`StswExpress.Sql` references `StswExpress.Commons`.

### Additional WPF themes

```bash
dotnet add package StswExpress.Wpf.Themes
```

### Analyzers and source generators

```bash
dotnet add package StswExpress.Analyzers
```

`StswExpress.Wpf` already references the analyzer package, so WPF applications normally do not need to add it separately unless they have a specific reason to do so.

## Install with Visual Studio

1. Right-click the project.
2. Select **Manage NuGet Packages**.
3. Search for the required StswExpress package.
4. Select the desired version.
5. Click **Install**.

## WPF namespace

After installing `StswExpress.Wpf`, add the assembly-qualified namespace to your XAML root:

```xml
xmlns:se="clr-namespace:StswExpress.Wpf;assembly=StswExpress.Wpf"
```

## Basic C# namespace

Many shared types use the `StswExpress.Commons` namespace:

```csharp
using StswExpress.Commons;
```

This also applies to the current SQL helper types, even though they are distributed in the separate `StswExpress.Sql` package.

## Additional themes

After installing `StswExpress.Wpf.Themes`, register the additional themes during application startup:

```csharp
using StswExpress.Wpf.Themes;

StswThemeResources.Register();
```

For the full WPF application setup, see the [Startup tutorial](tutorials/startup.md).

## Versioning

The packages use semantic versioning-style versions. Check the [Changelog](changelog/index.md) before upgrading, especially when moving across releases that contain breaking API changes.
