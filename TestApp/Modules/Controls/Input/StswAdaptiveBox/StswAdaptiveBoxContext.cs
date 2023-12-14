using System.Linq;

namespace TestApp;

public class StswAdaptiveBoxContext : ControlsContext
{
    public StswCommand ClearTypeCommand => new(() => Type = null);

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        Type = (StswAdaptiveType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Type)))?.Value ?? default;
    }

    #region Properties
    /// IsReadOnly
    private bool isReadOnly;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// SelectedValue
    private object? selectedValue;
    public object? SelectedValue
    {
        get => selectedValue;
        set => SetProperty(ref selectedValue, value);
    }

    /// Type
    private StswAdaptiveType? type;
    public StswAdaptiveType? Type
    {
        get => type;
        set => SetProperty(ref type, value);
    }
    #endregion
}
