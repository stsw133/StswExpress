using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TestApp;

public class StswRatingControlContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Direction = (ExpandDirection?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Direction)))?.Value ?? default;
        IsResetEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsResetEnabled)))?.Value ?? default;
        ItemsNumber = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ItemsNumber)))?.Value ?? default;
        ItemsNumberVisibility = (Visibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ItemsNumberVisibility)))?.Value ?? default;
    }

    /// Direction
    public ExpandDirection Direction
    {
        get => _direction;
        set => SetProperty(ref _direction, value);
    }
    private ExpandDirection _direction;

    /// ItemsNumber
    public int ItemsNumber
    {
        get => _itemsNumber;
        set => SetProperty(ref _itemsNumber, value);
    }
    private int _itemsNumber;

    /// IsResetEnabled
    public bool IsResetEnabled
    {
        get => _isResetEnabled;
        set => SetProperty(ref _isResetEnabled, value);
    }
    private bool _isResetEnabled;

    /// ItemsNumberVisibility
    public Visibility ItemsNumberVisibility
    {
        get => _itemsNumberVisibility;
        set => SetProperty(ref _itemsNumberVisibility, value);
    }
    private Visibility _itemsNumberVisibility;

    /// SelectedValue
    public double? SelectedValue
    {
        get => _selectedValue;
        set => SetProperty(ref _selectedValue, value);
    }
    private double? _selectedValue = 0;
}
