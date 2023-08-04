using System.Windows.Controls;

namespace TestApp;

public class StswRatingControlContext : ControlsContext
{
    #region Properties
    /// ItemsNumber
    private int itemsNumber = 5;
    public int ItemsNumber
    {
        get => itemsNumber;
        set => SetProperty(ref itemsNumber, value);
    }

    /// Orientation
    private Orientation orientation;
    public Orientation Orientation
    {
        get => orientation;
        set => SetProperty(ref orientation, value);
    }

    /// SelectedValue
    private double? selectedValue = 0;
    public double? SelectedValue
    {
        get => selectedValue;
        set => SetProperty(ref selectedValue, value);
    }
    #endregion
}
