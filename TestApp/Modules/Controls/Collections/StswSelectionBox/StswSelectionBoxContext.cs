using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace TestApp;

public class StswSelectionBoxContext : ControlsContext
{
    public StswCommand ClearCommand => new(Clear);
    public StswCommand RandomizeCommand => new(Randomize);
    public ICommand? SetTextCommand { get; set; } = null; /// this command is only for updating text in box when popup did not load yet

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    #region Events & commands
    /// Command: clear
    private void Clear()
    {
        Items.Where(x => x.IsSelected).ToList().ForEach(x => x.IsSelected = false);
        SetTextCommand?.Execute(null);
    }
    /// Command: randomize
    private void Randomize()
    {
        foreach (var item in Items.Where(x => new Random().NextDouble() > 0.5))
            item.IsSelected = !item.IsSelected;
        SetTextCommand?.Execute(null);
    }
    #endregion

    /// IsReadOnly
    private bool isReadOnly;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// Items
    private BindingList<StswListBoxTestModel> items = new(Enumerable.Range(1, 15).Select(i => new StswListBoxTestModel { Name = "Option " + i, IsSelected = new Random().Next(2) == 0 }).ToList());
    public BindingList<StswListBoxTestModel> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }

    /// SelectionCounter
    public int SelectionCounter => Items.AsEnumerable().Count(x => x.IsSelected);

    /// SubControls
    private bool subControls = false;
    public bool SubControls
    {
        get => subControls;
        set => SetProperty(ref subControls, value);
    }
}
