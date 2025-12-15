using Avalonia.Controls;
using Avalonia.Media;

namespace TestApp.Ava;
public partial class StswIconContext : ControlsContext
{
    public StswIconContext()
    {
        Task.Run(() => Icons = [.. typeof(StswIcons).GetProperties()
                                 .Select(x => new StswComboItem() { Display = x.Name, Value = x.GetValue(x) })
                                 .OrderBy(x => x.Display)]);
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Scale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property!.Name.Equals(nameof(Scale)))?.Value ?? default;
        SelectedIcon = Icons.FirstOrDefault(x => x.Value is Geometry geometry && geometry.Equals(Data));
    }

    [StswCommand] void SetGridLengthAuto() => Scale = GridLength.Auto;
    [StswCommand] void SetGridLengthFill() => Scale = new GridLength(1, GridUnitType.Star);
    
    [StswObservableProperty] Geometry? _data = StswIcons.Abacus;
    partial void OnDataChanged(Geometry? oldValue, Geometry? newValue)
    {
        if (SelectedIcon?.Value is Geometry geometry && geometry.Equals(newValue))
            return;

        SelectedIcon = Icons.FirstOrDefault(x => x.Value is Geometry icon && icon.Equals(newValue));
    }

    [StswObservableProperty] IReadOnlyList<StswComboItem> _icons = [];
    [StswObservableProperty] GridLength _scale;

    [StswObservableProperty] StswComboItem? _selectedIcon;
    partial void OnSelectedIconChanged(StswComboItem? oldValue, StswComboItem? newValue) => Data = newValue?.Value as Geometry;
}
