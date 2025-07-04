using System;
using System.Linq;
using System.Windows.Media;

namespace TestApp;
public partial class StswColorBoxContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        IsAlphaEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsAlphaEnabled)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    [StswCommand] void Clear() => SelectedColor = default;
    [StswCommand] void Randomize() => SelectedColor = Color.FromRgb((byte)new Random().Next(255), (byte)new Random().Next(255), (byte)new Random().Next(255));

    [StswObservableProperty] bool _icon;
    [StswObservableProperty] bool _isAlphaEnabled;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] Color _selectedColor = Color.FromRgb(24, 240, 24);
    [StswObservableProperty] bool _subControls = false;
}
