using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace TestApp;
public partial class StswSelectionBoxContext : ControlsContext
{
    //public ICommand? UpdateTextCommand { get; } = null; /// this command is only for updating text in box when popup did not load yet

    public override void SetDefaults()
    {
        base.SetDefaults();

        HideSelectedItemWhenFiltered = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property == StswComboBox.HideSelectedItemWhenFilteredProperty)?.Value ?? default;
        IsFilterEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property == StswComboBox.IsFilterEnabledProperty)?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property == StswSelectionBox.IsReadOnlyProperty)?.Value ?? default;
        DropArrowVisibility = (Visibility?)ThisControlSetters.FirstOrDefault(x => x.Property == StswDropArrow.VisibilityProperty)?.Value ?? default;
    }

    [StswCommand] void Randomize() => Items.Where(x => new Random().NextDouble() > 0.6).ForEach(x => x.IsSelected = !x.IsSelected);

    [StswObservableProperty] bool _hideSelectedItemWhenFiltered;
    [StswObservableProperty] bool _icon;
    [StswObservableProperty] bool _isFilterEnabled;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] ObservableCollection<StswListBoxTestModel> _items = new([.. Enumerable.Range(1, 15).Select(i => new StswListBoxTestModel { Name = "Option " + i, IsSelected = new Random().Next(2) == 0 })]);
    [StswObservableProperty] bool _subControls;
    [StswObservableProperty] Visibility _dropArrowVisibility;
    public int SelectionCounter => Items.AsEnumerable().Count(x => x.IsSelected);
}
