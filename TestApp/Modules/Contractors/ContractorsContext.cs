using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TestApp.Modules.Contractors;

public class ContractorsContext : StswObservableObject
{
    /// ColumnFilters
    private StswDictionary<string, StswColumnFilterData> columnFilters = new();
    public StswDictionary<string, StswColumnFilterData> ColumnFilters
    {
        get => columnFilters;
        set => SetProperty(ref columnFilters, value);
    }

    /// IsBusy
    private StswDictionary<string, bool> isBusy = new();
    public StswDictionary<string, bool> IsBusy
    {
        get => isBusy;
        set => SetProperty(ref isBusy, value);
    }

    /// ComboLists
    public List<string?> ListTypes => ContractorsQueries.ListOfTypes();
    public List<string?> SelectedTypes => new List<string?>() { "Test2", "Test3", null };

    /// IsLoading
    private int countActions = 0;
    public int CountActions
    {
        get => countActions;
        set
        {
            SetProperty(ref countActions, value);
            NotifyPropertyChanged(nameof(IsLoading));
        }
    }
    public bool IsLoading => CountActions > 0;

    /// ListContractors
    private StswCollection<ContractorModel> listContractors = new();
    public StswCollection<ContractorModel> ListContractors
    {
        get => listContractors;
        set => SetProperty(ref listContractors, value);
    }

    /// Commands
    public ICommand ClearCommand { get; set; }
    public ICommand RefreshCommand { get; set; }
    public ICommand SaveCommand { get; set; }
    public ICommand ExportToExcelCommand { get; set; }
    public ICommand AddCommand { get; set; }
    public ICommand EditCommand { get; set; }
    public ICommand DeleteCommand { get; set; }

    public ContractorsContext()
    {
        ContractorsQueries.InitializeTables();
        ClearCommand = new StswRelayCommand(Clear);
        RefreshCommand = new StswRelayCommand(Refresh);
        SaveCommand = new StswRelayCommand(Save);
        ExportToExcelCommand = new StswRelayCommand(ExportToExcel);
        AddCommand = new StswRelayCommand(Add);
        EditCommand = new StswRelayCommand(Edit);
        DeleteCommand = new StswRelayCommand(Delete);
    }
    
    /// Clear
    private async void Clear()
    {
        if (IsBusy[nameof(Clear)]) return;
        
        await Task.Run(() =>
        {
            IsBusy[nameof(Clear)] = true;
            CountActions++;

            Thread.Sleep(100);
            ListContractors = new();

            CountActions--;
            IsBusy[nameof(Clear)] = false;
        });
    }

    /// Refresh
    private async void Refresh()
    {
        if (IsBusy[nameof(Refresh)]) return;
        
        ColumnFilters.GetColumnFilters(out var filter, out var parameters);
        await Task.Run(() =>
        {
            IsBusy[nameof(Refresh)] = true;
            CountActions++;

            Thread.Sleep(100);
            ListContractors = ContractorsQueries.GetContractors(filter, parameters);

            CountActions--;
            IsBusy[nameof(Refresh)] = false;
        });
    }

    /// Save
    private void Save()
    {
        if (IsBusy[nameof(Save)]) return;

        IsBusy[nameof(Save)] = true;
        CountActions++;

        Thread.Sleep(100);
        if (ContractorsQueries.SetContractors(ListContractors))
        {
            Refresh();
            MessageBox.Show("Data saved successfully.");
        }

        CountActions--;
        IsBusy[nameof(Save)] = false;
    }

    /// ExportToExcel
    private async void ExportToExcel()
    {
        if (IsBusy[nameof(ExportToExcel)]) return;
        
        await Task.Run(() =>
        {
            IsBusy[nameof(ExportToExcel)] = true;
            CountActions++;

            Thread.Sleep(100);
            StswExport.ExportToExcel(ListContractors, null, true, new()
            {
                new() { FieldName = nameof(ContractorModel.ID), ColumnName = "ID" },
                new() { FieldName = nameof(ContractorModel.Type), ColumnName = "Type" },
                new() { FieldName = nameof(ContractorModel.Name), ColumnName = "Name" },
                new() { FieldName = nameof(ContractorModel.Country), ColumnName = "Country" },
                new() { FieldName = nameof(ContractorModel.PostCode), ColumnName = "Post code" },
                new() { FieldName = nameof(ContractorModel.City), ColumnName = "City" },
                new() { FieldName = nameof(ContractorModel.Street), ColumnName = "Street" },
                new() { FieldName = nameof(ContractorModel.IsArchival), ColumnName = "Is archival", ColumnFormat = "{0:yes;1;no}" },
                new() { FieldName = nameof(ContractorModel.CreateDT), ColumnName = "Date of creation", ColumnFormat = "yyyy-MM-dd" },
            });

            CountActions--;
            IsBusy[nameof(ExportToExcel)] = false;
        });
    }

    /// SelectedItem
    private object? selectedItem = new();
    public object? SelectedItem
    {
        get => selectedItem;
        set => SetProperty(ref selectedItem, value);
    }

    /// Add
    private void Add()
    {
        if (IsBusy[nameof(Add)]) return;
        
        IsBusy[nameof(Add)] = true;
        CountActions++;

        Thread.Sleep(100);
        var navi = StswExtensions.FindVisualChild<StswNavigation>(Application.Current.MainWindow);
        navi.PageChange(typeof(ContractorsSingle.ContractorsSingleView).FullName, true);
        //((navi.Content as Page).DataContext as ContractorsSingle.ContractorsSingleContext).ID = null;

        CountActions--;
        IsBusy[nameof(Add)] = false;
    }

    /// Edit
    private void Edit()
    {
        if (IsBusy[nameof(Edit)]) return;
        
        IsBusy[nameof(Edit)] = true;
        CountActions++;

        Thread.Sleep(100);
        var navi = StswExtensions.FindVisualChild<StswNavigation>(Application.Current.MainWindow);
        //navi.PageChange(typeof(ContractorsSingle.ContractorsSingleView).FullName, true);
        //navi.Pages[typeof(ContractorsSingle.ContractorsSingleView).FullName].DataContext.ID = SelectedItem.ID;
        //((navi.Content as Page).DataContext as ContractorsSingle.ContractorsSingleContext).ID = null;

        CountActions--;
        IsBusy[nameof(Edit)] = false;
    }

    /// Delete
    private void Delete()
    {
        if (IsBusy[nameof(Delete)]) return;
        
        IsBusy[nameof(Delete)] = true;
        CountActions++;

        Thread.Sleep(100);
        //var navi = StswExtensions.FindVisualChild<StswNavigation>(Application.Current.MainWindow);
        //navi.PageChange(typeof(ContractorsSingle.ContractorsSingleView).FullName, true);
        //((navi.Content as Page).DataContext as ContractorsSingle.ContractorsSingleContext).ID = null;

        CountActions--;
        IsBusy[nameof(Delete)] = false;
    }
}
