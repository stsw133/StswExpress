using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TestApp.Modules.Contractors;

public class ContractorsContext : StswContext
{
    /// ColumnFilters
    private ExtDictionary<string, StswColumnFilterData> columnFilters = new();
    public ExtDictionary<string, StswColumnFilterData> ColumnFilters
    {
        get => columnFilters;
        set => SetProperty(ref columnFilters, value, () => ColumnFilters);
    }

    /// IsBusy
    private ExtDictionary<string, bool> isBusy = new();
    public ExtDictionary<string, bool> IsBusy
    {
        get => isBusy;
        set => SetProperty(ref isBusy, value, () => IsBusy);
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
            SetProperty(ref countActions, value, () => CountActions);
            NotifyPropertyChanged(nameof(IsLoading));
        }
    }
    public bool IsLoading => CountActions > 0;

    /// ListContractors
    private ExtCollection<ContractorModel> listContractors = new();
    public ExtCollection<ContractorModel> ListContractors
    {
        get => listContractors;
        set => SetProperty(ref listContractors, value, () => ListContractors);
    }

    /// Commands
    public StswRelayCommand ClearCommand { get; set; }
    public StswRelayCommand RefreshCommand { get; set; }
    public StswRelayCommand SaveCommand { get; set; }
    public StswRelayCommand ExportToCsvCommand { get; set; }
    public StswRelayCommand AddCommand { get; set; }
    public StswRelayCommand EditCommand { get; set; }
    public StswRelayCommand DeleteCommand { get; set; }

    public ContractorsContext()
    {
        ClearCommand = new StswRelayCommand(o => Clear(), o => true);
        RefreshCommand = new StswRelayCommand(o => Refresh(), o => true);
        SaveCommand = new StswRelayCommand(o => Save(), o => true);
        ExportToCsvCommand = new StswRelayCommand(o => ExportToCsv(), o => true);
        AddCommand = new StswRelayCommand(o => Add(), o => true);
        EditCommand = new StswRelayCommand(o => Edit(), o => true);
        DeleteCommand = new StswRelayCommand(o => Delete(), o => true);
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
            MessageBox.Show("Data saved successfully.");

        CountActions--;
        IsBusy[nameof(Save)] = false;
    }

    /// ExportToCsv
    private async void ExportToCsv()
    {
        if (IsBusy[nameof(ExportToCsv)]) return;
        
        await Task.Run(() =>
        {
            IsBusy[nameof(ExportToCsv)] = true;
            CountActions++;

            Thread.Sleep(100);
            ListContractors.ExportToCsv(null, true, "\t");

            CountActions--;
            IsBusy[nameof(ExportToCsv)] = false;
        });
    }

    /// SelectedItem
    private object? selectedItem = new();
    public object? SelectedItem
    {
        get => selectedItem;
        set => SetProperty(ref selectedItem, value, () => SelectedItem);
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
