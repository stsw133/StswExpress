using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp;

public class StswSpinnerContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
    }

    /// ClickCounter
    private SpinnerType _spinnerType;
    public SpinnerType SpinnerType
    {
        get => _spinnerType;
        set => SetProperty(ref _spinnerType, value);
    }
}
