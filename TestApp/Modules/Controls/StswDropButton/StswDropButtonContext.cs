﻿using System.Windows.Input;
using System.Windows;

namespace TestApp;

public class StswDropButtonContext : StswObservableObject
{
    public ICommand ClickCommand { get; set; }

    public StswDropButtonContext()
    {
        ClickCommand = new StswRelayCommand<string>(Click);
    }

    /// ClickCommand
    private void Click(string parameter)
    {
        MessageBox.Show(parameter);
    }
}