using System;
using System.Linq;
using System.Windows.Media;

namespace TestApp;
public partial class StswColorBoxContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => SelectedColor = default);
    public StswCommand RandomizeCommand => new(() => SelectedColor = Color.FromRgb((byte)new Random().Next(255), (byte)new Random().Next(255), (byte)new Random().Next(255)));

    public override void SetDefaults()
    {
        base.SetDefaults();
        
        IsAlphaEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsAlphaEnabled)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    [StswObservableProperty] bool _icon;
    [StswObservableProperty] bool _isAlphaEnabled;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] Color _selectedColor = Color.FromRgb(24, 240, 24);
    [StswObservableProperty] bool _subControls = false;
}
