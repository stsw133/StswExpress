using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace TestApp;

public class StswInfoPanelContext : ControlsContext
{
    public StswCommand AddRandomItemCommand => new(AddRandomItem);
    public StswAsyncCommand LoadFromFilesCommand => new(LoadFromFiles);

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsClosable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsClosable)))?.Value ?? default;
    }

    #region Events & methods
    /// Command: add random item
    private void AddRandomItem()
    {
        ItemsSource.Add(new(
            StswInfoType.None.GetNextValue(new Random().Next(Enum.GetValues(typeof(StswInfoType)).Length)),
            "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta."[..new Random().Next(10, 199)])
        );
    }

    /// Command: load from files
    private Task LoadFromFiles() => Task.Run(() => ItemsSource = StswLog.Import(DateTime.Now.AddDays(-14), DateTime.Now));
    #endregion

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
}
