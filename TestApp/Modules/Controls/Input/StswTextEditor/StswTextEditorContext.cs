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

    /// Content
    public FlowDocument Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }
    private FlowDocument _content = new FlowDocument(new Paragraph(new Run("TEST")));

    /// IsReadOnly
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }
    private bool _isReadOnly;
}
