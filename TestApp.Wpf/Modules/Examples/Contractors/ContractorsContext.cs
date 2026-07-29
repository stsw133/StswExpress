using System.Threading.Tasks;
using System.Windows;

namespace TestApp.Wpf;
public partial class ContractorsContext : StswObservableObject
{
	[StswObservableProperty] StswDataGridFiltersDataModel _filtersContractors = new();
	[StswObservableProperty] StswCollectionViewWrapper<ContractorModel> _listContractors = new();
	[StswObservableProperty] object? _selectedContractor;

	[StswCommand(TryCatch = StswLogTarget.MessageDialog)]
    async Task Init()
    {
        SQLService.InitializeContractorsTables();
    }

    [StswCommand(TryCatch = StswLogTarget.MessageDialog)]
    async Task Clear()
    {
        ListContractors.ReplaceWith([]);
    }

    [StswCommand(TryCatch = StswLogTarget.MessageDialog)]
    async Task Refresh()
    {
        ListContractors.ReplaceWith(await Task.Run(() => SQLService.GetContractors(null)));
        FiltersContractors.Apply?.Invoke(); // this is necessary to re-apply filters after refreshing the collection, otherwise collection is unfiltered at start
    }

    [StswCommand(TryCatch = StswLogTarget.MessageDialog)]
    async Task Save()
    {
        await Task.Run(() => SQLService.SetContractors(ListContractors.Items));
        RefreshCommand.Execute(null);
        await StswMessageDialog.Show("Data saved successfully.", nameof(TestApp.Wpf), null, StswDialogButtons.OK, StswDialogImage.Success);
    }

    [StswCommand(TryCatch = StswLogTarget.MessageDialog)]
    async Task Export()
    {
        //await Task.Run(() => StswExport.ExportToExcel("Sheet1", ListContractors, null, new() { OpenFile = true }));
    }

    [StswCommand(TryCatch = StswLogTarget.MessageDialog)]
    async Task Add()
    {
        _pendingTabAction = EditorAction.Add;
        _pendingContractor = null;
        await App.Current.Dispatcher.InvokeAsync(CreateAndConfigureTab);
    }

    /// Clone
    [StswCommand(nameof(CloneCondition), TryCatch = StswLogTarget.MessageDialog)]
    async Task Clone()
    {
        if (SelectedContractor is not ContractorModel m || m.Id <= 0)
            return;

        _pendingTabAction = EditorAction.Clone;
        _pendingContractor = m;
        await App.Current.Dispatcher.InvokeAsync(CreateAndConfigureTab);
    }
    private bool CloneCondition() => SelectedContractor is ContractorModel m && m.Id > 0;

    /// Edit
    [StswCommand(nameof(EditCondition), TryCatch = StswLogTarget.MessageDialog)]
    async Task Edit()
    {
        if (SelectedContractor is not ContractorModel m || m.Id <= 0)
            return;

        _pendingTabAction = EditorAction.Edit;
        _pendingContractor = m;
        await App.Current.Dispatcher.InvokeAsync(CreateAndConfigureTab);
    }
    private bool EditCondition() => SelectedContractor is ContractorModel m && m.Id > 0;

    /// Delete
    [StswCommand(nameof(DeleteCondition), TryCatch = StswLogTarget.MessageDialog)]
    async Task Delete()
    {
        if (SelectedContractor is not ContractorModel m)
            return;

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
    private bool DeleteCondition() => SelectedContractor is ContractorModel;

    private void CreateAndConfigureTab()
    {
        var tab = StswTabControl.Add("ContractorsView");

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
    private EditorAction _pendingTabAction = EditorAction.Add;
    private ContractorModel? _pendingContractor;
}
