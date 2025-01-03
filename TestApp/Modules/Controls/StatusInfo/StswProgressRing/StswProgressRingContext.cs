﻿using System.Linq;
using System.Windows;

namespace TestApp;

public class StswProgressRingContext : ControlsContext
{
    public StswCommand SetGridLengthAutoCommand => new(() => Scale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => Scale = new GridLength(1, GridUnitType.Star));

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsIndeterminate = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsIndeterminate)))?.Value ?? default;
        Scale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Scale)))?.Value ?? default;
        State = (StswProgressState?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(State)))?.Value ?? default;
        TextMode = (StswProgressTextMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(TextMode)))?.Value ?? default;
    }

    /// IsIndeterminate
    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        set => SetProperty(ref _isIndeterminate, value);
    }
    private bool _isIndeterminate;

    /// Maximum
    public double? Maximum
    {
        get => _maximum;
        set => SetProperty(ref _maximum, value);
    }
    private double? _maximum = 100;

    /// Minimum
    public double? Minimum
    {
        get => _minimum;
        set => SetProperty(ref _minimum, value);
    }
    private double? _minimum = 0;

    /// Scale
    public GridLength Scale
    {
        get => _scale;
        set => SetProperty(ref _scale, value);
    }
    private GridLength _scale;

    /// SelectedValue
    public double? SelectedValue
    {
        get => _selectedValue;
        set => SetProperty(ref _selectedValue, value);
    }
    private double? _selectedValue = 0;

    /// State
    public StswProgressState State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }
    private StswProgressState _state;

    /// TextMode
    public StswProgressTextMode TextMode
    {
        get => _textMode;
        set => SetProperty(ref _textMode, value);
    }
    private StswProgressTextMode _textMode;
}
