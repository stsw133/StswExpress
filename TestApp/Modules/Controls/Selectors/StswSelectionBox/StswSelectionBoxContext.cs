using System;
using System.ComponentModel;
using System.Linq;

namespace TestApp;
public partial class StswSelectionBoxContext : ControlsContext
{
    public StswCommand RandomizeCommand => new(Randomize);
    //public ICommand? UpdateTextCommand { get; } = null; /// this command is only for updating text in box when popup did not load yet

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    #region Events & commands
    /// Command: randomize
    private void Randomize()
    {
        foreach (var item in Items.Where(x => new Random().NextDouble() > 0.6))
            item.IsSelected = !item.IsSelected;
        //UpdateTextCommand?.Execute(null);
    }
    #endregion

    [StswObservableProperty] bool _icon;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] BindingList<StswListBoxTestModel> _items = new([.. Enumerable.Range(1, 15).Select(i => new StswListBoxTestModel { Name = "Option " + i, IsSelected = new Random().Next(2) == 0 })]);
    public int SelectionCounter => Items.AsEnumerable().Count(x => x.IsSelected);
    [StswObservableProperty] bool _subControls = false;
}
