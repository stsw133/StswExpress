﻿using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TestApp;

public class StswToggleSwitchContext : ControlsContext
{
    public ICommand OnClickCommand { get; set; }

    public StswToggleSwitchContext()
    {
        OnClickCommand = new StswCommand<string?>(OnClick);
    }

    #region Events and methods
    /// OnClickCommand
    private void OnClick(string? parameter)
    {
        if (int.TryParse(parameter, out var result))
            ClickOption = result;
    }
    #endregion

    #region Properties
    /// ClickOption
    private int clickOption;
    public int ClickOption
    {
        get => clickOption;
        set => SetProperty(ref clickOption, value);
    }
    /// SelectedOption
    private ObservableCollection<bool?> selectedOption = new() { null, false, false, true, false };
    public ObservableCollection<bool?> SelectedOption
    {
        get => selectedOption;
        set => SetProperty(ref selectedOption, value);
    }

    /// HasContent
    private bool hasContent = true;
    public bool HasContent
    {
        get => hasContent;
        set
        {
            SetProperty(ref hasContent, value);
            Content1 = value ? "Option 1" : null;
            Content2 = value ? "Option 2" : null;
            Content3 = value ? "Option 3" : null;
            Content4 = value ? "Option 4" : null;
            Content5 = value ? "Option 5" : null;
        }
    }
    private object? content1 = "Option 1";
    public object? Content1
    {
        get => content1;
        set => SetProperty(ref content1, value);
    }
    private object? content2 = "Option 2";
    public object? Content2
    {
        get => content2;
        set => SetProperty(ref content2, value);
    }
    private object? content3 = "Option 3";
    public object? Content3
    {
        get => content3;
        set => SetProperty(ref content3, value);
    }
    private object? content4 = "Option 4";
    public object? Content4
    {
        get => content4;
        set => SetProperty(ref content4, value);
    }
    private object? content5 = "Option 5";
    public object? Content5
    {
        get => content5;
        set => SetProperty(ref content5, value);
    }

    /// IsThreeState
    private bool isThreeState = false;
    public bool IsThreeState
    {
        get => isThreeState;
        set => SetProperty(ref isThreeState, value);
    }
    #endregion
}