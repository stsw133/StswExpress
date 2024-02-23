using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace TestApp;

public class StswRadioBoxContext : ControlsContext
{
    public StswCommand<string?> OnClickCommand => new((x) => ClickOption = Convert.ToInt32(x));

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        IsThreeState = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsThreeState)))?.Value ?? default;
    }

    /// ClickOption
    private int clickOption;
    public int ClickOption
    {
        get => clickOption;
        set => SetProperty(ref clickOption, value);
    }
    /// SelectedOption
    private ObservableCollection<bool?> selectedOption = new() { null, false, false, true, false };
    public ObservableCollection<bool?> SelectedOption
    {
        get => selectedOption;
        set => SetProperty(ref selectedOption, value);
    }

    /// HasContent
    private bool hasContent = true;
    public bool HasContent
    {
        get => hasContent;
        set => SetProperty(ref hasContent, value);
    }

    /// IsReadOnly
    private bool isReadOnly;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// IsThreeState
    private bool isThreeState;
    public bool IsThreeState
    {
        get => isThreeState;
        set => SetProperty(ref isThreeState, value);
    }
}
