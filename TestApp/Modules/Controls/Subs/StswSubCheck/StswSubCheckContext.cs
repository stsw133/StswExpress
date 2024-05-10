﻿using System.Linq;
using System.Windows;

namespace TestApp;

public class StswSubCheckContext : ControlsContext
{
    public StswCommand SetGridLengthAutoCommand => new(() => IconScale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => IconScale = new GridLength(1, GridUnitType.Star));

    public override void SetDefaults()
    {
        base.SetDefaults();

        IconScale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IconScale)))?.Value ?? default;
        IsBusy = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsBusy)))?.Value ?? default;
        IsContentVisible = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsContentVisible)))?.Value ?? default;
        IsThreeState = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsThreeState)))?.Value ?? default;
    }

    /// IconScale
    public GridLength IconScale
    {
        get => _iconScale;
        set => SetProperty(ref _iconScale, value);
    }
    private GridLength _iconScale;

    /// IsBusy
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }
    private bool _isBusy;

    /// IsContentVisible
    public bool IsContentVisible
    {
        get => _isContentVisible;
        set => SetProperty(ref _isContentVisible, value);
    }
    private bool _isContentVisible;

    /// IsThreeState
    public bool IsThreeState
    {
        get => _isThreeState;
        set => SetProperty(ref _isThreeState, value);
    }
    private bool _isThreeState;
}
