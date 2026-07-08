# StswExpress

A modern WPF library for .NET 8.0, offering enhanced controls, tools, and customization options for building responsive and feature-rich desktop applications.

📖 Documentation:  
https://stsw133.github.io/StswExpress/

## Features

### Controls
StswExpress provides a wide range of controls, many inspired by **WinUI 3**, as well as WPF controls with enhanced styling and functionalities.
Here's a breakdown:

- **Buttons**: `StswButton`, `StswDropButton`, `StswHoldButton`, `StswHyperlinkButton`, `StswRadioButton`, `StswRepeatButton`, `StswSplitButton`, `StswToggleButton`
- **Charts**: `StswChartLegend`, `StswColumnChart`, `StswLineChart`, `StswPieChart`
- **Colors**: `StswColorBox`, `StswColorPicker`, `StswColorSelector`
- **DataGrids**: `StswDataGrid`, `StswDataGridFilterBox`, `StswDataPager`
- **Date & Time**: `StswCalendar`, `StswDatePicker`, `StswRangeCalendar`, `StswTimeline`, `StswTimePicker`, `StswTimerControl`
- **Dialogs**: `StswContentDialog`, `StswFileDialog`, `StswMessageDialog`
- **Filters**: `StswFilterTags`, `StswSearchBox`
- **Input**: `StswAdaptiveBox`, `StswCalculator`, `StswNumberBox`, `StswPasswordBox`, `StswRatingControl`, `StswRichBox`, `StswRichEditor`, `StswSlider`, `StswTextBox`
- **Layout**: `StswBorder`, `StswExpander`, `StswGroupBox`, `StswSeparator`, `StswTimedSwitch`, `StswToolBar`, `StswZoomControl`
- **Media**: `StswBarcode`, `StswGifImage`, `StswImage`, `StswMediaPlayer`
- **Navigation**: `StswMenu`, `StswNavigation`, `StswTabControl`
- **Panels**: `StswDynamicGrid`, `StswGrid`, `StswGridSplitter`, `StswSidePanel`
- **Paths**: `StswPathPicker`, `StswPathTree`
- **Scrollers**: `StswDirectionView`, `StswScrollBar`, `StswScrollView`
- **Selectors**: `StswComboBox`, `StswDragBox`, `StswFlipView`, `StswListBox`, `StswListView`, `StswSegment`, `StswSelectionBox`, `StswTreeView`
- **Status & Info**: `StswInfoBadge`, `StswInfoBar`, `StswInfoPanel`, `StswProgressBar`, `StswProgressRing`, `StswStatusBar`, `StswToaster`
- **Sub-controls**: `StswSubButton`, `StswSubCheck`, `StswSubDrop`, `StswSubError`, `StswSubLabel`, `StswSubRadio`, `StswSubRepeater`, `StswSubSelector`
- **Toggles**: `StswCheckBox`, `StswRadioBox`, `StswToggleSwitch`
- **Typography**: `StswEmojiText`, `StswHighlightText`, `StswIcon`, `StswLabel`, `StswOutlinedText`, `StswSpinner`, `StswText`
- **Windowing**: `StswContextMenu`, `StswNotifyIcon`, `StswPopup`, `StswToolTip`, `StswWindow`, `StswWindowBar`

### Utilities & Enhancements

The `Utils` namespace in StswExpress provides a comprehensive set of tools to simplify WPF development. Here's an overview of its main categories:

#### Bindings
Tools to extend and enhance WPF bindings, including utilities for proxying, triggering, and monitoring bindings.  
Examples: `StswBindingProxy`, `StswBindingTrigger`.

#### Collections
Custom collection types and utilities for advanced data handling, such as observable collections, enhanced views, and dictionaries.  
Examples: `StswObservableCollection`, `StswObservableDictionary`.

#### Commands
Command implementations for MVVM, supporting asynchronous, cancellable, and pausable commands.  
Examples: `StswAsyncCommand`, `StswCommand`.

#### Comparers
Custom comparers for advanced sorting and comparison needs, including natural string comparison.  
Example: `StswNaturalStringComparer`.

#### Converters
Value converters for common scenarios like boolean logic, color manipulation, formatting, and more.  
Examples: `StswBoolConverter`, `StswColorConverter`, `StswIfElseConverter`.

#### Databases
Helpers for working with databases, including connection factories, query helpers, and configuration tools.  
Examples: `StswDatabaseHelper`, `StswSqlConnectionFactory`.

#### Events
Classes and arguments for handling events, particularly for value change notifications.  
Example: `StswValueChangedEventArgs`.

#### Export
Utilities for exporting data with support for customization via attributes and parameters.  
Examples: `StswExport`, `StswExportAttribute`.

#### Helpers
General-purpose utilities for calculations, cloning, and object mapping.  
Examples: `StswCalculator`, `StswMapping`.

#### Logging
An integrated logging system for debugging and monitoring, with configurable logging behavior.  
Examples: `StswLog`, `StswLogConfig`.

#### Mailing
Email management tools for sending and handling emails directly from the application.  
Examples: `StswMailboxes`, `StswMailboxModel`.

#### Markup Extensions
Extensions to enhance XAML development, such as dynamic resource binding and enum-to-list conversion.  
Examples: `StswDynamicColorExtension`, `StswEnumToListExtension`.

#### Messengers
A lightweight messaging system for inter-component communication.  
Example: `StswMessanger`.

#### Stores
State management tools for handling application data and refresh operations.  
Examples: `StswStoreBase`, `StswRefreshBlocker`.

#### Translator
Localization tools to manage translations and bind them easily in XAML.  
Examples: `StswTranslator`, `StswTranslateExtension`.

#### Miscellaneous
A variety of utilities including extensions, icons, observable objects, and security tools.  
Examples: `StswExtensions`, `StswIcons`, `StswObservableObject`.

---

These tools provide essential building blocks for streamlining WPF application development, making it easier to build robust, scalable, and maintainable projects.

## Licensing & Acknowledgments

- **Icons**: The icons in `StswIcons` are sourced from [Material Design Icons](https://pictogrammers.com/library/mdi/).

## Get Started

You can find StswExpress on NuGet [here](https://www.nuget.org/packages/StswExpress).

For more details and the full changelog, click [here](https://karol-staszewski.github.io/StswExpress-docs/articles/changelog/index.md).
