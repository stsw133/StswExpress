﻿using System.Collections.Generic;
using System.Windows.Input;

namespace TestApp;

public class StswComboBoxContext : ControlsContext
{
    #region Properties
    /// IsEditable
    private bool isEditable = false;
    public bool IsEditable
    {
        get => isEditable;
        set => SetProperty(ref isEditable, value);
    }

    /// IsReadOnly
    private bool isReadOnly = false;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// Items
    private List<string?> items = new() { "Option 1", "Option 2", "Option 3", "Option 4", "Option 5" };
    public List<string?> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }
    /// SelectedItem
    private string? selectedItem = "Option 4";
    public string? SelectedItem
    {
        get => selectedItem;
        set => SetProperty(ref selectedItem, value);
    }
    #endregion
}