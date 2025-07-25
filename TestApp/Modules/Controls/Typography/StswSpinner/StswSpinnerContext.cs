﻿using System.Linq;
using System.Windows;

namespace TestApp;
public partial class StswSpinnerContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Scale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Scale)))?.Value ?? default;
        Type = (StswSpinnerType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Type)))?.Value ?? default;
    }

    [StswCommand] void SetGridLengthAuto() => Scale = GridLength.Auto;
    [StswCommand] void SetGridLengthFill() => Scale = new GridLength(1, GridUnitType.Star);

    [StswObservableProperty] GridLength _scale;
    [StswObservableProperty] StswSpinnerType _type;
}
