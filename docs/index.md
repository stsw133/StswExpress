# StswExpress

**StswExpress** is a modular .NET library focused on building modern desktop applications faster — with consistent patterns, reusable components and a growing set of ready-to-use UI controls.

It is split into independent packages, so you can adopt only what you need.

---

## Packages

### StswExpress.Commons
Foundation layer with general-purpose utilities used across the ecosystem.

Core utilities and infrastructure layer:
- MVVM base classes
- SQL helpers
- mapping utilities
- converters, extensions, comparers
- logging & security helpers

### StswExpress.Wpf
WPF-focused controls, attached behaviors and UX building blocks.

Extended WPF control ecosystem:
- advanced DataGrid
- dialogs, navigation, notifications
- charts and visualization controls
- theming & localization

### StswExpress.Avalonia
Avalonia UI layer aligned with the same architectural concepts.

---

## Why StswExpress

- **Practical** – built out of real application needs, not “demo” widgets.
- **Consistent** – naming, behavior and patterns are aligned across packages.
- **Composable** – adopt Commons only, or add WPF/Avalonia on top.
- **MVVM-friendly** – designed to work cleanly with typical MVVM flows.
- **Documented** – API Reference is generated from XML docs, guides explain the rest.

---

# 🔥 Highlights

## 🧠 SQL Helpers (micro-ORM inspired)

Powerful query preparation utilities:

- `PrepareInsertQuery`
- `PrepareUpdateQuery`
- Bulk insert for `IEnumerable`
- `SqlParameterCollection` helpers
- Model mapping (`MapTo`)
- `ToDataTable` and reverse mapping

Designed to reduce boilerplate while keeping full SQL control.

---

## 📊 Advanced StswDataGrid Ecosystem

A heavily extended DataGrid with:

- custom column types (`Check`, `Combo`, `Date`, `Decimal`, `Path`, `Status`, etc.)
- built-in filtering system
- dynamic XAML refresh
- virtualization improvements
- tab drag & reordering
- automatic CollectionView handling

![StswDataGrid](assets/images/datagrid.png)

---

## 📈 Charts & Visualization Controls

Built-in visualization components:

- `StswColumnChart`
- `StswPieChart`
- `StswLineChart`
- `StswTimeline`
- `StswRangeCalendar`
- color pickers
- numeric controls
- media player & GIF support

![Charts](assets/images/charts.png)

---

## 🔳 QR Codes & Barcodes

Native support for:

- `StswBarcode`
- QR code generation

Perfect for ERP, POS, warehouse and document workflows.

![QR & Barcode](assets/images/qr-barcode.png)

---

## 🧩 Identifier-Based Control System

Structured control management through identifiers:

- `StswContentDialog`
- `StswTabControl`
- `StswNavigation`
- `StswNotifyIcon`
- `StswToaster`

Enables loosely coupled UI communication.

---

## 🎨 Themes & Localization

Out-of-the-box support for:

- Light theme
- Dark theme
- Additional themes
- Runtime theme switching
- `StswTranslator`
- Runtime language switching

![Themes](assets/images/themes.png)

---

## 🪟 Custom Window System

`StswWindow` provides:

- fullscreen support
- dynamic theme switching
- UI scaling
- runtime language switching
- extended window controls
- layout enhancements

![StswWindow](assets/images/window.png)

---

## 🔁 Rich Utility Layer

Includes:

- DataTable → IEnumerable mapping
- deep object mapping
- date range utilities
- math helpers (`Lerp`, `InverseLerp`)
- logging with archive support
- encryption & hashing helpers
- async command generators

---

# Getting started

1. **Install** the package you need:
   - `StswExpress.Commons` for shared helpers
   - `StswExpress.Wpf` for WPF controls
   - `StswExpress.Avalonia` for Avalonia layer

2. Continue with:
- **Guides → Getting started**
- **Guides → Installation**
- **API Reference** (auto-generated)

---

## Installation (quick)

> Pick the package that matches your project.

**Commons**

dotnet add package StswExpress.Commons


**WPF**

dotnet add package StswExpress.Wpf


**Avalonia**

dotnet add package StswExpress.Avalonia


---

## Documentation map

- **Guides**
  - *Getting started* – quickest path to first use
  - *Installation* – packages and requirements
  - *Changelog* – what changed between versions
- **API Reference**
  - namespaces, classes, methods, properties (generated)

---

## Versioning & compatibility

StswExpress follows **Semantic Versioning**:

- `MAJOR` – breaking changes
- `MINOR` – new features (backwards compatible)
- `PATCH` – bug fixes

Pre-releases use suffixes like `-preview.x` or `-rc.x`.

---

## Support and feedback

If something is unclear or you hit a bug:
- check **Changelog** for recent changes
- look up the type in **API Reference**
- report the issue / suggestion in the repository

---

## License

See the repository license information.