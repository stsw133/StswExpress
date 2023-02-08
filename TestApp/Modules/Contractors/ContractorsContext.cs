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

    public ContractorsContext()
    {
        ClearCommand = new StswRelayCommand(o => Clear(), o => true);
        RefreshCommand = new StswRelayCommand(o => Refresh(), o => true);
        SaveCommand = new StswRelayCommand(o => Save(), o => true);
        ExportToCsvCommand = new StswRelayCommand(o => ExportToCsv(), o => true);
    }
    
    /// Clear
    private async void Clear()
    {
        if (IsBusy[nameof(Clear)]) return;
        IsBusy[nameof(Clear)] = true;
        CountActions++;

        await Task.Run(() =>
        {
            Thread.Sleep(100);
            ListContractors = new();
        });

        CountActions--;
        IsBusy[nameof(Clear)] = false;
    }

    /// Refresh
    private async void Refresh()
    {
        if (IsBusy[nameof(Refresh)]) return;
        IsBusy[nameof(Refresh)] = true;
        CountActions++;

        ColumnFilters.GetColumnFilters(out var filter, out var parameters);
        await Task.Run(() =>
        {
            Thread.Sleep(100);
            ListContractors = ContractorsQueries.GetContractors(filter, parameters);
        });

        CountActions--;
        IsBusy[nameof(Refresh)] = false;
    }

    /// Save
    private void Save()
    {
        if (IsBusy[nameof(Save)]) return;
        IsBusy[nameof(Save)] = true;
        CountActions++;

        if (ContractorsQueries.SetContractors(ListContractors))
            MessageBox.Show("Data saved successfully.");

        CountActions--;
        IsBusy[nameof(Save)] = false;
    }

    /// ExportToCsv
    private async void ExportToCsv()
    {
        if (IsBusy[nameof(ExportToCsv)]) return;
        IsBusy[nameof(ExportToCsv)] = true;
        CountActions++;

        await Task.Run(() =>
        {
            Thread.Sleep(100);
            ListContractors.ExportToCsv(null, true, "\t");
        });

        CountActions--;
        IsBusy[nameof(ExportToCsv)] = false;
    }
}
