using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TestApp;

public class ContractorsListContext : StswObservableObject
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

    /// Item commands
    public ICommand AddPdfCommand { get; set; }

    public ContractorsListContext()
    {
        SQL.InitializeContractorsTables();
        ClearCommand = new StswCommand(Clear);
        RefreshCommand = new StswCommand(Refresh);
        SaveCommand = new StswCommand(Save);
        ExportCommand = new StswCommand(Export);
        AddCommand = new StswCommand(Add);
        CloneCommand = new StswCommand(Clone, CloneCondition);
        EditCommand = new StswCommand(Edit, EditCondition);
        DeleteCommand = new StswCommand(Delete, DeleteCondition);
        AddPdfCommand = new StswCommand(AddPdf, AddPdfCondition);
    }

    #region Commands
    /// Clear
    private async void Clear()
    {
        if (IsBusy[nameof(Clear)]) return;

        IsBusy[nameof(Clear)] = true;
        LoadingActions++;

        /// ...
        await Task.Run(() => ListContractors = new());
        /// ...

        LoadingActions--;
        IsBusy[nameof(Clear)] = false;
    }

    /// Refresh
    private async void Refresh()
    {
        if (IsBusy[nameof(Refresh)]) return;

        IsBusy[nameof(Refresh)] = true;
        LoadingActions++;

        /// ...
        FiltersContractors.Refresh?.Invoke();
        await Task.Run(() => ListContractors = SQL.GetContractors(FiltersContractors.SqlFilter, FiltersContractors.SqlParameters));
        /// ...

        LoadingActions--;
        IsBusy[nameof(Refresh)] = false;
    }

    /// Save
    private void Save()
    {
        if (IsBusy[nameof(Save)]) return;

        IsBusy[nameof(Save)] = true;
        LoadingActions++;

        /// ...
        if (SQL.SetContractors(ListContractors))
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

        IsBusy[nameof(Export)] = true;
        LoadingActions++;

        /// ...
        await Task.Run(() => StswExport.ExportToExcel(("Sheet1", ListContractors), null, true));
        /// ...

        LoadingActions--;
        IsBusy[nameof(Export)] = false;
    }

    /// Add
    private void Add()
    {
        if (IsBusy[nameof(Add)]) return;

        IsBusy[nameof(Add)] = true;
        LoadingActions++;

        /// ...
        if (ContractorsContext.Tabs_.FirstOrDefault() is StswTabItem tabItem and not null)
        {
            if (StswFn.FindVisualAncestor<StswTabControl>(tabItem) is StswTabControl tabControl)
            {
                tabControl.PART_FunctionButton_Click(this, new RoutedEventArgs());
                //if (ContractorsContext.Tabs_[tabControl.SelectedIndex]?.DataContext is ContractorsSingleContext newContext)
                //{
                //    newContext.ID = 0;
                //    newContext.IsCloned = false;
                //
                //    if (ContractorsContext.Tabs_[tabControl.SelectedIndex]?.Header is StswHeader header and not null)
                //    {
                //        header.Content = $"New contractor";
                //        header.IconData = StswIcons.AccountPlus;
                //    }
                //}
            }
        }

        /// old methods with using StswNavigation
        //var navi = StswFn.FindVisualChild<StswNavigation>(Application.Current.MainWindow);
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

        /// ...
        if (SelectedContractor is ContractorModel m && m.ID > 0)
        {
            if (ContractorsContext.Tabs_.FirstOrDefault() is StswTabItem tabItem and not null)
            {
                if (StswFn.FindVisualAncestor<StswTabControl>(tabItem) is StswTabControl tabControl)
                {
                    tabControl.PART_FunctionButton_Click(this, new RoutedEventArgs());
                    if (ContractorsContext.Tabs_[tabControl.SelectedIndex]?.Content is ContractorsSingleContext newContext)
                    {
                        newContext.ID = m.ID;
                        newContext.IsCloned = true;

                        if (ContractorsContext.Tabs_[tabControl.SelectedIndex]?.Header is StswHeader header and not null)
                        {
                            header.Content = $"Cloning contractor (ID: {newContext.ID})";
                            header.IconData = StswIcons.AccountPlus;
                        }
                    }
                }
            }

            /// old methods with using StswNavigation
            //var navi = StswFn.FindVisualChild<StswNavigation>(Application.Current.MainWindow);
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
    private bool CloneCondition() => SelectedContractor is ContractorModel m && m.ID > 0;

    /// Edit
    private void Edit()
    {
        if (IsBusy[nameof(Edit)]) return;

        IsBusy[nameof(Edit)] = true;
        LoadingActions++;

        /// ...
        if (SelectedContractor is ContractorModel m && m.ID > 0)
        {
            if (ContractorsContext.Tabs_.FirstOrDefault() is StswTabItem tabItem and not null)
            {
                if (StswFn.FindVisualAncestor<StswTabControl>(tabItem) is StswTabControl tabControl)
                {
                    tabControl.PART_FunctionButton_Click(this, new RoutedEventArgs());

                    if (ContractorsContext.Tabs_[tabControl.SelectedIndex]?.Content is ContractorsSingleContext newContext)
                    {
                        newContext.ID = m.ID;
                        newContext.IsCloned = false;

                        if (ContractorsContext.Tabs_[tabControl.SelectedIndex]?.Header is StswHeader header and not null)
                        {
                            header.Content = $"Editing contractor (ID: {newContext.ID})";
                            header.IconData = StswIcons.AccountEdit;
                        }
                    }
                }
            }

            /// old methods with using StswNavigation
            //var navi = StswFn.FindVisualChild<StswNavigation>(Application.Current.MainWindow);
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
    private bool EditCondition() => SelectedContractor is ContractorModel m && m.ID > 0;

    /// Delete
    private void Delete()
    {
        if (IsBusy[nameof(Delete)]) return;

        IsBusy[nameof(Delete)] = true;
        LoadingActions++;

        /// ...
        if (SelectedContractor is ContractorModel m)
        {
            if (m.ID == 0)
                ListContractors.Remove(m);
            else if (m.ID > 0 && MessageBox.Show("Are you sure you want to delete selected item?", string.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (SQL.DeleteContractor(m.ID))
                    ListContractors.Remove(m);
            }
        }
        /// ...

        LoadingActions--;
        IsBusy[nameof(Delete)] = false;
    }
    private bool DeleteCondition() => SelectedContractor is ContractorModel m and not null;

    /// AddPdf
    private void AddPdf()
    {
        if (IsBusy[nameof(AddPdf)]) return;

        IsBusy[nameof(AddPdf)] = true;
        LoadingActions++;

        /// ...
        if (SelectedContractor is ContractorModel m && m.ID > 0)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "PDF files|*.pdf"
            };
            if (dialog.ShowDialog() == true)
                SQL.AddPdf(m.ID, File.ReadAllBytes(dialog.FileName));
        }
        /// ...

        LoadingActions--;
        IsBusy[nameof(AddPdf)] = false;
    }
    private bool AddPdfCondition() => SelectedContractor is ContractorModel m && m.ID > 0;
    #endregion

    #region Properties
    /// Busy & loading
    private StswDictionary<string, bool> isBusy = new();
    public StswDictionary<string, bool> IsBusy
    {
        get => isBusy;
        set => SetProperty(ref isBusy, value);
    }
    private int loadingActions = 0;
    public int LoadingActions
    {
        get => loadingActions;
        set
        {
            SetProperty(ref loadingActions, value);

            if (!LoadingState.In(StswProgressState.Paused, StswProgressState.Error))
                LoadingState = LoadingActions > 0 ? StswProgressState.Running : StswProgressState.Ready;
        }
    }
    private StswProgressState loadingState;
    public StswProgressState LoadingState
    {
        get => loadingState;
        set => SetProperty(ref loadingState, value);
    }

    /// Combo sources
    public static ObservableCollection<StswComboItem> ComboSourceContractorTypes => SQL.ListOfContractorTypes();

    /// Data collections
    private StswCollection<ContractorModel> listContractors = new();
    public StswCollection<ContractorModel> ListContractors
    {
        get => listContractors;
        set => SetProperty(ref listContractors, value);
    }

    /// Filters data
    private StswDataGridFiltersDataModel filtersContractors = new();
    public StswDataGridFiltersDataModel FiltersContractors
    {
        get => filtersContractors;
        set => SetProperty(ref filtersContractors, value);
    }

    /// Selected items
    private object? selectedContractor = new();
    public object? SelectedContractor
    {
        get => selectedContractor;
        set => SetProperty(ref selectedContractor, value);
    }
    #endregion
}
