using System;namespace StswExpress;
/// <summary>
/// Represents a control functioning as header for multiple other controls like buttons, expanders and more.
/// </summary>
[Obsolete($"This control will propably be deleted soon. Use {nameof(StswLabel)} instead!")]
public class StswHeader : StswLabel
{
    static StswHeader()
    {
        //DefaultStyleKeyProperty.OverrideMetadata(typeof(StswHeader), new FrameworkPropertyMetadata(typeof(StswHeader)));
    }
}
