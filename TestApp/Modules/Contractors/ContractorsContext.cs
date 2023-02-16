using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TestApp.Modules.Contractors;

public class ContractorsContext : StswObservableObject
{
    /// ColumnFilters
    private StswDictionary<string, StswColumnFilterBindingData> columnFilters = new();
    public StswDictionary<string, StswColumnFilterBindingData> ColumnFilters
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

    /// ComboLists
    public List<string?> ListTypes => ContractorsQueries.ListOfTypes();

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
    public ICommand CloneCommand { get; set; }
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
        CloneCommand = new StswRelayCommand(Clone, CloneCondition);
        EditCommand = new StswRelayCommand(Edit, EditCondition);
        DeleteCommand = new StswRelayCommand(Delete, DeleteCondition);
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

            /// ...
            ListContractors = new();
            /// ...

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

            /// ...
            ListContractors = ContractorsQueries.GetContractors(filter, parameters);
            /// ...

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

        /// ...
        if (ContractorsQueries.SetContractors(ListContractors))
        {
            Refresh();
            MessageBox.Show("Data saved successfully.");
        }
        /// ...

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

            /// ...
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
            /// ...

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

        /// ...
        var navi = StswExtensions.FindVisualChild<StswNavigation>(Application.Current.MainWindow);
        navi?.PageChange(typeof(ContractorsSingle.ContractorsSingleView).FullName ?? string.Empty, true);
        /// ...

        CountActions--;
        IsBusy[nameof(Add)] = false;
    }

    /// Clone
    private void Clone()
    {
        if (IsBusy[nameof(Clone)]) return;

        IsBusy[nameof(Clone)] = true;
        CountActions++;
        Thread.Sleep(100);

        /// ...
        var selectedItem = SelectedItem as ContractorModel;
        if (selectedItem?.ID > 0)
        {
            var navi = StswExtensions.FindVisualChild<StswNavigation>(Application.Current.MainWindow);
            if (navi?.PageChange(typeof(ContractorsSingle.ContractorsSingleView).FullName ?? string.Empty, true) is ContractorsSingle.ContractorsSingleView page)
                ((ContractorsSingle.ContractorsSingleContext)page.DataContext).ID = selectedItem.ID;
        }
        /// ...

        CountActions--;
        IsBusy[nameof(Clone)] = false;
    }
    private bool CloneCondition() => SelectedItem is ContractorModel and not null;
    
    /// Edit
    private void Edit()
    {
        if (IsBusy[nameof(Edit)]) return;

        IsBusy[nameof(Edit)] = true;
        CountActions++;
        Thread.Sleep(100);

        /// ...
        var selectedItem = SelectedItem as ContractorModel;
        if (selectedItem?.ID > 0)
        {
            var navi = StswExtensions.FindVisualChild<StswNavigation>(Application.Current.MainWindow);
            if (navi?.PageChange(typeof(ContractorsSingle.ContractorsSingleView).FullName ?? string.Empty, true) is ContractorsSingle.ContractorsSingleView page)
                ((ContractorsSingle.ContractorsSingleContext)page.DataContext).ID = selectedItem.ID;
        }
        /// ...

        CountActions--;
        IsBusy[nameof(Edit)] = false;
    }
    private bool EditCondition() => SelectedItem is ContractorModel and not null;

    /// Delete
    private void Delete()
    {
        if (IsBusy[nameof(Delete)]) return;

        IsBusy[nameof(Delete)] = true;
        CountActions++;
        Thread.Sleep(100);

        /// ...
        var selectedItem = SelectedItem as ContractorModel;
        if (selectedItem?.ID == 0)
            ListContractors.Remove(selectedItem);
        else if (selectedItem?.ID > 0 && MessageBox.Show("Are you sure you want to delete selected item?", string.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            if (ContractorsQueries.DeleteContractor(selectedItem.ID))
                ListContractors.Remove(selectedItem);
        }
        /// ...

        CountActions--;
        IsBusy[nameof(Delete)] = false;
    }
    private bool DeleteCondition() => SelectedItem is ContractorModel and not null;
}
