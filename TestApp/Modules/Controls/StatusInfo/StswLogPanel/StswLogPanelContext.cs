using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TestApp;

public class StswLogPanelContext : ControlsContext
{
    public ICommand AddRandomItemCommand { get; set; }

    public StswLogPanelContext()
    {
        AddRandomItemCommand = new StswRelayCommand(AddRandomItem);
    }

    #region Events
    /// Command: add random item
    private void AddRandomItem()
    {
        Items.Add(new(
            StswFn.GetNextEnumValue(StswLogType.None, new Random().Next(Enum.GetValues(typeof(StswLogType)).Length)),
            "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta."[..new Random().Next(199)])
        );
    }
    #endregion

    #region Properties
    /// Items
    private ObservableCollection<StswLogItem> items = new();
    public ObservableCollection<StswLogItem> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }
    #endregion
}
