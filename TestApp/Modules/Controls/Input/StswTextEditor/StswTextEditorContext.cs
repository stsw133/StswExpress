using System.Windows.Documents;

namespace TestApp;

public class StswTextEditorContext : StswObservableObject
{
    #region Properties
    /// Text
    private FlowDocument text = new FlowDocument(new Paragraph(new Run("TEST")));
    public FlowDocument Text
    {
        get => text;
        set => SetProperty(ref text, value);
    }
    #endregion
}
