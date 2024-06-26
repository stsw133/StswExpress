﻿using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TestApp;

public class DatabasesContext : StswObservableObject
{
    public ICommand MoveUpCommand { get; set; }
    public ICommand MoveDownCommand { get; set; }
    public ICommand AddCommand { get; set; }
    public ICommand RemoveCommand { get; set; }
    public ICommand ImportCommand { get; set; }
    public ICommand ExportCommand { get; set; }

    public DatabasesContext()
    {
        MoveUpCommand = new StswCommand(MoveUp);
        MoveDownCommand = new StswCommand(MoveDown);
        AddCommand = new StswCommand(Add);
        RemoveCommand = new StswCommand(Remove);
        ImportCommand = new StswAsyncCommand(Import);
        ExportCommand = new StswAsyncCommand(Export);
    }

    #region Commands & methods
    /// MoveUp
    private void MoveUp()
    {
        if (StswDatabases.List.IndexOf(SelectedDatabase!) is int i and > 0)
            StswDatabases.List.Move(i, i - 1);
    }

    /// MoveDown
    private void MoveDown()
    {
        if (StswDatabases.List.IndexOf(SelectedDatabase!) is int i and >= 0 && i < StswDatabases.List.Count - 1)
            StswDatabases.List.Move(i, i + 1);
    }

    /// Add
    private void Add()
    {
        var newDatabase = new StswDatabaseModel();
        StswDatabases.List.Add(newDatabase);
        SelectedDatabase = newDatabase;
    }

    /// Remove
    private void Remove()
    {
        if (SelectedDatabase != null)
            StswDatabases.List.Remove(SelectedDatabase);
        SelectedDatabase = null;
    }

    /// Import
    private async Task Import()
    {
        StswDatabases.ImportList();
        await Task.Run(() => StswDatabases.Current = StswDatabases.List.FirstOrDefault() ?? new());
        SelectedDatabase = StswDatabases.Current;
    }

    /// Export
    private async Task Export()
    {
        await Task.Run(() => StswDatabases.ExportList());
    }
    #endregion

    /// SelectedDatabase
    public StswDatabaseModel? SelectedDatabase
    {
        get => _selectedDatabase;
        set => SetProperty(ref _selectedDatabase, value);
    }
    private StswDatabaseModel? _selectedDatabase = StswDatabases.Current ?? new();
}
