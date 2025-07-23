using System.Windows.Media;

namespace TestApp;
public partial class StswColorSelectorContext : ControlsContext
{
    [StswObservableProperty] Color _selectedColor = Color.FromRgb(24, 240, 24);
}
