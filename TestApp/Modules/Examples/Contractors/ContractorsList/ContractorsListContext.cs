using System;
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

    public ContractorsListContext()
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

    #region Commands & methods
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
        IsRefreshOpen = false;
        LoadingActions++;
        
        await Task.Run(() => ListContractors = []);

        LoadingActions--;
    }

    /// Refresh
    private async Task Refresh()
    {
        LoadingActions++;

        FiltersContractors.Refresh?.Invoke();
        await Task.Run(() => ListContractors = SQL.GetContractors(FiltersContractors.SqlFilter!, FiltersContractors.SqlParameters!).ToStswBindingList());
        
        LoadingActions--;
    }

    /// Save
    private async Task Save()
    {
        LoadingActions++;

        try
        {
            await Task.Run(() => SQL.SetContractors(ListContractors));
            RefreshCommand.Execute(null);
            MessageBox.Show("Data saved successfully.");
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, "Error");
        }

        LoadingActions--;
    }

    /// Export
    private async Task Export()
    {
        LoadingActions++;

        await Task.Run(() => StswExcelFn.ExportTo("Sheet1", ListContractors, null, true));

        LoadingActions--;
    }

    /// Add
    private async Task Add()
    {
        LoadingActions++;

        if (ContractorsContext.Tabs_.FirstOrDefault() is StswTabItem tabItem)
        {
            if (StswFn.FindVisualAncestor<StswTabControl>(tabItem) is StswTabControl tabControl)
            {
                tabControl.PART_NewTabButton_Click(this, new RoutedEventArgs());
                //if (ContractorsContext.Tabs_[tabControl.SelectedIndex]?.DataContext is ContractorsSingleContext newContext)
                //{
                //    newContext.ID = 0;
                //    newContext.IsCloned = false;
                //
                //    if (ContractorsContext.Tabs_[tabControl.SelectedIndex]?.Header is StswHeader header)
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
            if (ContractorsContext.Tabs_.FirstOrDefault() is StswTabItem tabItem)
            {
                if (StswFn.FindVisualAncestor<StswTabControl>(tabItem) is StswTabControl tabControl)
                {
                    tabControl.PART_NewTabButton_Click(this, new RoutedEventArgs());
                    if (ContractorsContext.Tabs_[tabControl.SelectedIndex]?.Content is ContractorsSingleContext newContext)
                    {
                        newContext.ID = m.ID;
                        newContext.IsCloned = true;

                        if (ContractorsContext.Tabs_[tabControl.SelectedIndex]?.Header is StswHeader header)
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
            if (ContractorsContext.Tabs_.FirstOrDefault() is StswTabItem tabItem)
            {
                if (StswFn.FindVisualAncestor<StswTabControl>(tabItem) is StswTabControl tabControl)
                {
                    tabControl.PART_NewTabButton_Click(this, new RoutedEventArgs());

                    if (ContractorsContext.Tabs_[tabControl.SelectedIndex]?.Content is ContractorsSingleContext newContext)
                    {
                        newContext.ID = m.ID;
                        newContext.IsCloned = false;

                        if (ContractorsContext.Tabs_[tabControl.SelectedIndex]?.Header is StswHeader header)
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

        try
        {
            if (SelectedContractor is ContractorModel m)
            {
                await Task.Run(() =>
                {
                    if (m.ID == 0)
                        ListContractors.Remove(m);
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

        LoadingActions--;
    }
    private bool DeleteCondition() => SelectedContractor is ContractorModel;

    #endregion

    /// Data collections
    public StswBindingList<ContractorModel> ListContractors
    {
        get => _listContractors;
        set => SetProperty(ref _listContractors, value);
    }
    private StswBindingList<ContractorModel> _listContractors = [];

    /// Filters data
    public StswDataGridFiltersDataModel FiltersContractors
    {
        get => _filtersContractors;
        set => SetProperty(ref _filtersContractors, value);
    }
    private StswDataGridFiltersDataModel _filtersContractors = new();

    /// Is drop down open
    public bool IsRefreshOpen
    {
        get => _isRefreshOpen;
        set => SetProperty(ref _isRefreshOpen, value);
    }
    private bool _isRefreshOpen = false;

    /// Loading
    public int LoadingActions
    {
        get => _loadingActions;
        set
        {
            SetProperty(ref _loadingActions, value);

            if (!LoadingState.In(StswProgressState.Paused, StswProgressState.Error))
                LoadingState = LoadingActions > 0 ? StswProgressState.Running : StswProgressState.Ready;
        }
    }
    private int _loadingActions = 0;

    public StswProgressState LoadingState
    {
        get => _loadingState;
        set => SetProperty(ref _loadingState, value);
    }
    private StswProgressState _loadingState;

    /// Selected items
    public object? SelectedContractor
    {
        get => _selectedContractor;
        set => SetProperty(ref _selectedContractor, value);
    }
    private object? _selectedContractor = new();
}
