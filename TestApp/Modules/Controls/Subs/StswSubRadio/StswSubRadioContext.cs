using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace TestApp;

public class StswSubRadioContext : ControlsContext
{
    public StswCommand<string?> OnClickCommand => new((x) => ClickOption = Convert.ToInt32(x));
    public StswCommand SetGridLengthAutoCommand => new(() => IconScale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => IconScale = new GridLength(1, GridUnitType.Star));

    public override void SetDefaults()
    {
        base.SetDefaults();

        IconScale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IconScale)))?.Value ?? default;
        IsBusy = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsBusy)))?.Value ?? default;
        IsContentVisible = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsContentVisible)))?.Value ?? default;
    }

    /// ClickOption
    public int ClickOption
    {
        get => _clickOption;
        set => SetProperty(ref _clickOption, value);
    }
    private int _clickOption;

    /// SelectedOption
    public ObservableCollection<bool?> SelectedOption
    {
        get => _selectedOption;
        set => SetProperty(ref _selectedOption, value);
    }
    private ObservableCollection<bool?> _selectedOption = [null, false, false, true, false];

    /// IconScale
    public GridLength IconScale
    {
        get => _iconScale;
        set => SetProperty(ref _iconScale, value);
    }
    private GridLength _iconScale;

    /// IsBusy
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }
    private bool _isBusy;

    /// IsContentVisible
    public bool IsContentVisible
    {
        get => _isContentVisible;
        set => SetProperty(ref _isContentVisible, value);
    }
    private bool _isContentVisible;
}
