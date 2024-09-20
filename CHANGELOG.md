# **0.11.0**
2024-09-19

### New functionality:
* New extension method: **ToImageSource** (from Geometry).
* New dynamic function in **StswFn** named **MergeObjects** (for more comfortable **StswDatabaseHelper** methods using).
* **StswDatabaseHelper**'s methods will exclude all properties from parameter model that have no equivalent parameters in query.
* Prototype "Spring" theme.

### Changed functionality:
* StswDataGrid's special column moved to separated class named **StswDataGridStatusColumn**. **StswDataGrid** no longer has "SpecialColumnVisibility" property and column needs to be added manually.
* Global command **ClearText** renamed to **Clear** and now supports reseting selected items.
* **StswHeader** removed.
* Added "FrameworkPropertyMetadataOptions" (to handle affecting render, measure, arrange) to properties in Stsw controls.
* Small changes in **StswConfig**.
* A few optimisations.

### Bugfixes:
* **StswMailboxex** renamed to **StswMailboxes**.
* **StswAdaptiveBox** has been remade to fix bug with binding "SubControls" property.
* Bugfix for **StswNumberBox**'s "TryParse" method by changing invariant culture to current culture.
* Bugfix for **StswFn**'s "AppVersion" method because it was cutting out all parts with only 0.
* Bugfix for **StswTabControl** placement style.
* Bugfix for creating directories when **StswLog** is used.

# **0.10.0**
2024-09-03

### New functionality:
* New controls: **StswTimerControl**, **StswLabelPanel**.
* New adorner: **StswRippleAdorner**. It is used to enable a new click effect to most Stsw controls, especially the buttons (they already have it enabled by default) and can be set through **StswControl**'s "EnableRippleEffect" attached property.
* New classes: **StswNaturalStringComparer** (to provide a natural string comparison that sorts string in a "human-expected" order), **StswStoreBase** (base model for stores), **StswStoreChangedArgs** (for stores).
* **StswDatabases** has new config property where all configuration has been moved (**StswDatabaseConfig** supports auto disposing connections, setting delimiters for mappings, changing file path for importing and exporting lists, enabling less space queries, returning if designer mode).
* **StswMailboxes** has new config property where all configuration has been moved (**StswMailboxesConfig** supports global email address for debug mode, disabling mail sending, changing file path for importing and exporting lists).
* **StswExport** has new method for exporting IXLWorksheet to html code string named "ExportToHtml".
* New extensions and functions: **ToDictionarySafely** (converts a collection to a dictionary, safely handling duplicate keys by ignoring subsequent duplicates), **Next** (finds the next occurrence of a specified day of the week after a given date), **GetDescription** (retrieves the description attribute from an enum value, if present), **Batch** (splits a collection into smaller batches of the specified size), **Shuffle** (randomizes the order of elements in a list), **sSimilarTo** (simple comparing the string representations of property values), **FindLogicalChild** (finds the first logical child of a specific type for the given control).

### Changed functionality:
* **StswHeader** renamed into **StswLabel**. For backward compatibility, it exists with two different names in this version, but older one is marked as obsolete.
* **StswSubHeader** renamed into **StswSubLabel**.
* **StswMessageDialog** has a little changed template (to make details button look better).
* **StswCancellableAsyncCommand** no longer have action on cancel as parameter.
* **StswShiftButton** remade into **StswShiftSelector**.
* **StswDropArrow**'s icon can be changed now. Additionaly its visibility in other controls from now on is controled only by **StswControl**'s "IsArrowless" attached property and no longer controled by dynamic resource named "StswDropArrow.Visibility".
* **StswComboBox** implements **IStswSelectionControl** interface.
* **StswDatabaseHelper**'s "Set" method has been reworked.
* Importing and exporting lists in **StswDatabases**, **StswMailboxes** has been reworked. It works with encrypted json now and can work asynchronously. **StswLog** also has async methods now and can auto disable logging if there was enough failed attempts.
* Removed extensions and functions: **Remove** (for IEnumerable), **SerializeToJson**, **DeserializeFromJson**.
* Changed extensions and functions: **ToHex** (it always returns color in hex - before it could return name of color), **Between** (reversed order can be allowed), **ShiftIndexBy** renamed into **ShiftBy**, **GetProcessUser** renamed into **GetUser** (and made into extension).
* **ParametersAddList** method is limited to max. 20 elements.

### Bugfixes:
* **IsInDebug** method should better determine if entry assembly was built in debug mode.
* Stsw sub controls also have nullable "IsBusy" property to allow them to automatically bind to command's "IsBusy" property (similar to what **StswLabel** has).
* Bugfix for archiving logs.

# **0.9.3**
2024-08-14

### New functionality:
* New global command: **SelectAll**.
* **StswLog** has been extended (can write async and handle exceptions with defined action).

### Changed functionality:
* **StswDatabaseHelper**'s methods support transactions now, and are directly used on **SqlConnection** instead of **StswDatabaseModel**.
* **StswDatabaseModel** simplified and no longer have methods to handle transactions.
* **FindVisual**[...] methods can be used now to find interfaces and classes.
* **TryMultipleTimes** is now function method instead of extension method.
* **Evaluate** function method renamed into **Compute**.
* **ColorFromAhsv** made into **ColorFromHsv**'s overload.
  **ColorFromAhsl** made into **ColorFromHsl**'s overload.

### Bugfixes:
* **StswDatabaseHelper** and **StswSqlConnectionFactory** reworked to fix many bugs.

# **0.9.2**
2024-08-11

### New functionality:

* New command classes: **StswCancellableAsyncCommand** and **StswPausableAsyncCommand**.
* New class: **StswMessanger** (to support communication between ViewModels).
* New extension method: **ModifyEach** (it is basically **ForEach** from "System.Linq" but it can return a modified list so it can be helpful with query methods from **StswDatabaseModel**).

### Changed functionality:

* **StswHeader** can automatically bind "IsBusy" property also if its TempatedParent is ICommandSource.
* **StswDatabaseModel** query methods can also receive Dictionary type value as parameters now.
* **IsListType** extension method no longer treats string as list type.

### Bugfixes:

* A lot of bugfixes for **StswDatabaseModel** (and its query methods that are now in **StswDatabaseHelper**).
* Bugfix for **StswHeader** - it will set "IsBusy" property only if there is no default value set.
* Bugfix to **ConvertTo** extension method so it can properly assign null value.
* Bugfix for **MapTo** extension method (one with delimiter parameter) so it can again map system types including object type. This way it can support query method **Get** mapped to **StswComboItem**.

# **0.9.1**
2024-08-08

### New functionality:

* New class: **StswCollectionView** (to support using **StswBindingList** with **ICollectionView**).
* **StswHeader** will automatically bind "IsBusy" if its parent is any Button and **IStswCommand** is assigned to it.
* New function: **PrintFile**.

### Changed functionality:

* **StswDatabaseModel**'s query methods from now on can receive timeout as parameter.
* Changed logo for StswExpress NuGet package.

### Bugfixes:

* All Stsw collection controls will properly set "UsesSelectionItems" property when their items source is ICollectionView. Just in case, setter for this property is also public now.
* Extension **ParametersAddList** should properly handle null values and byte arrays now.

# **0.9.0**
2024-08-01

### New functionality:

* New converters: **StswCalculateConverter** (it replaces 3 other numeric converters: **StswAddConverter**, **StswModuloConverter** and **StswMultiplyConverter**).
* New extension methods: **AddIfNotContains**, **DeepClone**, **GetInnermostException**, **GetPropertyValue**, **InferSqlDbType**, **IsInDebug**, **IsNullOrDefault**, **ParametersAddList**, **Replace** (for IList), **ToDataTable**, **ToUnixTimeSeconds**.
* New function methods: **Evaluate** (it replaces **TryCalculateString**), **GetUniqueMonthsFromRange**, **IsFileInUse**.
* New markup extensions: **StswBindableParameter**.
* **StswDatabaseModel** has been extended to support executing SQL queries.
* New extension method overload for **MapTo** that supports nested class property mappings.
* New info type: **Fatal**.
* New class for Store purposes named **StswRefreshBlocker**.
* New static class for global commands (right now it contains 1 command for clearing text in boxes).
* Possibility to run **StswConfig** in window instead of content dialog.

### Changed functionality:

* **StswNumericBox** reworked into **StswDecimalBox** using "INumber" interface. You can make **StswDoubleBox** or **StswIntegerBox** if needed by inheriting from **StswNumberBoxBase**.
* Extended **StswLog** functionalities (for example auto-archiving).
* Extended functionalities for **StswObservableObject**.
* Extended functionalities for **StswSecurity**.
* **StswExcelFn** has been renamed into **StswExport** and its functionalities has been extended. Now it also contains method for importing data from Excel and supports exporting and importing for a few new formats: CSV, JSON and XML.
* Extended functionalities for **StswInfoBadge**, **StswInfoBar** and **StswInfoPanel** (for example bars can be minimized and its content copied).
* **StswMailboxModel** supports default values for "BCC" and for "ReplyTo".
* **StswWindowBar** is fully visible in fullscreen mode if mouse move over it, not only buttons on right top side.
* **StswHeader** no longer have "IsHighlighted" property - it was moved to **StswSubRadio** because this is the only place where it is needed.
* Changed template for **StswMessageDialog**.
* **StswDatabases** and **StswMailboxes** no longer have "Collection" property. Also "Current" property has been renamed into "Default". Importing list now returns IEnumerable and exporting list now needs IEnumerable as argument.
* Some color methods moved from **StswExtensions** to **StswFn**.
* **Clone** extension method renamed into **TryClone**.
  **RemoveBy** extension method renamed into **Remove**.
* **StswTranslator** can cache translations and load translations from file asynchronously.
* Changed logo for StswExpress NuGet package.

### Bugfixes:

* Improvements and bugfixes for converters.
* Bugfix for **StswRatingControl** when "ExpandDirection" property was set to "Up".
* Bugfix for displaying **StswInfoBadge** value when value was less than 0.
* **StswHeader** has fixed alignment of icon and text when stretched.
* Changed logic fo Stsw boxes when Enter key has been pressed and input is invalid.
* **StswColorBox** can manage null value with converting it to default without binding failures.
* Bugfix for **StswColorConverter** when target type is nullable.
* Bugfix for **StswTabControl** initial selected index.

# **0.8.2**
2024-07-08

### New functionality:
* **StswMailboxModel** has additional property called "Domain". This property can be used for NetworkCredential in sending mail.

# **0.8.1**
2024-06-12

### Changed functionality:
* **StswInfoBadge** can use value separated by thousands.
* **StswCompareConverter** regained its old functionality alongside with the new one.

### Bugfixes:
* **StswMailbox**'s method for sending mail disposes SmtpClient at the end of it.
* **StswFilePicker** has small bugfix for separating by thousands.

# **0.8.0**  
2024-06-10

### New functionality:
* Upgraded to **.NET 8**
* New controls: **StswSegment**, **StswMenu**, **StswMenuItem**.
* New converters: **StswEnumDescriptionConverter**.
* New markup extensions: **StswEnumToListExtension** (it replaces **StswEnumToListConverter**), **StswMakeListExtension**.
* **StswDatabases** has new experimental "LessSpaceQuery" method. This method allows to remove unnecessary spaces in query to make them more readable in loggers.

### Changed functionality:
* **StswScrollViewer** renamed to **StswScrollView**.  
  **StswDirectionViewer** renamed to **StswDirectionView**.
* **StswPopup** can change the type of contained scroll inside it between **ScrollView** and **DirectionView** using "ScrollType" property.
* **StswMapTo** extension removes spaces from column names and normalizes diacritics in them for mapping purposes.
* **IStswCollectionItem** no longer implements "ItemMessage" property and **StswDataGrid** no longer uses it in its special column.
* **StswMessageDialog** has new button to copy its content and new section to display details.
* **StswCompareConverter** has been reworked to compare numeric value to a specified threshold nad determine if it is greater than, less than, greater than or equal to, or less than or equal to the threshold. It can also compare a value or check if value contains a flag.
* **StswHyperlinkButton** by default has content equal to its "NavigateUri" property.
* **StswGrid** has extended auto definition functionality - more options to make auto definitions and options to auto size its columns and rows.
* **StswContextMenu** style uses **StswDirectionView** now.
* **StswHeader** has changed margins in template to match other controls.
* Stsw sub controls have bigger icon by default.
* **NewExtension** renamed to **StswCreateInstanceExtension**.  
  **NameOfExtension** renamed to **StswNameOfExtension**.

### Bugfixes:
* **StswWindow** can properly pass "Components" to **StswWindowBar** now.
* **StswComboBox** no longer keyboard focus when its filter is not enabled.
* **StswTextEditor** has customizable scroll viewer control now.
* **StswBindingList** properly counts initial items as "Unchanged".

# **0.7.1**
2024-04-29

### Bugfixes:
* Stsw collection items no longer set "DisplayMemberPath" if items source type is assignable to **StswComboItem** but item template is not null.
* **StswMessageDialog** has properly stretched text box.

# **0.7.0**
2024-04-28

### New functionality:

* New controls: **StswChartColumns**, **StswGrid** (auto column and row definitions), **StswSpinner** (replaces **StswLoadingCircle** and allows to choose different animations).
* **StswScrollViewer** has some new functions. First is new property "AutoScroll" that allows to automatically scroll content to bottom if scroll was at bottom and new items were added. Second is possibility to prepare a command that will trigger when scroll move to bottom that can be used for example to make infinite scroll for data grid starting with small number of rows.
* **StswNotifyIcon** has new static method to display system notifications.
* **StswToolTip** has new property named "IsMoveable" that allows it to move together with mouse cursor.
* New converter: **StswColorAdvancedConverter** that merges 3 other converters for alpha, brightness and saturation of color.
* New function: **GenerateColor** (allows to generate color from any text).

### Changed functionality:

* **StswScrollViewer**'s and **StswPopup**'s config in other controls can be controlled by attached properties now.
* **StswComboBox** uses VirtualizingStackPanel now and also can contain filter inside popup when "IsFilterEnabled" property is set to true.
* **StswMessageDialog** template has been improved. One new message type has been added named "Blockade". Message type "Question" has slightly different color now.
* **StswDropArrow** is no longer hidden from designer and has additional properties similar to **StswIcon**. Property "IsDropDownOpen" renamed into "IsExpanded".
* **StswDropButton** and **StswSplitButton** both have new property named "AutoClose" that allows to close popup after clicking any button inside.
* **StswFilePicker** has new property named "IsFileSizeVisible" that allows it to display file size.
* Stsw chart controls have been improved.
* **StswHyperlinkButton** has been improved.
* **StswRatingControl** has been improved and now supports key shortcuts.
* **StswDataGrid**'s special column template has been improved.
* Stsw collection controls no longer need to have specified "DisplayMemberPath" and "SelectedValuePath" properties as long as ItemsSource uses **StswComboItem** as its item class.
* **StswSidePanel**'s property "ExpandMode" has been divided into two properties: "IsAlwaysVisible" and "IsCollapsed".
* **StswNotifyIcon** has new property "IsAlwaysVisible" that allows to keep icon even if window is not hidden.
* **StswImage** uses **StswBorder** so it can be clipped now.
* Stsw subcontrols inside box controls can be now docked to any side, not only to right.
* Stsw box controls have default vertical alignment "Top" instead of "Stretch". **StswGroupBox** has default vertical alignment "Stretch" instead of "Top".
* **StswApp** has overridable method named "OnThemeChanged" to simplify a little setting custom theme brushes.
* Attached properties named "IsBorderless" and "IsArrowless" have been moved to **StswControl** class.
* Dark theme is a little lighter again. Brush colors for Stsw navigation controls have been a little desaturated.

### Bugfixes:

* Settings like **interface size**, **language**, **theme** will be remembered by library.
* **StswRatingControl** properly supports directions other than "Right".
* **StswInfoPanel** has pixel scroll unit virtualization by default now.
* **RadioConverter** renamed into **StswRadioConverter**.
* Extensions **PadLeft** and **PadRight** work properly now.

# **0.6.2**
2024-03-14

### Bugfixes:

* **ConvertTo** extension now will work with nullable enums.

# **0.6.1**
2024-03-05

### New functionality:

* New hidden sub control: **StswSubError** (used in box controls to show validation errors).
* New base class: **StswBoxBase** (for box controls).
* New converter: **StswRadioConverter** (for RadioBox based controls).
* New extension: **RemoveBy** (to remove multiple items from IList).

### Changed functionality:

* **IStswBoxControl** interface has been extended. All box controls have 2 new properties: "Errors", "HasError", and 1 less property: "IsErrorVisible".

### Bugfixes:

* **StswBindingList** can be sorted now (for example in **StswDataGrid** when clicked on column).

# **0.6.0**
2024-03-02

### New functionality:

* Stsw check controls have read only mode controlled by "IsReadOnly" property.
* New interface: **IStswScrollableControl**. Other Stsw interfaces are extended too.
* New property for **StswApp** named "AllowMultipleInstances". If it is set to false then any additional instance of application is automatically closed allowing only one instance to be runned at the same time.
* New converter: **StswIsTypeConverter**.
* New markup extensions: **NewExtension** and **NameOfExtension**.
* New extensions: **Copy** and **IsListType**.
* New functions: **ShiftIndexBy** and **GetProcessUser**.
* New attached property "StswBorder.IsBorderless" that allows to set border thickness and corner radius to 0 for any control that implements **IStswCornerControl** interface.

### Changed functionality:

* All Stsw controls have set "SnapsToDevicePixels" to false by default.
* All Stsw controls that contain **StswPopup** have new property "Popup" of new **StswPopupModel** type allowing to style its background, border and padding.
* All Stsw controls that contain **StswScrollViewer** have new property "ScrollViewer" of new **StswScrollViewerModel** type allowing to style its scroll bars.
* All Stsw box controls have improved validation error style that turns borders red and shows additional sub control. Its visibility can be managed by "IsErrorVisible" property.
* **StswTimedSwitch** can be set to infinite time (without reverting content to its base form).
* **StswToggleSwitch** has been remade.
* **StswDataGrid** can detect **IStswSelectionItem** interface on its "ItemsSource" property - it is similar functionality to other Stsw collection controls and allows to bind multiple selected items.
* **StswDirectionViewer** have only two button border properties instead of four: "BBtnThickness" (up or left) and "FBtnThickness" (down or right).
* Stsw commands are improved. Additionally "IsWorking" property has been renamed into "IsBusy" to match **StswHeader**'s "IsBusy" property name. Async commands also have new "IsReausable" property that allows to run command multiple times.
* A few changes in theme brush key names and template styles (for Stsw check controls).
* A few changes in template part names.
* **StswChartPie** is full by default - it just looks better than a version with hole in center.
* **StswRatingControl**'s "CanReset" property has been renamed into "IsResetEnabled". There is also one new property named "ItemsNumberVisibility" that allows to display order number below each icon.
* Dark theme has been modified to have darker colored parts. Pink theme has been remade.
* Stsw internal sub controls are disabled for editor browsing.

### Bugfixes:

* All Stsw controls have corrected "Focusable" and "IsTabStop" values.
* All Stsw controls that were using **StswScrollViewer** have properly visible horizontal scroll bar - before it was never showing up, for example in **StswDataGrid**.
* All Stsw collection controls have better logic for detecting if its items source implements **IStswSelectionItem** interface.
* All Stsw box controls no longer updates binding property on Enter key if "AcceptsReturn" property is set to true.
* **StswSubDrop** has improved template to properly detect if it has content. **StswHeader** has "IsHighlighted" property so **StswSubRadio** has fixed checked style.
* **StswNotifyIcon** can show in tray only if it is enabled.
* **StswWindow** now can pass "Components" to its title bar.

# **0.5.0**
2024-01-22

### New functionality:

* There is new category for Stsw controls named "File" that includes 1 new control: **StswFilePicker** used to select either file or directory path.
* New controls: **StswHyperlinkButton**, **StswShiftButton**, **StswTimedSwitch**, **StswDataPager**, **StswTimePicker**, **StswFilePicker**, **StswMediaPlayer**, **StswMenuItem**, **StswInfoBadge**, **StswInfoBar**, **StswWindowBar**.
* New control style: **StswContextMenu**.
* New interface: **IStswBoxControl**. **IStswComponentControl** renamed into **IStswSubControl**.
* New converter: **StswListFromRangeConverter**.
* New extensions: **ToImageSource**, **GetNextValue** (instead of **GetNextEnumValue** function).
* New functions: **SplitStringByLines**, **RemoveFromParent**.

### Changed functionality:

* External library called "NewtonsoftJson" is no longer used.
* **StswNavigation** and **StswNavigationElement** have been reworked.
* **StswSidePanel** can be set to be always visible.
* "Components" controls category renamed into "Subs" controls. All component controls changed prefix to "Sub". All properties for components renamed into "SubControls".
* **StswShifter** renamed into **StswDirectionViewer**. **StswLogPanel** renamed into **StswInfoPanel**.
* **StswHeader** and all sub controls now has property "IsContentVisible" instead of "ContentVisibility".
* Part of **StswWindow**'s functionality moved to new control **StswWindowBar**.
* A few changes in Stsw enums.
* A few changes in theme brush key names.
* **StswMailboxModel** implements INotifyPropertyChanged now.

### Bugfixes:

* Bugfix translation for **StswColorBox**'s tab headers.
* Going into fullscreen mode should properly activate and focus window.

# **0.4.3**
2023-12-27

### Bugfixes:
* **StswExport** is improved - now numeric and date values should be properly displayed and formatted in cells.

# **0.4.2**
2023-12-14

### Bugfixes:
* **StswCalendar** no longer allows to select date that is not between minimum and maximum range. "Today" button becomes disabled when current date is not in minimum and maximum range.

# **0.4.1**
2023-12-13

### Changed functionality:
* **StswFilterBox** have "FilterMenuMode" property for filter mode button. **StswFilterMode** enum no longer have "None" value.

# **0.4.0**
2023-12-13

### New functionality:

* There is new category for Stsw controls named "Charts" that includes 2 new controls: **StswChartLegend** and **StswChartPie** used to present analytics data and statistics.
* New component controls: **StswComponentDrop** (almost the same functionality as **StswDropButton**), **StswComponentHeader** (almost the same functionality as **StswHeader**).
* New control: **StswProgressRing** (almost the same functionality as **StswProgressBar**).
* New control: **StswToolTip** (just a tooltip with layout scaling and with style that fits other Stsw controls).
* New extension to clone binding and new extension for getting attribute of T type from enum.

### Changed functionality:

* **StswClippingBorder** has been renamed into **StswBorder** and no longer clips its child by default. **StswFilter** has been renamed into **StswFilterBox**. **IStswComponent** has been renamed into **IStswComponentControl**. **IStswSelection** has been renamed into **IStswSelectionItem**. Added 3 new control interfaces: **IStswCornerControl**, **IStswDropControl** and **IStswIconControl**.
* All Stsw controls that have **StswBorder** inside their template implement **IStswCornerControl** interface that allows them to use two properties: "CornerClipping" and "CornerRadius". Latter is used to set radius of border's corners, former is used to clip all elements inside border that stick out of border when border has corner radius greater than 0.
* All Stsw controls that had arrow icon in their templates are now using separated internal control named **StswDropArrow**. Those controls no longer have "ArrowVisibility" property - instead arrow's visibility can be changed through dynamic visibility with key: "StswDropArrow.Visibility".
* All Stsw component controls changes their opacity on IsMouseOver and IsPressed instead of changing foreground brush. Additionally they have new properties for icon: "IconFill", "IconStroke" and "IconStrokeThickness".
* All Stsw controls that have "MaxDropDownHeight" property, have its default value equal to ⅓ of primary screen's height.
* Changed the way scale worked for icons and images:
  **Before:** *null* fits content, *auto* fits whole space, *star* do nothing, *value* multiply by 12 pixels.
  **After:** *null* is not possible, *auto* fits content, *star* fits whole space, *value* multiply by 12 pixels.
* Changed the way some text modes worked for progress controls:
  **Before:** *value* shows current value based on minimum and maximum.
  **After:** *value* shows exact value from property, *progress* shows what *value* was showing before.
* **StswLogPanel** derives from ListBox class now (simply to enable virtualization) and its templates have been improved.
* **StswComponentPanel** has been reworked and renamed into **StswComponentSelector**.
* **StswTruncateLabel** has been renamed into **StswLabel** and no longer truncates content by default.
* **StswPager** has been renamed into **StswGallery** and its goal now is presenting images. Also uses **StswZoomPanel** inside its template and can properly slide images with arrow keys.
* **StswRatingControl** instead of "Orientation" property with 2 options, has new "Direction" property that allows for all 4 direction modes for selecting value. Additionally it implements **IStswIconControl** interface now so its icon properties have "Icon" word prefix now.
* **StswDropButton** and **StswSplitButton** controls are derived from HeaderedItemsControl class now so they can use advanced header properties.
* **StswToggleButton**'s "Icon" property has been removed and some properties have been renamed: "CheckedGlyphBrush" into "GlyphBrushChecked" and "UncheckedGlyphBrush" into "GlyphBrushUnchecked".
* **StswColorPicker**'s and **StswColorSelector**'s templates have been improved a little, no longer have outer border and additionally can have stretched content.
* **StswGroupBox**'s and **StswEpander**'s templates have been simplified.
* **StswPopup**'s border thickness, corner radius and padding are now controlled by dynamic resource values.
* **StswShifter** has 4 new properties for border thickness, one for each of its buttons.
* **StswAdaptiveBox** can now pass "Components" and "IsThreeState" property values to box that is in use at the moment.
* **StswFilterBox**'s "FilterSqlColumn" property has been renamed into "FilterValuePath".
* All Stsw controls no longer use "RecognizesAccessKey" property in their templates and no longer use "ComponentAlignment" and "PopupThickness" properties at all.
* Some changes in usage of theme brushes in Stsw controls - for example **StswDropButton** uses "StswButton.Pressed.Background" as its background brush when drop-down is open instead of "StswDropButton.Checked.Background".
* **StswLog**'s import is improved.
* **StswDatabase** renamed to **StswDatabases**. Some methods are renamed and the way of importing and exporting "Port" is improved.
* Automatic language can be removed from confgiuration dialog now.

### Bugfixes:

* **StswToggleSwitch** no longer have different size when is checked at start.
* **StswListBox**, **StswListView** and **StswTreeView** will properly determine if they are using ItemsSource of **IStswSelectionItem** type.
* **StswSelectionBox**'s list box do not content scroll by default to elimiate bug with last empty item.
* **StswColorBox**, **StswDatePicker** and **StswNumericBox** no longer update their main property twice in certain situations.
* **StswProgressBar**'s background and border can be changed in custom state mode.
* **StswTabControl** properly shows border when last tab is selected.
* **StswImage**'s, **StswFilterBox**'s and **StswWindow**'s context menus are scaling with interface size from configuration dialog now.
* Bugfix for double date selections in **StswCalendar** when starting date had specified time.
* Bugfix for **StswBindingList** if list has been reseted.
* Bugfix for **StswDataGrid**'s special column dynamic brushes for Dark and Pink themes.
* Stsw button controls no longer have additional grid in their template to fix clipping border since there exists "CornerClipping" property now.
* **StswContentDialog** will no longer throw exception if someone will try to close it multiple times in a moment.

# **0.3.2**
2023-10-26

### Changed functionality:

* **StswCollection** remade into **StswBindingList** - it derives from BindingList now and is behaving better when any property inside list has been modified.
* **IStswCollectionItem**'s "ItemState" property is now of type "StswItemState" (new enum) instead of "DataRowState".
* **StswEnumToListConverter** returns list of **StswSelectionItem** type instead of **StswComboItem** - it should fix **StswFilter** if it has result of this converter as ItemsSource.
* **ToStswCollection** extension method renamed into **ToStswBindingList**.

### Bugfixes:

* **ConvertTo** extension method can properly convert to enum type.
* **StswFilter** properly calculates SQL string when its ItemsSource contains enum values.

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
* **StswDropButton**, **StswSplitButton**, **StswComboBox**, **StswSelectionBox** and all component buttons have simplified template. **StswDataGrid** is finally using its own template. **StswDropButton** and **StswSplitButton** are derived from ItemsControl class.
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

### New functionality:

* New controls: **StswCalendar**, **StswColorBox**, **StswColorPicker**, **StswColorSelector**, **StswComponentButton**, **StswComponentCheck**, **StswComponentPanel**, **StswComponentRebutton**, **StswContentDialog**, **StswDropButton**, **StswExpander**, **StswGroupBox**, **StswListBox**, **StswLogPanel**, **StswNotifyIcon**, **StswRadioBox**, **StswRatingControl**, **StswScrollBar**, **StswScrollViewer**, **StswSelectionBox**, **StswSplitButton**, **StswTabControl**, **StswTabItem**, **StswText**, **StswTextEditor, StswTruncateLabel**.
* New converters: **StswEnumToListConverter**, **StswModuloConverter**.
  New converters splitted from **StswColorConverter**: **StswColorAlphaConverter**, **StswColorGeneratorConverter**, **StswColorSaturationConverter**.
* New extensions: **PadLeft**, **PadRight**, **FromAhsl**, **FromHsl**, **ToHsl**, **FromAhsv**, **FromHsv**, **ToHsv**, **ToColor**, **ToDrawingColor**, **ToMediaColor**, **ToHtml**, **Clone**.
* New functions: **CalculateString**, **FindVisualAncestor**, **GetParent**.
* New item classes: **StswCollectionItem** (with "ItemState" property and "ShowDetails" property), **StswLogItem**.
* New icons.
* New class: **StswApp**.
* New **StswCommand** that can pass parameter.
* Some Stsw controls now have a new event that invokes when their main property has been changed.
* **StswNumericBox** has new "Format" property.
* **StswIcon** and **StswHeader** now have "Stroke" property and "StrokeThickness" property.
* **StswNavigation** now has "ItemsWidth" property so it can have constant width for items panel.
* **StswComboBox**'s text box can have placeholder now.

### Changed functionality:

* GUI scaling has been upgraded - now it finally uses "ScaleTransform" on **StswWindow** instead of spamming **StswMultiplyConverter** on "FontSize" in every control's style.
* All Stsw controls use dynamic resources to manage brushes instead of having style properties.
* **StswCheckBox** has smaller icon.
* **StswCalendarPicker** renamed to **StswDatePicker**.
* **StswTextBox**, **StswNumericBox**, **StswDateBox** no longer have clear button. **StswPasswordBox** no longer has show password button.
* Improvements for conveters. Additionally, each converter will be created only once first time it is referenced to.
* **StswSimpleModel** name reverted to **StswComboBoxModel**.
* **StswHeader** no longer has "SubTexts" property since the same effect can be achieved by declaring controls as its content.
* Some functional improvements in **StswNavigation** and **StswNavigationElement** (renamed from **StswNavigationButton** and now can function as both: button and expander).
* **StswDataGrid** shows different colors for special column's buttons when "ItemState" property has value other than "Unchanged".
* **StswNavigationElement** can take "GroupName" from **StswNavigation**.
* Property "Buttons" for Stsw box controls has been renamed into "Components". This property can only receive **IStswComponent** controls. Additionally, components will no longer be disabled when control is in read only mode.
* **StswDatePicker** and **StswColorBox** now have function button with Press click mode.
* **StswLoadingCircle** changes its opacity when disabled instead of fill color (similarly to **StswIcon** and **StswImage**).
* **StswImage** has different method for paste image from clipboard.
* **StswSeparator** now has a little more intuitive "BorderThickness" logic (shortly described: separator's thickness is not doubled). Additionally has different look when disabled.
* **StswComboBox** and **StswComboView** can be opened in read only mode (but selection can not be changed).
* **StswNumericBox** and **StswDatePicker** refreshes "Value"/"SelectedDate" property before incrementing.
* **StswFilter** has been reworked. Clearing and getting filters data is moved from **StswFilter** to **StswDataGrid**.
* Improvements for **StswPasswordBox**, **StswWindow**'s title bar, **StswCollection**.
* Stsw control styles moved to resource dictionaries.
* Corner radius for all Stsw controls changed from 9 to 10. **StswDataGrid**'s header cell's border thickness changed from 0.5 to 1.
* **ClippingBorder** renamed into **StswClippingBorder**. Every main border in Stsw controls that uses "CornerRadius" property is a **ClippingBorder** now.
* **OutlinedTextBlock** has been renamed into **StswOutlinedText**.
* Some Stsw controls now have visible checked state while being disabled (for example buttons and checks).
* **StswTextBox** now refreshes after Enter click (that also fixes its behaviour in **StswFilter**).
* **StswExport** now takes only properties that have export attribute.

### Bugfixes:

* Bugfix for converters when decimal separator is not set to dot.
* Stsw buttons now have grids in templates between "ClippingBorder" and "ContentPresenter" so content should be clipped properly now.
* **StswWindow** now properly works with fullscreen mode.
* Bugfix for menu option that set default window size.
* Bugfixes for **StswCalendar**. Additionally **StswCalendar** now works with first days of week other than "Monday" and properly shows last possible day to choose (9999-12-31).
* Bugfixes for **StswDataGrid**'s special column.
* Bugfix for Stsw buttons' border colors.
* **StswDatePicker** and **StswNumericBox** now properly display formatted text after binding failure.
* Bugfix for "Minimum" property and "Maximum" property in **StswDatePicker** and **StswNumericBox** - new value is validated before being assigned instead of corrected after being assigned.
* Bugfixes for "IconScale" property in a few controls.
* Many other small bugfixes.

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
* **MultiBox**'s functionality is merged into **StswComboBox** (when "SelectionMode" property is set to "Multiple").
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
