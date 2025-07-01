using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace TestApp;

public class ContractorsContext : StswObservableObject
{
    /// Grid commands
    public ICommand ClearCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ExportCommand { get; }

    /// Instances commands
    public ICommand AddCommand { get; }
    public ICommand CloneCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

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

        SelectedContractor = null;

        ListContractorsView = CollectionViewSource.GetDefaultView(_listContractors);
    }

    /// Init
    private async Task Init()
    {
        try
        {
            SQLService.InitializeContractorsTables();
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }

    /// Clear
    private async Task Clear()
    {
        try
        {
            ListContractors.Clear();
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }

    /// Refresh
    private async Task Refresh()
    {
        try
        {
            // for CollectionView filters:
            ListContractors = [.. await Task.Run(() => SQLService.GetContractors(null))];
            ListContractorsView = CollectionViewSource.GetDefaultView(ListContractors);
            FiltersContractors.Apply?.Invoke();

            // for SQL filters:
            //FiltersContractors.Apply?.Invoke();

            //IEnumerable<ContractorModel> list = [];
            //await Task.Run(() => list = SQL.GetContractors(FiltersContractors));
            //ListContractors = new(list);
            //ListContractorsView?.Refresh();
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }

    /// Save
    private async Task Save()
    {
        try
        {
            await Task.Run(() => SQLService.SetContractors(ListContractors));
            RefreshCommand.Execute(null);
            await StswMessageDialog.Show("Data saved successfully.", nameof(TestApp), null, StswDialogButtons.OK, StswDialogImage.Success);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
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
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
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
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
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
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
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
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
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
                        SQLService.DeleteContractor(m.ID);
                        ListContractors.Remove(m);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
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
    public StswObservableCollection<ContractorModel> ListContractors
    {
        get => _listContractors;
        set => SetProperty(ref _listContractors, value);
    }
    private StswObservableCollection<ContractorModel> _listContractors = [];
    
    /// ListContractorsView
    public ICollectionView? ListContractorsView
    {
        get => _listContractorsView;
        set => SetProperty(ref _listContractorsView, value);
    }
    private ICollectionView? _listContractorsView;

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
