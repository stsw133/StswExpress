﻿using System;
using System.ComponentModel;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// A command implementation (without parameter) that can be used to bind to UI controls in order to execute a given action when triggered.
/// </summary>
public sealed class StswCommand : ICommand, INotifyPropertyChanged
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public StswCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    /// <summary>
    /// 
    /// </summary>
    public void Execute(object? parameter)
    {
        IsWorking = true;
        _execute();
        IsWorking = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsWorking
    {
        get => isWorking;
        private set
        {
            isWorking = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsWorking)));
        }
    }
    private bool isWorking;

    /// Notify the view that the IsWorking property has changed
    public event PropertyChangedEventHandler? PropertyChanged;
}
