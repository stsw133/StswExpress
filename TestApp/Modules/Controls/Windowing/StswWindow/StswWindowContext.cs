using System.Windows;
using System.Windows.Data;

namespace TestApp;

public class StswWindowContext : ControlsContext
{
    public StswCommand OpenNewWindowCommand => new(OpenNewWindow);

    #region Events & methods
    /// Command: open new window
    private void OpenNewWindow()
    {
        var window = new StswWindow()
        {
            Title = "New window",
            Height = 450,
            Width = 750
        };
        window.SetBinding(UIElement.IsEnabledProperty, new Binding()
        {
            Path = new PropertyPath(nameof(IsEnabled)),
            Source = this
        });
        window.Show();
    }
    #endregion
}
