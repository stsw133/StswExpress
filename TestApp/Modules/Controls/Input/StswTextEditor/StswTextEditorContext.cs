using System.Windows.Documents;

namespace TestApp;

public class StswTextEditorContext : ControlsContext
{
    #region Properties
    /// IsReadOnly
    private bool isReadOnly = false;
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
