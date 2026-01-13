using System.Linq;

namespace TestApp.Wpf;
public partial class StswHighlightTextContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        HighlightText = (string?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(HighlightText)))?.Value ?? default;
    }

    [StswObservableProperty] string? _highlightText;
}
