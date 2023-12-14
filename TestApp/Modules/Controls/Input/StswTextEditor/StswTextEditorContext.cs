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

    #region Properties
    /// IsReadOnly
    private bool isReadOnly;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// Text
    private FlowDocument text = new FlowDocument(new Paragraph(new Run("TEST")));
    public FlowDocument Text
    {
        get => text;
        set => SetProperty(ref text, value);
    }
    #endregion
}
