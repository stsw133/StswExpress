using System;
using System.Linq;

namespace TestApp;

public class StswSplitButtonContext : ControlsContext
{
    public StswCommand<string?> OnClickCommand => new((x) => { ClickOption = Convert.ToInt32(x); IsDropDownOpen = false; });

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    /// ClickOption
    private int clickOption;
    public int ClickOption
    {
        get => clickOption;
        set => SetProperty(ref clickOption, value);
    }

    /// IsDropDownOpen
    private bool isDropDownOpen = false;
    public bool IsDropDownOpen
    {
        get => isDropDownOpen;
        set => SetProperty(ref isDropDownOpen, value);
    }

    /// IsReadOnly
    private bool isReadOnly;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }
}
