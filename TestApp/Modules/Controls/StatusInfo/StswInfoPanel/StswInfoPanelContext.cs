using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace TestApp;
public partial class StswInfoPanelContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsClosable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsClosable)))?.Value ?? default;
        IsCopyable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsCopyable)))?.Value ?? default;
        IsExpandable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsExpandable)))?.Value ?? default;
        ShowControlPanel = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ShowControlPanel)))?.Value ?? default;
    }

    [StswCommand] void AddRandomItem()
    {
        ItemsSource.Add(new(
            StswInfoType.None.GetNextValue(new Random().Next(Enum.GetValues(typeof(StswInfoType)).Length)),
            "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta."[..new Random().Next(10, 199)])
        );
    }
    [StswCommand] async Task LoadFromFiles() => await Task.Run(async () => ItemsSource = [.. await StswLog.ImportListAsync(DateTime.Now.AddYears(-1), DateTime.Now)]);

    [StswObservableProperty] bool _isClosable;
    [StswObservableProperty] bool _isCopyable;
    [StswObservableProperty] bool _isExpandable;
    [StswObservableProperty] ObservableCollection<StswLogItem> _itemsSource = [];
    [StswObservableProperty] bool _showControlPanel;
}
