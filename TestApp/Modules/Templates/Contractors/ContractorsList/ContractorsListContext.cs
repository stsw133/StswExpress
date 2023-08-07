using Microsoft.Win32;
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
        ClearCommand = new StswAsyncCommand(Clear);
        RefreshCommand = new StswAsyncCommand(Refresh);
        SaveCommand = new StswAsyncCommand(Save);
        ExportCommand = new StswAsyncCommand(Export);
        AddCommand = new StswAsyncCommand(Add);
        CloneCommand = new StswAsyncCommand(Clone, CloneCondition);
        EditCommand = new StswAsyncCommand(Edit, EditCondition);
        DeleteCommand = new StswAsyncCommand(Delete, DeleteCondition);
        AddPdfCommand = new StswAsyncCommand(AddPdf, AddPdfCondition);
    }

    #region Commands
    /// Clear
    private async Task Clear()
    {
        IsRefreshOpen = false;
        LoadingActions++;
        
        await Task.Run(() => ListContractors = new());

        LoadingActions--;
    }

    /// Refresh
    private async Task Refresh()
    {
        LoadingActions++;

        FiltersContractors.Refresh?.Invoke();
        await Task.Run(() => ListContractors = SQL.GetContractors(FiltersContractors.SqlFilter, FiltersContractors.SqlParameters));
        
        LoadingActions--;
    }

    /// Save
    private async Task Save()
    {
        LoadingActions++;

        if (SQL.SetContractors(ListContractors))
        {
            await Task.Run(() => Refresh());
            MessageBox.Show("Data saved successfully.");
        }

        LoadingActions--;
    }

    /// Export
    private async Task Export()
    {
        LoadingActions++;

        await Task.Run(() => StswExport.ExportToExcel(("Sheet1", ListContractors), null, true));

        LoadingActions--;
    }

    /// Add
    private async Task Add()
    {
        LoadingActions++;

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
        await Task.Run(() =>
        {
        });

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

        LoadingActions--;
    }

    /// Clone
    private async Task Clone()
    {
        LoadingActions++;

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
        await Task.Run(() =>
        {
        });

        LoadingActions--;
    }
    private bool CloneCondition() => SelectedContractor is ContractorModel m && m.ID > 0;

    /// Edit
    private async Task Edit()
    {
        LoadingActions++;

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
        await Task.Run(() =>
        {
        });

        LoadingActions--;
    }
    private bool EditCondition() => SelectedContractor is ContractorModel m && m.ID > 0;

    /// Delete
    private async Task Delete()
    {
        LoadingActions++;

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
        await Task.Run(() =>
        {
        });

        LoadingActions--;
    }
    private bool DeleteCondition() => SelectedContractor is ContractorModel m and not null;

    /// AddPdf
    private async Task AddPdf()
    {
        LoadingActions++;


        if (SelectedContractor is ContractorModel m && m.ID > 0)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "PDF files|*.pdf"
            };
            if (dialog.ShowDialog() == true)
                await Task.Run(() => SQL.AddPdf(m.ID, File.ReadAllBytes(dialog.FileName)));
        }

        LoadingActions--;
    }
    private bool AddPdfCondition() => SelectedContractor is ContractorModel m && m.ID > 0;
    #endregion

    #region Properties
    /// Combo sources
    public static ObservableCollection<StswSelectionItem> ComboSourceContractorTypes => SQL.ListOfContractorTypes();

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

    /// Is drop down open
    private bool isRefreshOpen = false;
    public bool IsRefreshOpen
    {
        get => isRefreshOpen;
        set => SetProperty(ref isRefreshOpen, value);
    }

    /// Loading
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

    /// Selected items
    private object? selectedContractor = new();
    public object? SelectedContractor
    {
        get => selectedContractor;
        set => SetProperty(ref selectedContractor, value);
    }
    #endregion
}
