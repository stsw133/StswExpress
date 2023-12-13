using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TestApp;

public class StswLogPanelContext : ControlsContext
{
    public StswCommand AddRandomItemCommand { get; set; }
    public StswAsyncCommand LoadFromFilesCommand { get; set; }

    public StswLogPanelContext()
    {
        AddRandomItemCommand = new(AddRandomItem);
        LoadFromFilesCommand = new(LoadFromFiles_Executed);
    }

    #region Events and methods
    /// Command: add random item
    private void AddRandomItem()
    {
        ItemsSource.Add(new(
            StswFn.GetNextEnumValue(StswLogType.None, new Random().Next(Enum.GetValues(typeof(StswLogType)).Length)),
            "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta."[..new Random().Next(10, 199)])
        );
    }

    /// Command: load from files
    private Task LoadFromFiles_Executed() => Task.Run(() => ItemsSource = StswLog.Import(DateTime.Now.AddDays(-14), DateTime.Now));
    #endregion

    #region Properties
    /// IsClosable
    private bool isClosable;
    public bool IsClosable
    {
        get => isClosable;
        set => SetProperty(ref isClosable, value);
    }

    /// ItemsSource
    private ObservableCollection<StswLogItem> itemsSource = new();
    public ObservableCollection<StswLogItem> ItemsSource
    {
        get => itemsSource;
        set => SetProperty(ref itemsSource, value);
    }
    #endregion
}
