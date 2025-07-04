using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TestApp;
public partial class StswRatingControlContext : ControlsContext
{
    public StswCommand SetGridLengthAutoCommand => new(() => IconScale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => IconScale = new GridLength(1, GridUnitType.Star));

    public override void SetDefaults()
    {
        base.SetDefaults();

        Direction = (ExpandDirection?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Direction)))?.Value ?? default;
        IconScale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IconScale)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        IsResetEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsResetEnabled)))?.Value ?? default;
        ItemsNumber = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ItemsNumber)))?.Value ?? default;
        ItemsNumberVisibility = (Visibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ItemsNumberVisibility)))?.Value ?? default;
    }

    [StswObservableProperty] ExpandDirection _direction;
    [StswObservableProperty] GridLength _iconScale;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] bool _isResetEnabled;
    [StswObservableProperty] int _itemsNumber;
    [StswObservableProperty] Visibility _itemsNumberVisibility;
    [StswObservableProperty] double? _selectedValue = 0;
}
