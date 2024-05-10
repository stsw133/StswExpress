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
    public bool IsAlphaEnabled
    {
        get => _isAlphaEnabled;
        set => SetProperty(ref _isAlphaEnabled, value);
    }
    private bool _isAlphaEnabled;

    /// IsReadOnly
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }
    private bool _isReadOnly;

    /// SelectedColor
    public Color SelectedColor
    {
        get => _selectedColor;
        set => SetProperty(ref _selectedColor, value);
    }
    private Color _selectedColor = Color.FromRgb(24, 240, 24);

    /// SubControls
    public bool SubControls
    {
        get => _subControls;
        set => SetProperty(ref _subControls, value);
    }
    private bool _subControls = false;
}
