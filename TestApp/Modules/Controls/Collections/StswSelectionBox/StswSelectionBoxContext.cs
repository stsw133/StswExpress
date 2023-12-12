using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace TestApp;

public class StswSelectionBoxContext : ControlsContext
{
    public ICommand ClearCommand { get; set; }
    public ICommand RandomizeCommand { get; set; }
    public ICommand? SetTextCommand { get; set; }

    public StswSelectionBoxContext()
    {
        Items.ListChanged += (s, e) => NotifyPropertyChanged(nameof(SelectionCounter));

        ClearCommand = new StswCommand(Clear);
        RandomizeCommand = new StswCommand(Randomize);
        SetTextCommand = null; /// this command is only for updating text in box when popup did not load yet
    }

    #region Events and methods
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

    #region Properties
    /// Components
    private bool components = false;
    public bool Components
    {
        get => components;
        set => SetProperty(ref components, value);
    }

    /// IsReadOnly
    private bool isReadOnly = false;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// Items
    private BindingList<StswListBoxTestModel> items = new()
    {
        new() { Name = "Option 1", IsSelected = true },
        new() { Name = "Option 2", IsSelected = false },
        new() { Name = "Option 3", IsSelected = false },
        new() { Name = "Option 4", IsSelected = true },
        new() { Name = "Option 5", IsSelected = false },
        new() { Name = "Option 6", IsSelected = true },
        new() { Name = "Option 7", IsSelected = false },
        new() { Name = "Option 8", IsSelected = false },
        new() { Name = "Option 9", IsSelected = true },
        new() { Name = "Option 10", IsSelected = false }
    };
    public BindingList<StswListBoxTestModel> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }

    /// SelectionCounter
    public int SelectionCounter => Items.AsEnumerable().Count(x => x.IsSelected);
    #endregion
}
