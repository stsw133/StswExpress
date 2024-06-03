using System.Linq;
using System.Windows.Documents;

namespace TestApp;

public class StswTextEditorContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    /// IsReadOnly
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }
    private bool _isReadOnly;

    /// Text
    public FlowDocument Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }
    private FlowDocument _text = new FlowDocument(new Paragraph(new Run("TEST")));
}
