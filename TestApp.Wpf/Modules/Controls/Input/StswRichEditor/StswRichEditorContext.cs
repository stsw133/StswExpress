using System.Linq;
using System.Windows.Documents;

namespace TestApp.Wpf;
public partial class StswRichEditorContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    [StswObservableProperty] FlowDocument _content = new(new Paragraph(new Run("TEST")));
    [StswObservableProperty] bool _isReadOnly;
}
