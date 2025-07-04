using System;
using System.Linq;
using System.Windows;

namespace TestApp;
public partial class StswSubDropContext : ControlsContext
{
    public StswCommand<string?> OnClickCommand => new((x) => { ClickOption = Convert.ToInt32(x); IsDropDownOpen = false; });
    public StswCommand SetGridLengthAutoCommand => new(() => IconScale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => IconScale = new GridLength(1, GridUnitType.Star));

    public override void SetDefaults()
    {
        base.SetDefaults();

        IconScale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IconScale)))?.Value ?? default;
        IsBusy = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsBusy)))?.Value ?? default;
        IsContentVisible = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsContentVisible)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    [StswObservableProperty] int _clickOption;
    [StswObservableProperty] GridLength _iconScale;
    [StswObservableProperty] bool _isBusy;
    [StswObservableProperty] bool _isContentVisible;
    [StswObservableProperty] bool _isDropDownOpen = false;
    [StswObservableProperty] bool _isReadOnly;
}
