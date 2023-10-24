# **0.3.1**
2023-10-24

### Bugfixes:
* **StswDataGrid**'s special column has proper margins again.

# ﻿**0.3.0**
2023-10-23

### New functionality:

* New translator mechanism with two builded languages: **en** and **pl**, with possibility to auto select one of them based on system language.
* New controls: **StswAdaptiveBox**, **StswConfig** (internal), **StswTreeView**.
* New extension method: **IsNumericType**.
* New methods: **IsValidEmail**, **IsValidPhoneNumber**, **IsValidUrl**, **MoveToRecycleBin**, **SerializeToJson**, **DeserializeFromJson**.
* **StswResources** have new event named "ThemeChanged" so it is now possible to make or change resources based on theme.

### Changed functionality:

* **StswCalendar** had mini-rework that should simplify its code with template and overall make it works better. **StswCalendar** also has new button to select today's date.
* **StswDropButton**, **StswSplitButton**, **StswComboBox**, **StswSelectionBox** and all component buttons** have simplified template. **StswDataGrid** is finally using its own template. **StswDropButton** and **StswSplitButton** are derived from ItemsControl class.
* "IsTabStop" property is set to false in propably all places in Stsw controls where it is needed.
* **StswClippingBorder** has new property "DoClipping" that tells if should clip child's radius and rect.
* **StswNumericBox** uses a decimal typed "Value" property instead of double typed "Value" property - it should fix the problem with odd numbers and precision.
* **StswTextEditor** has a little improved template and code behind.
* **StswContentDialog** uses "Background" property instead of "OverlayBackground" and its background have more opacity.
* **StswMessageDialog** now have colored border and header's background based on its image. **StswMessageDialog** also has new Image type: **Debug**.
* **StswLogPanel** has been improved.
* **StswWindow** no longer has option in menu to center window on screen. This option is now merged with option to set window size to default.
* **Pink** theme changes font family and font size and **Dark** theme is a little lighter now (inverted color from **Light** theme and added 10% missing brightness excluding colored brushes). Every theme has changes for colored brushes (**StswMessageDialog**, **StswLogPanel**, **StswProgessBar**).
* **StswApp** is hidden from EditorBrowsable and no longer have "OpenHelp" method.
* Input controls and **StswProgressBar** are now stretched by default instead of topped.
* All placeholders in Stsw controls are **StswText** control now instead of TextBlock. **StswFilter** uses **StswAdaptiveBox** in its template now.
* **StswComponentRebutton** has been renamed into **StswComponentRepeater**. **IStswSelectionItem** has been renamed to **IStswSelection**. **IStswCollectionItem** has property renamed from "ErrorMessage" to "ItemMessage". "PopupBorderThickness" property has been renamed to "PopupThickness" in all Stsw controls that use it.
* "MaxDropDownHeight" property's default value increased from 120 to 140 in all Stsw controls that use it.
* Debug messages (**StswMessageDialog**, **StswLogPanel**) has different icon with original one removed completely. **StswDataGrid**'s items with filled "ItemMessage" property will show information icon in its special column.
* Changes in **StswSecurity**.

### Bugfixes:

* **StswPopup** properly binds DataContext now.
* **StswToggleSwitch** is properly colored now.
* **StswHeader** uses **StswClippingBorder** without clipping now so it should fix some problems like cut off month names in **StswCalendar**.
* Bugfix for binding to **StswDatabaseModel**'s properties.
* **StswExportParameters**'s "ExcludeNonAttributes" property renamed to "IncludeNonAttributed".

# **0.2.1**
2023-10-02

### Changed functionality:

* Changed named of extension from **ToObjectList** to **MapTo**.

### Bugfixes:

* Extension named **MapTo** properly maps DataTable's columns to generic type properties.
* **StswDataGrid** is no longer overriding "FrozenColumnCount" property.
* **StswDropButton** properly binds its separator's brush.

**0.2.0**
2023-10-01

### New functionality:

* New controls: **StswToggleSwitch**, **StswListView**, **StswContentDialog** (is listed here because was completely remade), **StswMessageDialog**, **StswSidePanel**, **StswZoomControl**, **StswPager**, **StswShifter**, **StswPopup**.
* New class **StswDynamicResource** that allows to use dynamic resource in XAML with converter.
* New extension **ConvertTo** that converts and returns object to generic type.
* New global command that can be invoked on F1 if method named "OpenHelp" have defined value.
* New theme: **Pink**.

### Changed functionality:

* Two main dictionaries (Generic and Theme) are merged into one named **StswResources**.
* Extension named **ToObjectList** has been improved.
* **StswWindow** no longer have sub title functionality and have default height and default width as dependency properties.
* **StswDataGrid** has default header brushes now.
* **StswProgressBar** can use custom Text now and have new state named "Finished".
* **StswLogPanel** now can have closable items if one of its property is set to true.
* **StswRatingControl** can have minimum value of 1 on click if new property "CanReset" is set to false.
* Method named **ExportToExcel** can receive additional properties as parameter such as including properties without export attribute.
* Numeric Stsw converters can receive "CornerRadius" and "Thickness" as value now.
* All Stsw controls that were using sub-borders use **StswSeparator** instead of **Border**. They also use different property to set its thickness: "SeparatorThickness" that is a double instead of "SubBorderThickness" that was Thickness. It is important to notice that new property requires half of previous value.
* **StswTabControl** and **StswTextEditor** both use **StswShifter** instead of **StswScrollViewer** (former on tab panel and latter on toolbar).
* **StswTextEditor** have new property for changing toolbar content from full to compact (with less options) and even collapsing it.
* All Stsw controls in read only state have different background and foreground brushes.
* All Stsw controls have defined horizontal and vertical alignments in style.
* Stsw check controls no longer have different FocusVisualStyle when they have content.
* Base classes for Stsw controls have changed from **UserControl** into either **Control** or **ContentControl**.
* **StswTruncateLabel** uses **StswPopup** in its template.
* Fullscreen binding has been moved from **StswApp** to **StswWindow**.
* **StswSettings** renamed into **Settings** (is internal anyway).
* Some brush properties in Stsw controls have different dynamic resources as default value.

### Bugfixes:

* Stsw input controls no longer update their main property on lost focus if property didn't changed (main property is for example "SelectedColor" for StswColorBox).
* **StswTextEditor**'s "FilePath" property loads or clears document if its value is changed.
* **StswWindow**'s title bar has fixed height of 30, simplier button style and chrome updating and no longer have border under.
* **StswRatingControl**'s placeholder calculation works properly on stretched alignment now.
* **StswDropButton**, **StswSplitButton**, **StswComboBox** and **StswSelectionBox** have default "MaxDropDownHeight".
* **StswFilter** will no longer have top margin if it does not have header.

# **0.1.1**
2023-09-18

### New functionality:

* **StswSelectionBox** uses new property "SetTextCommand" to set custom method for generating text in box after selection changing.
* **StswFilter** can now hide mode changing button with new property.
* **StswApp** have new method called "StswWindow". This method will return current app's main window as StswWindow.

### Bugfixes:

* Content dialog is declared properly now for **StswWindow**. This eliminates XAML binding failure caused by previous version.
* **StswSelectionBox** uses scroll viewer from its list box instead of separated scroll viewer - thanks to that scroll viewer works properly and can for example be scrolled with mouse wheel.
* **StswCheckBox** and **StswRadioBox** have the same default value for padding as other controls by cost of default icon scale (this "cost" is required so they have the same height as other controls like - for example - box controls).
* **StswNotifyIcon**'s "IconFromPath" method gets resource stream from relative or absolute Uri now instead of absolute.
* **StswIcon** uses "Data" property as content property and **StswText** uses "Inlines" property as content property.
* **StswDataGrid**'s special column no longer can be reordered and resized.
* **StswContentDialog**'s "IsOpen" property binds two way by default now.
* **StswColorBox**, **StswDatePicker** and **StswNumericBox** have bindable Text property again.
* **StswDropButton**, **StswSplitButton** and **StswComboBox** can now be properly aligned.
* Arrow icon in Stsw controls with drop-down list are now aligned to right when control is in stretched alignment mode.
* Stsw component controls release space for icon if icon is null.

# **0.1.0**
2023-08-19

Info not prepared yet...

# **One year edition**
2023-03-01

### New functionality:

* .NET version increased from 5 to 6.
* All controls and helping classes in library has been renamed and use "Stsw" prefix. Some of them have also completely changed names like **ExtDatePicker** -> **StswCalendarPicker**.
* Many controls have been completely reworked, those with most changes are: **StswColumnFilter** (original name: ColumnFilter), **StswHeader** (original name: Header), **LoadingCircle**, **StswProgressBar** (original name: ExtProgressBar), **StswWindow**.
* Completely new controls: **ClippingBorder** (to properly render content when CornerRadius is used), **StswNavigation**, **StswNavigationButton**, **StswPasswordBox**.
* New extension methods: **AsBytes**, **ToNullable**, **ToStswDictionary**, **TrimEnd** (with string as parameter), **TrimStart** (with string as parameter), **TryMultipleTimes** (that tries to execute an action or function multiple times with a specified interval between each try until it succeeds or reaches a maximum number of tries).
* New classes: **StswCollection** (extended version of ObservableCollection - it notifies even if single item's property has changed and can contain info about states of items), **StswDictionary** (have INotifyProperty implemented and can be binded in a way that creates new key if it did not existed yet), **StswExport** (for exporting data to Excel file using "ClosedXML" library), **StswMC** (meant to store mail configurations and sending mails), **StswRelayCommand** (have ICommand implemented and is meant to be binded to control command properties), **StswSimpleModel** (meant to be model for item sources in combo boxes).
* New converters: **conv\_Add** (that adds number from parameter to target number), **conv\_Calculate** (that uses DataTable's compute method), **conv\_ColorBrightness**, **conv\_IfElse** (that displays one of two texts based on bool result of comparing), **conv\_NullToUnset**, **conv\_Sum** (that allows to sum field in list of class elements).
* Collection (7000+) of vector icons in new **StswIcons** static class.
* **StswDataGrid** has new special column for showing and clearing filters and showing row details.
* **StswWindow** has now fullscreen mode.
* TextBox based controls have placeholder functionality.
* New **AppStart** method meant to be put on application startup. This method sets all starting configuration needed for application to work properly if it is using Stsw controls and some of helpers.
* New project for library testing called **TestApp**. This version of test app has 3 simple modules: Database, Contractors, LibraryTests.

### Changed functionality:

* Themes are now based on external library called "DynamicAero2". There are two themes: **Light** and **Dark**, with possibility to auto select one of them based on system theme.
* Many controls and functions have been adjusted to use them comfortably in MVVM.
* Controls are now built on templates in styles in resource dictionaries.
* Most of controls have "CornerRadius" property now.
* **VM** renamed into **StswObservableObject**. **StswObservableObject** have method called "SetProperty" that can be used in simple way in property setters.
* **StswDB** has been improved. Also the way **StswDB** and **StswMC** saves info into files is changed to keep every instance in one line.
* **StswLog** now works differently - before it was splitting every 5 MB into new file, now it groups logs per day of creation and makes a new file for each day.
* **MultiBox**'s functionality** is merged into **StswComboBox** (when "SelectionMode" property is set to "Multiple").
* Converter **conv\_Contains** no longer works only for IEnumerable. Converter **conv\_Compare** has simplified functionality.
* Controls like **DataGridImage**, **ExtMenuItem** and **IconButton** have been removed. Methods like **AddCharBeforeUpperLetters** have been removed.
* Converter **conv\_Size** has been renamed to **conv\_Multiply**. Extension method named **ToList** has been renamed to **AsList**. Method extension **GetVisualChild** renamed to **FindVisualChild**.
* Method **ConvertTo** made into extension method. Method **LoadImage** made into extension method named **AsImage**.
* **BindingProxy**'s "Data" property renamed into "Proxy".
* **ColorSetter** renamed to **ColorPicker**.
* **StswNumericBox** accepts changes after pressing Enter key.
* **StswWindow** now uses WindowChrome (that also eliminated some bugs and weird behavior).
* **StswSecurity** class uses different algorithm for encrypting, decrypting and generating salt.

### Bugfixes:

* Not listed since most of controls and helper classes have been reworked and reworks include many bugfixes.

# **Re-edition**
2022-03-01

### New controls:

* **ColorSetter** - in short a control made from 4 sliders and labels. Each slider represent RGBA color channels.
* **ColumnFilter** - in short a control that allows user to specify a value used for "where" clause in SQL commands. This control shows different box based on filter type and generates different text based on filter mode.
* **DataGridImageColumn** - a DataGrid's template column used for showing an image.
* **ExtCheckBox** - same as CheckBox but have different style for dark mode (poor version) and can display different icon depending on "IsError" property.
* **ExtComboBox** - same as ComboBox but have different style for dark mode (poor version).
* **ExtDataGrid** - same as DataGrid but have different style for dark mode (poor version) and have additional properties for header brushes.
* **ExtImage** - same as Image but have additional context menu that allows user to cut, copy, paste, delete, load and save an image.
* **ExtMenuItem** - same as MenuItem but have image source property and creates Image as Icon at start.
* **ExtProgressBar** - same as ProgressBar but have different style for dark mode (poor version) and can display text on progress bar.
* **ExtSeparator** - same as Separator but have different style for dark mode (poor version) and can be displayed either horizontally or vertically.
* **ExtTextBox** - same as TextBox. The only reason it exists is vertical content alignment centered by default.
* **ExtToggleButton** - same as ToggleButton but have different style for dark mode (poor version) and is specially customized for MultiBox.
* **Header** - in short a control made from 2 images (one bigger and one smaller) and 2 text blocks.
* **IconButton** - in short a button made from image and text block.
* **LoadingCircle** - a control that displays rotating circle made from multiple Ellipses.
* **MultiBox** - in short a control that allows to select multiple items.
* **NumericUpDown** - in short a control made from text box and two repeat buttons allowing user to increase or decrese numeric value binded to box.
* **OutlinedTextBlock** - same as TextBlock but contains some properties allowing it to have a stroke.
* **StswWindow** - extended window with options to scale interface (poor version) and switch themes between light and dark.

### New helpers:

* **BindingProxy** - helper class that allows creating a proxy object for data binding purposes.
* **Commands** - helper class that contains some routed UI commands including their input gestures.
* **Converters** - a collection of some converters such as: **conv\_Bool**, **conv\_Color**, **conv\_Compare**, **conv\_Contains**, **conv\_MultiCultureNumber**, **conv\_NotNull**, **conv\_Size**, **conv\_StringToString**.
* **DB** - helper class that is model for database connection and contains methods for import and export encrypted connection data to file.
* **Extensions** - a collection of some methods used like extensions such as: **Between**, **Capitalize**, **ConvertTo**, **In**, **ToList**, **GetVisualChild**, **FindVisualChildren**.
* **Fn** - a collection of some methods such as: **AppName**, **AppVersion**, **AppNameAndVersion**, **AppCopyright**, **AppDatabase**, **AddCharBeforeUpperLetters**, **LoadImage**, **OpenContextMenu**, **OpenFile**, **GetWindowsThemeColor**, **GetColumnFilters**, **ClearColumnFilters**, **SetTheme**.
* **Log** - helper class that allows to create log text entry in specified log file. After reaching a size limit, file is renamed and logs are saved to a new one.
* **Mail** - helpers class that is model for mailbox connection and contains method for sending mails.
* **SQL** - helper class to generate connection strings based on data from DB model class.
* **Security** - helper class that provides methods for encrypting and decrypting text and for generating salt and hash.
* **VM** - helper class implementing INotifyPropertyChanged interface that provides a method for invoking PropertyChanged.

Other:

* **Resources** - a collection of filter icons.
* **Themes** - a collection of styles for some controls to provide them with light and dark theme look (poor version).
