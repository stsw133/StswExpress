# One year edition

**Release Date**: 2023-03-01

## New Functionality

- .NET version increased from 5 to 6.
- All controls and helping classes in library has been renamed and use "Stsw" prefix. Some of them have also completely changed names like **ExtDatePicker** -> **StswCalendarPicker**.
- Many controls have been completely reworked, those with most changes are: **StswColumnFilter** (original name: ColumnFilter), **StswHeader** (original name: Header), **LoadingCircle**, **StswProgressBar** (original name: ExtProgressBar), **StswWindow**.
- Completely new controls: **ClippingBorder** (to properly render content when CornerRadius is used), **StswNavigation**, **StswNavigationButton**, **StswPasswordBox**.
- New extension methods: **AsBytes**, **ToNullable**, **ToStswDictionary**, **TrimEnd** (with string as parameter), **TrimStart** (with string as parameter), **TryMultipleTimes** (that tries to execute an action or function multiple times with a specified interval between each try until it succeeds or reaches a maximum number of tries).
- New classes: **StswCollection** (extended version of ObservableCollection - it notifies even if single item's property has changed and can contain info about states of items), **StswDictionary** (have INotifyProperty implemented and can be binded in a way that creates new key if it did not existed yet), **StswExport** (for exporting data to Excel file using "ClosedXML" library), **StswMC** (meant to store mail configurations and sending mails), **StswRelayCommand** (have ICommand implemented and is meant to be binded to control command properties), **StswSimpleModel** (meant to be model for item sources in combo boxes).
- New converters: **conv\_Add** (that adds number from parameter to target number), **conv\_Calculate** (that uses DataTable's compute method), **conv\_ColorBrightness**, **conv\_IfElse** (that displays one of two texts based on bool result of comparing), **conv\_NullToUnset**, **conv\_Sum** (that allows to sum field in list of class elements).
- Collection (7000+) of vector icons in new **StswIcons** static class.
- **StswDataGrid** has new special column for showing and clearing filters and showing row details.
- **StswWindow** has now fullscreen mode.
- TextBox based controls have placeholder functionality.
- New **AppStart** method meant to be put on application startup. This method sets all starting configuration needed for application to work properly if it is using Stsw controls and some of helpers.
- New project for library testing called **TestApp**. This version of test app has 3 simple modules: Database, Contractors, LibraryTests.

## Changed Functionality

- Themes are now based on external library called "DynamicAero2". There are two themes: **Light** and **Dark**, with possibility to auto select one of them based on system theme.
- Many controls and functions have been adjusted to use them comfortably in MVVM.
- Controls are now built on templates in styles in resource dictionaries.
- Most of controls have "CornerRadius" property now.
- **VM** renamed into **StswObservableObject**. **StswObservableObject** have method called "SetProperty" that can be used in simple way in property setters.
- **StswDB** has been improved. Also the way **StswDB** and **StswMC** saves info into files is changed to keep every instance in one line.
- **StswLog** now works differently - before it was splitting every 5 MB into new file, now it groups logs per day of creation and makes a new file for each day.
- **MultiBox**'s functionality is merged into **StswComboBox** (when "SelectionMode" property is set to "Multiple").
- Converter **conv\_Contains** no longer works only for IEnumerable. Converter **conv\_Compare** has simplified functionality.
- Controls like **DataGridImage**, **ExtMenuItem** and **IconButton** have been removed. Methods like **AddCharBeforeUpperLetters** have been removed.
- Converter **conv\_Size** has been renamed to **conv\_Multiply**. Extension method named **ToList** has been renamed to **AsList**. Method extension **GetVisualChild** renamed to **FindVisualChild**.
- Method **ConvertTo** made into extension method. Method **LoadImage** made into extension method named **AsImage**.
- **BindingProxy**'s "Data" property renamed into "Proxy".
- **ColorSetter** renamed to **ColorPicker**.
- **StswNumericBox** accepts changes after pressing Enter key.
- **StswWindow** now uses WindowChrome (that also eliminated some bugs and weird behavior).
- **StswSecurity** class uses different algorithm for encrypting, decrypting and generating salt.

## Fixes

- Not listed since most of controls and helper classes have been reworked and reworks include many bugfixes.

---

# Re-edition

**Release Date**: 2022-03-01

### Controls:

- **ColorSetter** - in short a control made from 4 sliders and labels. Each slider represent RGBA color channels.
- **ColumnFilter** - in short a control that allows user to specify a value used for "where" clause in SQL commands. This control shows different box based on filter type and generates different text based on filter mode.
- **DataGridImageColumn** - a DataGrid's template column used for showing an image.
- **ExtCheckBox** - same as CheckBox but have different style for dark mode (poor version) and can display different icon depending on "IsError" property.
- **ExtComboBox** - same as ComboBox but have different style for dark mode (poor version).
- **ExtDataGrid** - same as DataGrid but have different style for dark mode (poor version) and have additional properties for header brushes.
- **ExtImage** - same as Image but have additional context menu that allows user to cut, copy, paste, delete, load and save an image.
- **ExtMenuItem** - same as MenuItem but have image source property and creates Image as Icon at start.
- **ExtProgressBar** - same as ProgressBar but have different style for dark mode (poor version) and can display text on progress bar.
- **ExtSeparator** - same as Separator but have different style for dark mode (poor version) and can be displayed either horizontally or vertically.
- **ExtTextBox** - same as TextBox. The only reason it exists is vertical content alignment centered by default.
- **ExtToggleButton** - same as ToggleButton but have different style for dark mode (poor version) and is specially customized for MultiBox.
- **Header** - in short a control made from 2 images (one bigger and one smaller) and 2 text blocks.
- **IconButton** - in short a button made from image and text block.
- **LoadingCircle** - a control that displays rotating circle made from multiple Ellipses.
- **MultiBox** - in short a control that allows to select multiple items.
- **NumericUpDown** - in short a control made from text box and two repeat buttons allowing user to increase or decrese numeric value binded to box.
- **OutlinedTextBlock** - same as TextBlock but contains some properties allowing it to have a stroke.
- **StswWindow** - extended window with options to scale interface (poor version) and switch themes between light and dark.

### Helpers:

- **BindingProxy** - helper class that allows creating a proxy object for data binding purposes.
- **Commands** - helper class that contains some routed UI commands including their input gestures.
- **Converters** - a collection of some converters such as: **conv\_Bool**, **conv\_Color**, **conv\_Compare**, **conv\_Contains**, **conv\_MultiCultureNumber**, **conv\_NotNull**, **conv\_Size**, **conv\_StringToString**.
- **DB** - helper class that is model for database connection and contains methods for import and export encrypted connection data to file.
- **Extensions** - a collection of some methods used like extensions such as: **Between**, **Capitalize**, **ConvertTo**, **In**, **ToList**, **GetVisualChild**, **FindVisualChildren**.
- **Fn** - a collection of some methods such as: **AppName**, **AppVersion**, **AppNameAndVersion**, **AppCopyright**, **AppDatabase**, **AddCharBeforeUpperLetters**, **LoadImage**, **OpenContextMenu**, **OpenFile**, **GetWindowsThemeColor**, **GetColumnFilters**, **ClearColumnFilters**, **SetTheme**.
- **Log** - helper class that allows to create log text entry in specified log file. After reaching a size limit, file is renamed and logs are saved to a new one.
- **Mail** - helpers class that is model for mailbox connection and contains method for sending mails.
- **SQL** - helper class to generate connection strings based on data from DB model class.
- **Security** - helper class that provides methods for encrypting and decrypting text and for generating salt and hash.
- **VM** - helper class implementing INotifyPropertyChanged interface that provides a method for invoking PropertyChanged.

### Other:

- **Resources** - a collection of filter icons.
- **Themes** - a collection of styles for some controls to provide them with light and dark theme look (poor version).
