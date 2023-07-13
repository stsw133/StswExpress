﻿using System.Windows.Input;

namespace TestApp;

public class StswRepeatButtonContext : ControlsContext
{
    public ICommand OnClickCommand { get; set; }

    public StswRepeatButtonContext()
    {
        OnClickCommand = new StswRelayCommand(OnClick);
    }

    #region Events
    /// OnClickCommand
    private void OnClick() => ClickCounter++;
    #endregion

    #region Properties
    /// ClickCounter
    private int clickCounter;
    public int ClickCounter
    {
        get => clickCounter;
        set => SetProperty(ref clickCounter, value);
    }
    #endregion
}
