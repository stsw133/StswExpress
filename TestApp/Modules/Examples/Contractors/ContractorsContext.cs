using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace TestApp;

public class ContractorsContext : StswObservableObject
{
    /// Grid commands
    public ICommand ClearCommand { get; set; }
    public ICommand RefreshCommand { get; set; }
    public ICommand SaveCommand { get; set; }
    public ICommand ExportCommand { get; set; }

    /// Instances commands
    public ICommand AddCommand { get; set; }
    public ICommand CloneCommand { get; set; }
    public ICommand EditCommand { get; set; }
    public ICommand DeleteCommand { get; set; }

    public ContractorsContext()
    {
        Task.Run(Init);
        ClearCommand = new StswAsyncCommand(Clear);
        RefreshCommand = new StswAsyncCommand(Refresh);
        SaveCommand = new StswAsyncCommand(Save);
        ExportCommand = new StswAsyncCommand(Export);
        AddCommand = new StswAsyncCommand(Add);
        CloneCommand = new StswAsyncCommand(Clone, CloneCondition);
        EditCommand = new StswAsyncCommand(Edit, EditCondition);
        DeleteCommand = new StswAsyncCommand(Delete, DeleteCondition);
    }

    /// Init
    private async Task Init()
    {
        try
        {
            SQL.InitializeContractorsTables();
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, "Error");
        }
    }

    /// Clear
    private async Task Clear()
    {
        try
        {
            await Task.Run(() => ListContractors = []);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, "Error");
        }
    }

    /// Refresh
    private async Task Refresh()
    {
        try
        {
            FiltersContractors.Refresh?.Invoke();
            await Task.Run(() => ListContractors = SQL.GetContractors(FiltersContractors.SqlFilter!, FiltersContractors.SqlParameters!).ToStswBindingList());
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, "Error");
        }
    }

    /// Save
    private async Task Save()
    {
        try
        {
            await Task.Run(() => SQL.SetContractors(ListContractors));
            RefreshCommand.Execute(null);
            await StswMessageDialog.Show("Data saved successfully.", nameof(TestApp), null, StswDialogButtons.OK, StswDialogImage.Success);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, "Error");
        }
    }

    /// Export
    private async Task Export()
    {
        try
        {
            //await Task.Run(() => StswExport.ExportToExcel("Sheet1", ListContractors, null, new() { OpenFile = true }));
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, "Error");
        }
    }

    /// Add
    private async Task Add()
    {
        try
        {
            await Task.Run(() => App.Current.Dispatcher.Invoke(() =>
            {
                NewTabCommand?.Execute(null);
                // if (NewTab.Content is ContractorsSingleContext context)
                // {
                //     context.ID = 0;
                //     context.IsCloned = false;
                // }
                // if (NewTab.Header is StswLabel header)
                // {
                //     header.Content = "New contractor";
                //     header.IconData = StswIcons.AccountPlus;
                // }
            }));
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, "Error");
        }
    }

    /// Clone
    private async Task Clone()
    {
        try
        {
            if (SelectedContractor is ContractorModel m && m.ID > 0)
            {
                await Task.Run(() => App.Current.Dispatcher.Invoke(() =>
                {
                    NewTabCommand?.Execute(null);
                    if (NewTab.Content is ContractorsSingleContext context)
                    {
                        context.ID = m.ID;
                        context.IsCloned = true;
                    }
                    if (NewTab.Header is StswLabel header)
                    {
                        header.Content = $"Cloning contractor (ID: {m.ID})";
                        header.IconData = StswIcons.AccountPlus;
                    }
                }));
            }
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, "Error");
        }
    }
    private bool CloneCondition() => SelectedContractor is ContractorModel m && m.ID > 0;

    /// Edit
    private async Task Edit()
    {
        try
        {
            if (SelectedContractor is ContractorModel m && m.ID > 0)
            {
                await Task.Run(() => App.Current.Dispatcher.Invoke(() =>
                {
                    NewTabCommand?.Execute(null);
                    if (NewTab.Content is ContractorsSingleContext context)
                    {
                        context.ID = m.ID;
                        context.IsCloned = false;
                    }
                    if (NewTab.Header is StswLabel header)
                    {
                        header.Content = $"Editing contractor (ID: {m.ID})";
                        header.IconData = StswIcons.AccountEdit;
                    }
                }));
            }
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, "Error");
        }
    }
    private bool EditCondition() => SelectedContractor is ContractorModel m && m.ID > 0;

    /// Delete
    private async Task Delete()
    {
        try
        {
            if (SelectedContractor is ContractorModel m)
            {
                await Task.Run(() =>
                {
                    if (m.ID == 0)
                    {
                        ListContractors.Remove(m);
                    }
                    else if (m.ID > 0 && MessageBox.Show("Are you sure you want to delete selected item?", string.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        SQL.DeleteContractor(m.ID);
                        ListContractors.Remove(m);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, "Error");
        }
    }
    private bool DeleteCondition() => SelectedContractor is ContractorModel;

    /// FiltersContractors
    public StswDataGridFiltersDataModel FiltersContractors
    {
        get => _filtersContractors;
        set => SetProperty(ref _filtersContractors, value);
    }
    private StswDataGridFiltersDataModel _filtersContractors = new();

    /// ListContractors
    public StswBindingList<ContractorModel> ListContractors
    {
        get => _listContractors;
        set => SetProperty(ref _listContractors, value);
    }
    private StswBindingList<ContractorModel> _listContractors = [];

    /// NewTab
    public StswTabItem NewTab
    {
        get => _newTab;
        set
        {
            value.Content = new ContractorsSingleContext();
            value.Header = new StswLabel() { Content = "New contractor", IconData = StswIcons.Plus };
            value.IsClosable = true;
            SetProperty(ref _newTab, value);
        }
    }
    private StswTabItem _newTab = new();

    /// NewTabCommand
    public ICommand? NewTabCommand { get; set; }

    /// Selected items
    public object? SelectedContractor
    {
        get => _selectedContractor;
        set => SetProperty(ref _selectedContractor, value);
    }
    private object? _selectedContractor = new();
}
