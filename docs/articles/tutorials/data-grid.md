# StswDataGrid Tutorial (WPF)

> This tutorial shows how to properly configure `StswDataGrid`, use `StswDataGridFilterBox` in column headers (preferably with Stsw columns), and add `StswDataGridStatusColumn` based on patterns used in **StswExpress.Wpf** and **TestApp.Wpf**.

## Screenshot placeholder

Add your final screenshot here after configuring the grid:

```md
![Configured StswDataGrid](./path-to-your-screenshot.png)
```

---

## 1) When to use `StswDataGrid`

Use `StswDataGrid` when you need:

- built-in filtering support,
- convenient custom column types (`StswDataGridTextColumn`, `StswDataGridDecimalColumn`, etc.),
- optional SQL filter generation,
- row state + row details integration via status column.

A basic setup in XAML:

```xml
<se:StswDataGrid ItemsSource="{Binding Items}">
    <se:StswDataGrid.Columns>
        <se:StswDataGridStatusColumn/>

        <se:StswDataGridDecimalColumn Binding="{Binding Id}" Width="100">
            <se:StswDataGridDecimalColumn.Header>
                <se:StswDataGridFilterBox Header="ID" FilterType="Number" FilterMode="Equal" FilterValuePath="Id"/>
            </se:StswDataGridDecimalColumn.Header>
        </se:StswDataGridDecimalColumn>

        <se:StswDataGridTextColumn Binding="{Binding Name}" Width="200">
            <se:StswDataGridTextColumn.Header>
                <se:StswDataGridFilterBox Header="Name" FilterType="Text" FilterMode="Contains" FilterValuePath="Name"/>
            </se:StswDataGridTextColumn.Header>
        </se:StswDataGridTextColumn>
    </se:StswDataGrid.Columns>
</se:StswDataGrid>
```

---

## 2) Recommended pattern: `StswDataGridFilterBox` in Stsw column headers

The recommended pattern is:

1. Use **Stsw columns** (`StswDataGridTextColumn`, `StswDataGridDecimalColumn`, `StswDataGridDateColumn`, `StswDataGridCheckColumn`, `StswDataGridComboColumn`) whenever possible.
2. Put `StswDataGridFilterBox` in each column header.
3. Match `FilterType` to data type and set `FilterValuePath` to the field/property name used for filtering.

### Example by data type

```xml
<!-- Number -->
<se:StswDataGridDecimalColumn Binding="{Binding Id}">
    <se:StswDataGridDecimalColumn.Header>
        <se:StswDataGridFilterBox Header="ID" FilterType="Number" FilterMode="Equal" FilterValuePath="Id"/>
    </se:StswDataGridDecimalColumn.Header>
</se:StswDataGridDecimalColumn>

<!-- Text -->
<se:StswDataGridTextColumn Binding="{Binding Name}">
    <se:StswDataGridTextColumn.Header>
        <se:StswDataGridFilterBox Header="Name" FilterType="Text" FilterMode="Contains" FilterValuePath="Name"/>
    </se:StswDataGridTextColumn.Header>
</se:StswDataGridTextColumn>

<!-- List / enum -->
<se:StswDataGridComboColumn
    DisplayMemberPath="Display"
    ItemsSource="{se:StswEnumToList local:ContractorType}"
    SelectedValueBinding="{Binding Type, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
    SelectedValuePath="Value">
    <se:StswDataGridComboColumn.Header>
        <se:StswDataGridFilterBox
            Header="Type"
            FilterType="List"
            FilterMode="In"
            FilterValuePath="Type"
            ItemsSource="{se:StswEnumToList local:ContractorType}"/>
    </se:StswDataGridComboColumn.Header>
</se:StswDataGridComboColumn>

<!-- Boolean -->
<se:StswDataGridCheckColumn Binding="{Binding IsArchival}">
    <se:StswDataGridCheckColumn.Header>
        <se:StswDataGridFilterBox Header="Is archival" FilterType="Check" FilterValuePath="IsArchival" ApplyNullReplacement="True"/>
    </se:StswDataGridCheckColumn.Header>
</se:StswDataGridCheckColumn>

<!-- Date -->
<se:StswDataGridDateColumn Binding="{Binding CreateDT, StringFormat='yyyy-MM-dd HH:mm'}" Format="yyyy-MM-dd HH:mm">
    <se:StswDataGridDateColumn.Header>
        <se:StswDataGridFilterBox Header="CreateDT" FilterType="Date" FilterMode="GreaterEqual" FilterValuePath="CreateDT"/>
    </se:StswDataGridDateColumn.Header>
</se:StswDataGridDateColumn>
```

### Important `StswDataGridFilterBox` properties

- `Header`: text shown in the header.
- `FilterType`: control/data type for filtering (`Text`, `Number`, `Date`, `Check`, `List`, etc.).
- `FilterMode`: operator (`Equal`, `Contains`, `GreaterEqual`, `Between`, `In`, etc.).
- `FilterValuePath`: source field/property used by filtering logic.
- `ItemsSource`, `DisplayMemberPath`, `SelectedValuePath`: for list-based filters.
- `ApplyCaseTransform`, `ApplyNullReplacement`, `Format`: optional behavior tweaks.

---

## 3) Add `StswDataGridStatusColumn`

Add the status column as the **first column**:

```xml
<se:StswDataGrid.Columns>
    <se:StswDataGridStatusColumn/>
    <!-- other columns -->
</se:StswDataGrid.Columns>
```

What it gives you:

- header actions for filter visibility and clear filters,
- row state indicator when row model supports tracking,
- row details toggle (expand/collapse) when row model supports details.

### Row model requirements (recommended)

To fully benefit from the status column behavior, your row model should implement:

- `IStswTrackableItem` (for `ItemState` indicator: added/modified/deleted/unchanged),
- `IStswDetailedItem` (for `ShowDetails` toggle + row details behavior).

Example model:

```csharp
public partial class MyRowModel : StswObservableObject, IStswDetailedItem, IStswTrackableItem
{
    [StswObservableProperty] private int _id;
    [StswObservableProperty] private string? _name;
    [StswObservableProperty] private StswItemState _itemState;
    [StswObservableProperty] private bool? _showDetails;
}
```

And optional details template:

```xml
<se:StswDataGrid.RowDetailsTemplate>
    <DataTemplate>
        <StackPanel Margin="5">
            <se:StswLabel Content="Row details..." Margin="5"/>
        </StackPanel>
    </DataTemplate>
</se:StswDataGrid.RowDetailsTemplate>
```

---

## 4) Full practical example (in one place)

```xml
<se:StswDataGrid FiltersData="{Binding Filters}"
                 FiltersType="CollectionView"
                 FrozenColumnCount="1"
                 ItemsSource="{Binding Items}"
                 SelectedItem="{Binding SelectedItem}">

    <se:StswDataGrid.Columns>
        <se:StswDataGridStatusColumn/>

        <se:StswDataGridDecimalColumn Binding="{Binding Id}" IsReadOnly="True">
            <se:StswDataGridDecimalColumn.Header>
                <se:StswDataGridFilterBox Header="ID" FilterType="Number" FilterMode="Equal" FilterValuePath="Id"/>
            </se:StswDataGridDecimalColumn.Header>
        </se:StswDataGridDecimalColumn>

        <se:StswDataGridTextColumn Binding="{Binding Name}">
            <se:StswDataGridTextColumn.Header>
                <se:StswDataGridFilterBox Header="Name" FilterType="Text" FilterMode="Contains" FilterValuePath="Name"/>
            </se:StswDataGridTextColumn.Header>
        </se:StswDataGridTextColumn>

        <se:StswDataGridCheckColumn Binding="{Binding IsArchival}">
            <se:StswDataGridCheckColumn.Header>
                <se:StswDataGridFilterBox Header="Is archival" FilterType="Check" FilterValuePath="IsArchival" ApplyNullReplacement="True"/>
            </se:StswDataGridCheckColumn.Header>
        </se:StswDataGridCheckColumn>
    </se:StswDataGrid.Columns>

    <se:StswDataGrid.RowDetailsTemplate>
        <DataTemplate>
            <se:StswText Margin="3" Text="There are some row details..."/>
        </DataTemplate>
    </se:StswDataGrid.RowDetailsTemplate>
</se:StswDataGrid>
```

---

## 5) Practical checklist

Before you finish, verify:

- [ ] `StswDataGridStatusColumn` is the first column.
- [ ] Every key column header uses `StswDataGridFilterBox`.
- [ ] `FilterType` and `FilterMode` match the data type.
- [ ] `FilterValuePath` points to the expected field/property.
- [ ] For list filters, `ItemsSource` is provided.
- [ ] Row model implements `IStswTrackableItem` and/or `IStswDetailedItem` if you want full status-column features.
- [ ] `RowDetailsTemplate` exists if you want expandable row details.

---

## 6) References in repository

- `StswExpress.Wpf/Controls/DataGrids/StswDataGrid.xaml.cs`
- `StswExpress.Wpf/Controls/DataGrids/StswDataGridFilterBox.xaml.cs`
- `StswExpress.Wpf/Controls/DataGrids/_columns/StswDataGridStatusColumn.xaml`
- `StswExpress.Wpf/Controls/DataGrids/_columns/StswDataGridStatusColumn.xaml.cs`
- `TestApp.Wpf/Modules/Controls/DataGrids/StswDataGrid/StswDataGridView.xaml`
- `TestApp.Wpf/Modules/Examples/Contractors/ContractorsView.xaml`