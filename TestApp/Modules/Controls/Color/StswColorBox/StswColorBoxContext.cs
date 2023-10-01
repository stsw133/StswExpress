using System;
using System.Windows.Input;
using System.Windows.Media;

namespace TestApp;

public class StswColorBoxContext : ControlsContext
{
    public ICommand ClearCommand { get; set; }
    public ICommand RandomizeCommand { get; set; }

    public StswColorBoxContext()
    {
        ClearCommand = new StswCommand(Clear);
        RandomizeCommand = new StswCommand(Randomize);
    }

    #region Events and methods
    /// Command: clear
    private void Clear() => SelectedColor = default;

    /// Command: randomize
    private void Randomize() => SelectedColor = Color.FromRgb((byte)new Random().Next(255), (byte)new Random().Next(255), (byte)new Random().Next(255));
    #endregion

    #region Properties
    /// Components
    private bool components = false;
    public bool Components
    {
        get => components;
        set => SetProperty(ref components, value);
    }

    /// IsAlphaEnabled
    private bool isAlphaEnabled = true;
    public bool IsAlphaEnabled
    {
        get => isAlphaEnabled;
        set => SetProperty(ref isAlphaEnabled, value);
    }

    /// IsReadOnly
    private bool isReadOnly = false;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// SelectedColor
    private Color selectedColor = Color.FromRgb(24, 240, 24);
    public Color SelectedColor
    {
        get => selectedColor;
        set => SetProperty(ref selectedColor, value);
    }
    #endregion
}
