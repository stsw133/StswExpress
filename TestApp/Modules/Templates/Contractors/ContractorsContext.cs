using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TestApp;

public class ContractorsContext : StswObservableObject
{
    #region Commands

    public ICommand ClearCommand { get; set; }
    public ICommand RefreshCommand { get; set; }
    public ICommand SaveCommand { get; set; }
    public ICommand ExportCommand { get; set; }
    public ICommand AddCommand { get; set; }
    public ICommand CloneCommand { get; set; }
    public ICommand EditCommand { get; set; }
    public ICommand DeleteCommand { get; set; }
    public ICommand AddPdfCommand { get; set; }

    public ContractorsContext()
    {
        ContractorsQueries.InitializeTables();
        ClearCommand = new StswRelayCommand(Clear);
        RefreshCommand = new StswRelayCommand(Refresh);
        SaveCommand = new StswRelayCommand(Save);
        ExportCommand = new StswRelayCommand(Export);
        AddCommand = new StswRelayCommand(Add);
        CloneCommand = new StswRelayCommand(Clone, CloneCondition);
        EditCommand = new StswRelayCommand(Edit, EditCondition);
        DeleteCommand = new StswRelayCommand(Delete, DeleteCondition);
        AddPdfCommand = new StswRelayCommand(AddPdf, AddPdfCondition);
    }

    /// Clear
    private async void Clear()
    {
        if (IsBusy[nameof(Clear)]) return;

        await Task.Run(() =>
        {
            IsBusy[nameof(Clear)] = true;
            LoadingActions++;
            Thread.Sleep(100);

            /// ...
            ListContractors = new();
            /// ...

            LoadingActions--;
            IsBusy[nameof(Clear)] = false;
        });
    }

    /// Refresh
    private async void Refresh()
    {
        if (IsBusy[nameof(Refresh)]) return;

        FiltersData.Refresh?.Invoke();
        await Task.Run(() =>
        {
            IsBusy[nameof(Refresh)] = true;
            LoadingActions++;
            Thread.Sleep(100);

            /// ...
            ListContractors = ContractorsQueries.GetContractors(FiltersData.SqlFilter, FiltersData.SqlParameters);
            /// ...

            LoadingActions--;
            IsBusy[nameof(Refresh)] = false;
        });
    }

    /// Save
    private void Save()
    {
        if (IsBusy[nameof(Save)]) return;

        IsBusy[nameof(Save)] = true;
        LoadingActions++;
        Thread.Sleep(100);

        /// ...
        if (ContractorsQueries.SetContractors(ListContractors))
        {
            Refresh();
            MessageBox.Show("Data saved successfully.");
        }
        /// ...

        LoadingActions--;
        IsBusy[nameof(Save)] = false;
    }

    /// Export
    private async void Export()
    {
        if (IsBusy[nameof(Export)]) return;

        await Task.Run(() =>
        {
            IsBusy[nameof(Export)] = true;
            LoadingActions++;
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

            LoadingActions--;
            IsBusy[nameof(Export)] = false;
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
        LoadingActions++;
        Thread.Sleep(100);

        /// ...
        var navi = StswFn.FindVisualChild<StswNavigation>(Application.Current.MainWindow);
        //navi?.ContextChange(new ContractorsSingleView()
        //{
        //    DataContext = new ContractorsSingleContext()
        //}, true);

        // or (if you set DataContext in XAML):
        /*
        var navi = StswExtensions.FindVisualChild<StswNavigation>(Application.Current.MainWindow);
        navi?.PageChange(typeof(ContractorsSingle.ContractorsSingleView).FullName ?? string.Empty, true);
        */
        /// ...

        LoadingActions--;
        IsBusy[nameof(Add)] = false;
    }

    /// Clone
    private void Clone()
    {
        if (IsBusy[nameof(Clone)]) return;

        IsBusy[nameof(Clone)] = true;
        LoadingActions++;
        Thread.Sleep(100);

        /// ...
        var selectedItem = SelectedItem as ContractorModel;
        if (selectedItem?.ID > 0)
        {
            var navi = StswFn.FindVisualChild<StswNavigation>(Application.Current.MainWindow);
            //navi?.ContextChange(new ContractorsSingleView()
            //{
            //    DataContext = new ContractorsSingleContext()
            //    {
            //        ID = selectedItem.ID,
            //        DoClone = true
            //    }
            //}, true);

            // or (if you set DataContext in XAML):
            /*
            var navi = StswExtensions.FindVisualChild<StswNavigation>(Application.Current.MainWindow);
            if (navi?.PageChange(typeof(ContractorsSingle.ContractorsSingleView).FullName ?? string.Empty, true) is ContractorsSingle.ContractorsSingleView page)
            {
                ((ContractorsSingle.ContractorsSingleContext)page.DataContext).ID = selectedItem.ID;
                ((ContractorsSingle.ContractorsSingleContext)page.DataContext).DoClone = true;
            }
            */
        }
        /// ...

        LoadingActions--;
        IsBusy[nameof(Clone)] = false;
    }
    private bool CloneCondition() => SelectedItem is ContractorModel and not null;

    /// Edit
    private void Edit()
    {
        if (IsBusy[nameof(Edit)]) return;

        IsBusy[nameof(Edit)] = true;
        LoadingActions++;
        Thread.Sleep(100);

        /// ...
        var selectedItem = SelectedItem as ContractorModel;
        if (selectedItem?.ID > 0)
        {
            var navi = StswFn.FindVisualChild<StswNavigation>(Application.Current.MainWindow);
            //navi?.ContextChange(new ContractorsSingleView()
            //{
            //    DataContext = new ContractorsSingleContext()
            //    {
            //        ID = selectedItem.ID,
            //        DoClone = false
            //    }
            //}, true);

            // or (if you set DataContext in XAML):
            /*
            var navi = StswExtensions.FindVisualChild<StswNavigation>(Application.Current.MainWindow);
            if (navi?.PageChange(typeof(ContractorsSingle.ContractorsSingleView).FullName ?? string.Empty, true) is ContractorsSingle.ContractorsSingleView page)
            {
                ((ContractorsSingle.ContractorsSingleContext)page.DataContext).ID = selectedItem.ID;
                ((ContractorsSingle.ContractorsSingleContext)page.DataContext).DoClone = false;
            }
            */
        }
        /// ...

        LoadingActions--;
        IsBusy[nameof(Edit)] = false;
    }
    private bool EditCondition() => SelectedItem is ContractorModel and not null;

    /// Delete
    private void Delete()
    {
        if (IsBusy[nameof(Delete)]) return;

        IsBusy[nameof(Delete)] = true;
        LoadingActions++;
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

        LoadingActions--;
        IsBusy[nameof(Delete)] = false;
    }
    private bool DeleteCondition() => SelectedItem is ContractorModel and not null;

    /// AddPdf
    private void AddPdf()
    {
        if (IsBusy[nameof(AddPdf)]) return;

        IsBusy[nameof(AddPdf)] = true;
        LoadingActions++;
        Thread.Sleep(100);

        /// ...
        var selectedItem = SelectedItem as ContractorModel;
        if (selectedItem?.ID > 0)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "PDF files|*.pdf"
            };
            if (dialog.ShowDialog() == true)
                ContractorsQueries.AddPdf(selectedItem.ID, File.ReadAllBytes(dialog.FileName));
        }
        /// ...

        LoadingActions--;
        IsBusy[nameof(AddPdf)] = false;
    }
    private bool AddPdfCondition() => SelectedItem is ContractorModel m and not null && m.ID > 0;

    #endregion

    /// FiltersData
    private StswDataGridFiltersDataModel filtersData = new();
    public StswDataGridFiltersDataModel FiltersData
    {
        get => filtersData;
        set => SetProperty(ref filtersData, value);
    }

    /// IsBusy
    private StswDictionary<string, bool> isBusy = new();
    public StswDictionary<string, bool> IsBusy
    {
        get => isBusy;
        set => SetProperty(ref isBusy, value);
    }

    /// Loading
    private int loadingActions = 0;
    public int LoadingActions
    {
        get => loadingActions;
        set
        {
            SetProperty(ref loadingActions, value);
            NotifyPropertyChanged(nameof(LoadingState));
        }
    }
    public StswProgressBar.States LoadingState => LoadingActions > 0 ? StswProgressBar.States.Running : StswProgressBar.States.Ready;

    /// ComboLists
    public List<string?> ListTypes => ContractorsQueries.ListOfTypes();

    /// ListContractors
    private StswCollection<ContractorModel> listContractors = new();
    public StswCollection<ContractorModel> ListContractors
    {
        get => listContractors;
        set => SetProperty(ref listContractors, value);
    }
    //public int ListContractors_Added => ListContractors.GetItemsByState(System.Data.DataRowState.Added).Count;
    //public int ListContractors_Modified => ListContractors.GetItemsByState(System.Data.DataRowState.Modified).Count;
    //public int ListContractors_Deleted => ListContractors.GetItemsByState(System.Data.DataRowState.Deleted).Count;
}
