using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TestApp;
public partial class ContractorsContext : StswObservableObject
{
    /// Init
    [StswCommand] async Task Init()
    {
        try
        {
            SQLService.InitializeContractorsTables();
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occurred in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }

    /// Clear
    [StswCommand] async Task Clear()
    {
        try
        {
            ListContractors.ReplaceWith([]);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occurred in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }

    /// Refresh
    [StswCommand] async Task Refresh()
    {
        try
        {
            // for CollectionView filters:
            ListContractors.ReplaceWith(await Task.Run(() => SQLService.GetContractors(null)));
            //FiltersContractors.Apply?.Invoke();

            // for SQL filters:
            //FiltersContractors.Apply?.Invoke();

            //IEnumerable<ContractorModel> list = [];
            //await Task.Run(() => list = SQL.GetContractors(FiltersContractors));
            //ListContractors = new(list);
            //ListContractorsView?.Refresh();
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occurred in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }

    /// Save
    [StswCommand] async Task Save()
    {
        try
        {
            await Task.Run(() => SQLService.SetContractors(ListContractors.Items));
            RefreshCommand.Execute(null);
            await StswMessageDialog.Show("Data saved successfully.", nameof(TestApp), null, StswDialogButtons.OK, StswDialogImage.Success);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occurred in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }

    /// Export
    [StswCommand] async Task Export()
    {
        try
        {
            //await Task.Run(() => StswExport.ExportToExcel("Sheet1", ListContractors, null, new() { OpenFile = true }));
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occurred in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }

    /// Add
    [StswCommand] async Task Add()
    {
        try
        {
            _pendingTabAction = EditorAction.Add;
            _pendingContractor = null;

            await Task.Run(() => App.Current.Dispatcher.Invoke(() => NewTabCommand?.Execute(null)));
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occurred in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }

    /// Clone
    [StswCommand(nameof(CloneCondition))]
    async Task Clone()
    {
        if (SelectedContractor is not ContractorModel m || m.Id <= 0)
            return;

        try
        {
            _pendingTabAction = EditorAction.Clone;
            _pendingContractor = m;

            await Task.Run(() => App.Current.Dispatcher.Invoke(() => NewTabCommand?.Execute(null)));
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occurred in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }
    private bool CloneCondition() => SelectedContractor is ContractorModel m && m.Id > 0;

    /// Edit
    [StswCommand(nameof(EditCondition))]
    async Task Edit()
    {
        if (SelectedContractor is not ContractorModel m || m.Id <= 0)
            return;

        try
        {
            _pendingTabAction = EditorAction.Edit;
            _pendingContractor = m;

            await Task.Run(() => App.Current.Dispatcher.Invoke(() => NewTabCommand?.Execute(null)));
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occurred in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }
    private bool EditCondition() => SelectedContractor is ContractorModel m && m.Id > 0;

    /// Delete
    [StswCommand(nameof(DeleteCondition))]
    async Task Delete()
    {
        if (SelectedContractor is not ContractorModel m)
            return;

        try
        {
            await Task.Run(() =>
            {
                if (m.Id == 0)
                {
                    ListContractors.Items.Remove(m);
                }
                else if (m.Id > 0 && MessageBox.Show("Are you sure you want to delete selected item?", string.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    SQLService.DeleteContractor(m.Id);
                    ListContractors.Items.Remove(m);
                }
            });
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occurred in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }
    private bool DeleteCondition() => SelectedContractor is ContractorModel;

    /// ConfigureNewTab
    private void ConfigureNewTab(StswTabItem tab)
    {
        var context = tab.Content switch
        {
            ContractorsSingleContext ctx => ctx,
            FrameworkElement { DataContext: ContractorsSingleContext ctx } => ctx,
            _ => new ContractorsSingleContext()
        };

        if (tab.Content is not FrameworkElement)
            tab.Content = context;

        var header = tab.Header as StswLabel ?? new StswLabel();
        tab.Header = header;

        header.Content = "New contractor";
        header.IconData = StswIcons.Plus;

        switch (_pendingTabAction)
        {
            case EditorAction.Clone when _pendingContractor is { Id: > 0 } contractor:
                context.Id = contractor.Id;
                context.IsCloned = true;
                header.Content = $"Cloning contractor (ID: {contractor.Id})";
                header.IconData = StswIcons.AccountPlus;
                break;
            case EditorAction.Edit when _pendingContractor is { Id: > 0 } contractor:
                context.Id = contractor.Id;
                context.IsCloned = false;
                header.Content = $"Editing contractor (ID: {contractor.Id})";
                header.IconData = StswIcons.AccountEdit;
                break;
            default:
                context.Id = 0;
                context.IsCloned = false;
                break;
        }
    }



    [StswObservableProperty] StswDataGridFiltersDataModel _filtersContractors = new();
    [StswObservableProperty] StswCollectionViewWrapper<ContractorModel> _listContractors = new();
    [StswObservableProperty] object? _selectedContractor;

    [StswObservableProperty] StswTabItem? _newTab;
    private EditorAction _pendingTabAction = EditorAction.Add;
    private ContractorModel? _pendingContractor;

    private ICommand? _newTabCreatedCommand;
    public ICommand NewTabCreatedCommand => _newTabCreatedCommand ??= new StswCommand<StswTabItem>(ConfigureNewTab);

    public ICommand? NewTabCommand { get; set; }
}
