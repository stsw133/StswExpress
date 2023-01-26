using System.Collections.Generic;
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

    /// ShowFilters
    private bool showFilters = true;
    public bool ShowFilters
    {
        get => showFilters;
        set => SetProperty(ref showFilters, value, () => ShowFilters);
    }
    /// ShowRowDetails
    private bool showRowDetails = true;
    public bool ShowRowDetails
    {
        get => showRowDetails;
        set => SetProperty(ref showRowDetails, value, () => ShowRowDetails);
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

    public ContractorsContext()
    {
        ClearCommand = new StswRelayCommand(o => Clear(), o => true);
        RefreshCommand = new StswRelayCommand(o => Refresh(), o => true);
        SaveCommand = new StswRelayCommand(o => Save(), o => true);
    }
    
    /// Clear
    private async void Clear()
    {
        ColumnFilters.ClearColumnFilters();
        await Task.Run(() =>
        {
            CountActions++;
            ListContractors = new();
            CountActions--;
        });
    }

    /// Refresh
    private async void Refresh()
    {
        ColumnFilters.GetColumnFilters(out var filter, out var parameters);
        await Task.Run(() =>
        {
            CountActions++;
            ListContractors = ContractorsQueries.GetContractors(filter, parameters);
            CountActions--;
        });
    }

    /// Save
    private void Save()
    {
        CountActions++;
        if (ContractorsQueries.SetContractors(ListContractors))
            MessageBox.Show("Data saved successfully.");
        CountActions--;
    }
}
