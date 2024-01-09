using System;
using System.Linq;
using System.Windows.Media;

namespace TestApp;

public class StswColorBoxContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => SelectedColor = default);
    public StswCommand RandomizeCommand => new(() => SelectedColor = Color.FromRgb((byte)new Random().Next(255), (byte)new Random().Next(255), (byte)new Random().Next(255)));

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsAlphaEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsAlphaEnabled)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    /// IsAlphaEnabled
    private bool isAlphaEnabled;
    public bool IsAlphaEnabled
    {
        get => isAlphaEnabled;
        set => SetProperty(ref isAlphaEnabled, value);
    }

    /// IsReadOnly
    private bool isReadOnly;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// SelectedColor
    private Color selectedColor = Color.FromRgb(24, 240, 24);
    public Color SelectedColor
    {
        get => selectedColor;
        set => SetProperty(ref selectedColor, value);
    }

    /// SubControls
    private bool subControls = false;
    public bool SubControls
    {
        get => subControls;
        set => SetProperty(ref subControls, value);
    }
}
