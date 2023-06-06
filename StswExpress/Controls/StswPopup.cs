using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace StswExpress;

public class StswPopup : Popup
{
    public StswPopup()
    {
        AddHandler(PreviewMouseUpEvent, new MouseButtonEventHandler(OnPreviewMouseUp));
    }

    #region Events
    /// OnPreviewMouseUp
    private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (CloseOnClick)
            IsOpen = false;
    }
    #endregion

    #region Main properties
    /// CloseOnClick
    public static readonly DependencyProperty CloseOnClickProperty
        = DependencyProperty.Register(
            nameof(CloseOnClick),
            typeof(bool),
            typeof(StswPopup)
        );
    public bool CloseOnClick
    {
        get => (bool)GetValue(CloseOnClickProperty);
        set => SetValue(CloseOnClickProperty, value);
    }
    #endregion
}
