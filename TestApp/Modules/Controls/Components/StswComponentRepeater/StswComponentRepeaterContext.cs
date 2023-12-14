﻿using System.Linq;
using System.Windows;

namespace TestApp;

public class StswComponentRepeaterContext : ControlsContext
{
    public StswCommand OnClickCommand => new(() => ClickCounter++);
    public StswCommand SetGridLengthAutoCommand => new(() => IconScale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => IconScale = new GridLength(1, GridUnitType.Star));

    public override void SetDefaults()
    {
        base.SetDefaults();

        ContentVisibility = (Visibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ContentVisibility)))?.Value ?? default;
        IconScale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IconScale)))?.Value ?? default;
        IsBusy = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsBusy)))?.Value ?? default;
    }

    #region Properties
    /// ClickCounter
    private int clickCounter;
    public int ClickCounter
    {
        get => clickCounter;
        set => SetProperty(ref clickCounter, value);
    }

    /// ContentVisibility
    private Visibility contentVisibility;
    public Visibility ContentVisibility
    {
        get => contentVisibility;
        set => SetProperty(ref contentVisibility, value);
    }

    /// IconScale
    private GridLength iconScale;
    public GridLength IconScale
    {
        get => iconScale;
        set => SetProperty(ref iconScale, value);
    }

    /// IsBusy
    private bool isBusy;
    public bool IsBusy
    {
        get => isBusy;
        set => SetProperty(ref isBusy, value);
    }
    #endregion
}
