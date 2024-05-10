using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace TestApp;

public class StswToggleSwitchContext : ControlsContext
{
    public StswCommand<string?> OnClickCommand => new((x) => ClickOption = Convert.ToInt32(x));

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        IsThreeState = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsThreeState)))?.Value ?? default;
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
    private ObservableCollection<bool?> _selectedOption = new() { null, false, false, true, false };

    /// HasContent
    public bool HasContent
    {
        get => _hasContent;
        set => SetProperty(ref _hasContent, value);
    }
    private bool _hasContent = true;

    /// IsReadOnly
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }
    private bool _isReadOnly;

    /// IsThreeState
    public bool IsThreeState
    {
        get => _isThreeState;
        set => SetProperty(ref _isThreeState, value);
    }
    private bool _isThreeState;
}
