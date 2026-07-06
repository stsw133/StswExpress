# Identifier-Based Controls in StswExpress (WPF)

> Practical guide for correctly wiring controls that depend on `Identifier`.

## Screenshot placeholders (add your images here)

### Configured Navigation example

![Configured StswNavigation - add your screenshot path here](./images/stswnavigation-configured.png)

### Configured Dialog host example

![Configured StswContentDialog - add your screenshot path here](./images/stswcontentdialog-configured.png)

---

## Why `Identifier` matters

Many StswExpress controls expose an `Identifier` property and static methods like `Show(...)`, `Close(...)`, `Navigate(...)`, or `ShowBalloonTip(...)`. Those static APIs locate the target control instance by matching the `Identifier` value.

If the identifier is missing, wrong, or duplicated, the call may fail (or match the wrong instance).

---

## Core rule

**Treat `Identifier` as a unique routing key for a specific host control instance.**

Recommended conventions:

- Use predictable, explicit values (e.g. `"ShellNavigation"`, `"MainDialogHost"`, `"TrayIcon.Main"`).
- Keep identifiers unique per window/scope.
- When you have multiple windows of the same type, include context in names:
  - `"CustomerWindow.Navigation"`
  - `"AdminWindow.Navigation"`
- Prefer constants or `nameof(...)`-based patterns in C# to avoid typo bugs.

---

## Quick map: controls that use `Identifier`

Main controls from your list:

- `StswContentDialog`
- `StswMessageDialog`
- `StswNavigation`
- `StswTabControl`
- `StswNotifyIcon`

Other frequently used Identifier-based controls/helpers in the library:

- `StswToaster`
- `StswFileDialog`
- internal `StswConfig` helper (used by config presentation patterns)

---

## 1) `StswContentDialog` (dialog host)

`StswContentDialog` is usually a host placed in your visual tree, then addressed through static methods:

- `StswContentDialog.Show(content, dialogIdentifier)`
- `StswContentDialog.Close(dialogIdentifier[, parameter])`

### XAML host

```xml
<se:StswContentDialog Identifier="MainDialogHost" />
```

### Show from code-behind / VM service layer

```csharp
await StswContentDialog.Show(new MyDialogContent(), "MainDialogHost");
```

### Good practices

- Place the host where it is always available for the scope you need.
- Use one host per logical area/window unless you explicitly need multiple hosts.
- If you use multiple hosts, document which workflow uses which identifier.

---

## 2) `StswMessageDialog` (message box-like dialog)

`StswMessageDialog` uses the same idea: it resolves where to display by `Identifier`.

### Typical usage

```csharp
var result = await StswMessageDialog.Show(
    new StswMessageDialog
    {
        Identifier = "MainDialogHost",
        Title = "Delete item",
        Message = "Are you sure?"
    });
```

### Best practices

- Reuse the same dialog host identifier across your app-level message dialogs.
- Centralize message-dialog calls in a service to keep identifiers consistent.

---

## 3) `StswNavigation` (navigate by identifier)

`StswNavigation` supports static navigation targeting a specific instance by `Identifier`.

### XAML

```xml
<se:StswNavigation Identifier="ShellNavigation" />
```

### Code

```csharp
StswNavigation.Navigate("ShellNavigation", typeof(DashboardView));
```

### Best practices

- Assign one stable identifier to your shell navigation control.
- For multi-shell/multi-window apps, namespace identifiers by shell.
- Keep navigation calls in one place (NavigationService) to avoid scattered string literals.

---

## 4) `StswTabControl` (tab operations by identifier)

`StswTabControl` can be targeted via static operations that search by identifier.

### XAML

```xml
<se:StswTabControl Identifier="MainTabs" />
```

### Example intent

```csharp
// Pseudocode-style example: call the static API that targets MainTabs
// StswTabControl.<Operation>("MainTabs", ...);
```

### Best practices

- Keep tab-related operations behind an abstraction (e.g., `ITabWorkspaceService`).
- Use one identifier per tab workspace (main tabs, tool tabs, report tabs, etc.).

---

## 5) `StswNotifyIcon` (tray icon instance lookup)

`StswNotifyIcon` also supports identifier-based static operations (e.g., showing notifications on a specific tray icon instance).

### XAML

```xml
<se:StswNotifyIcon Identifier="TrayIcon.Main" />
```

### Example

```csharp
StswNotifyIcon.ShowBalloonTip(
    "TrayIcon.Main",
    "Sync complete",
    "All files are up to date.");
```

### Best practices

- Always set explicit identifiers if there can be more than one notify icon.
- Keep tray notification calls centralized.

---

## Additional controls worth remembering

## `StswToaster`

If you use multiple toaster regions, give each a unique identifier and target the correct one in static calls.

## `StswFileDialog`

When shown through framework helpers, the identifier determines which host instance is used.

---

## Common mistakes and how to avoid them

1. **No `Identifier` set**
   - Symptom: static call fails to find control instance.
   - Fix: set identifier in XAML and verify control is loaded.

2. **Duplicate identifiers**
   - Symptom: ambiguous match / runtime exception.
   - Fix: enforce unique naming scheme.

3. **Typo in string literal**
   - Symptom: “not found” despite control existing.
   - Fix: use constants (`public const string MainDialogHost = ...`).

4. **Wrong scope/window**
   - Symptom: call hits another window or none.
   - Fix: include window/module context in identifier names.

5. **Calling too early (before load)**
   - Symptom: control instance not available yet.
   - Fix: call after UI initialization, or defer via dispatcher/app lifecycle.

---

## Recommended implementation pattern

Create a central static class for identifiers:

```csharp
public static class UiIdentifiers
{
    public const string MainDialogHost = "MainDialogHost";
    public const string ShellNavigation = "ShellNavigation";
    public const string MainTabs = "MainTabs";
    public const string MainTrayIcon = "TrayIcon.Main";
}
```

Then use it consistently:

```xml
<se:StswContentDialog Identifier="{x:Static local:UiIdentifiers.MainDialogHost}" />
<se:StswNavigation Identifier="{x:Static local:UiIdentifiers.ShellNavigation}" />
<se:StswTabControl Identifier="{x:Static local:UiIdentifiers.MainTabs}" />
<se:StswNotifyIcon Identifier="{x:Static local:UiIdentifiers.MainTrayIcon}" />
```

```csharp
await StswContentDialog.Show(new MyDialogContent(), UiIdentifiers.MainDialogHost);
StswNavigation.Navigate(UiIdentifiers.ShellNavigation, typeof(HomeView));
```

---

## Minimal checklist

Before shipping, confirm:

- [ ] Every Identifier-based control has an explicit `Identifier`.
- [ ] Identifiers are unique in the relevant runtime scope.
- [ ] Calls to static APIs use shared constants (not random literals).
- [ ] Multi-window scenarios have scoped naming.
- [ ] The targeted control is loaded before static calls are made.

---

## Summary

`Identifier` in StswExpress is your addressing mechanism for UI hosts and services exposed via static APIs. Consistent naming, uniqueness, and centralized usage patterns eliminate most runtime errors and make dialog/navigation workflows predictable.
