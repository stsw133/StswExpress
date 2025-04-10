﻿using System;
using System.ComponentModel;
using System.Linq;

namespace TestApp;

public class StswSelectionBoxContext : ControlsContext
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

    /// Icon
    public bool Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }
    private bool _icon;

    /// IsReadOnly
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }
    private bool _isReadOnly;

    /// Items
    public BindingList<StswListBoxTestModel> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }
    private BindingList<StswListBoxTestModel> _items = new(Enumerable.Range(1, 15).Select(i => new StswListBoxTestModel { Name = "Option " + i, IsSelected = new Random().Next(2) == 0 }).ToList());

    /// SelectionCounter
    public int SelectionCounter => Items.AsEnumerable().Count(x => x.IsSelected);

    /// SubControls
    public bool SubControls
    {
        get => _subControls;
        set => SetProperty(ref _subControls, value);
    }
    private bool _subControls = false;
}
