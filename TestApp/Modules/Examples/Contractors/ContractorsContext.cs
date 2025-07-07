using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace TestApp;
public partial class ContractorsContext : StswObservableObject
{
    public ContractorsContext()
    {
        Task.Run(Init);
        SelectedContractor = null;
        ListContractorsView = CollectionViewSource.GetDefaultView(_listContractors);
    }

    [StswCommand] async Task Init()
    {
        try
        {
            SQLService.InitializeContractorsTables();
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }

    [StswCommand] async Task Clear()
    {
        try
        {
            ListContractors.Clear();
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }

    [StswCommand] async Task Refresh()
    {
        try
        {
            // for CollectionView filters:
            ListContractors = [.. await Task.Run(() => SQLService.GetContractors(null))];
            ListContractorsView = CollectionViewSource.GetDefaultView(ListContractors);
            FiltersContractors.Apply?.Invoke();

            // for SQL filters:
            //FiltersContractors.Apply?.Invoke();

            //IEnumerable<ContractorModel> list = [];
            //await Task.Run(() => list = SQL.GetContractors(FiltersContractors));
            //ListContractors = new(list);
            //ListContractorsView?.Refresh();
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }
    
    [StswCommand] async Task Save()
    {
        try
        {
            await Task.Run(() => SQLService.SetContractors(ListContractors));
            RefreshCommand.Execute(null);
            await StswMessageDialog.Show("Data saved successfully.", nameof(TestApp), null, StswDialogButtons.OK, StswDialogImage.Success);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }
    
    [StswCommand] async Task Export()
    {
        try
        {
            //await Task.Run(() => StswExport.ExportToExcel("Sheet1", ListContractors, null, new() { OpenFile = true }));
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }

    [StswCommand] async Task Add()
    {
        try
        {
            await Task.Run(() => App.Current.Dispatcher.Invoke(() =>
            {
                NewTabCommand?.Execute(null);
                // if (NewTab.Content is ContractorsSingleContext context)
                // {
                //     context.ID = 0;
                //     context.IsCloned = false;
                // }
                // if (NewTab.Header is StswLabel header)
                // {
                //     header.Content = "New contractor";
                //     header.IconData = StswIcons.AccountPlus;
                // }
            }));
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }

    [StswCommand(ConditionMethodName = nameof(CloneCondition))] async Task Clone()
    {
        try
        {
            if (SelectedContractor is ContractorModel m && m.Id > 0)
            {
                await Task.Run(() => App.Current.Dispatcher.Invoke(() =>
                {
                    NewTabCommand?.Execute(null);
                    if (NewTab.Content is ContractorsSingleContext context)
                    {
                        context.Id = m.Id;
                        context.IsCloned = true;
                    }
                    if (NewTab.Header is StswLabel header)
                    {
                        header.Content = $"Cloning contractor (ID: {m.Id})";
                        header.IconData = StswIcons.AccountPlus;
                    }
                }));
            }
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }
    private bool CloneCondition() => SelectedContractor is ContractorModel m && m.Id > 0;

    [StswCommand(ConditionMethodName = nameof(EditCondition))] async Task Edit()
    {
        try
        {
            if (SelectedContractor is ContractorModel m && m.Id > 0)
            {
                await Task.Run(() => App.Current.Dispatcher.Invoke(() =>
                {
                    NewTabCommand?.Execute(null);
                    if (NewTab.Content is ContractorsSingleContext context)
                    {
                        context.Id = m.Id;
                        context.IsCloned = false;
                    }
                    if (NewTab.Header is StswLabel header)
                    {
                        header.Content = $"Editing contractor (ID: {m.Id})";
                        header.IconData = StswIcons.AccountEdit;
                    }
                }));
            }
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }
    private bool EditCondition() => SelectedContractor is ContractorModel m && m.Id > 0;

    [StswCommand(ConditionMethodName = nameof(DeleteCondition))] async Task Delete()
    {
        try
        {
            if (SelectedContractor is ContractorModel m)
            {
                await Task.Run(() =>
                {
                    if (m.Id == 0)
                    {
                        ListContractors.Remove(m);
                    }
                    else if (m.Id > 0 && MessageBox.Show("Are you sure you want to delete selected item?", string.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        SQLService.DeleteContractor(m.Id);
                        ListContractors.Remove(m);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, $"Error occured in: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }
    private bool DeleteCondition() => SelectedContractor is ContractorModel;

    [StswObservableProperty] StswDataGridFiltersDataModel _filtersContractors = new();
    [StswObservableProperty] StswObservableCollection<ContractorModel> _listContractors = [];
    [StswObservableProperty] ICollectionView? _listContractorsView;
    [StswObservableProperty] object? _selectedContractor = new();

    public ICommand? NewTabCommand { get; set; }
    public StswTabItem NewTab
    {
        get => _newTab;
        set
        {
            value.Content = new ContractorsSingleContext();
            value.Header = new StswLabel() { Content = "New contractor", IconData = StswIcons.Plus };
            value.IsClosable = true;
            SetProperty(ref _newTab, value);
        }
    }
    private StswTabItem _newTab = new();
}
