using System.Windows.Controls;

namespace TestApp;

public class StswRatingControlContext : ControlsContext
{
    #region Properties
    /// Direction
    private ExpandDirection direction = ExpandDirection.Right;
    public ExpandDirection Direction
    {
        get => direction;
        set => SetProperty(ref direction, value);
    }

    /// ItemsNumber
    private int itemsNumber = 5;
    public int ItemsNumber
    {
        get => itemsNumber;
        set => SetProperty(ref itemsNumber, value);
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
