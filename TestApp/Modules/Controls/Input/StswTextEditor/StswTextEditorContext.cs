using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace TestApp;

public class StswTextEditorContext : StswObservableObject
{
    private FlowDocument text = new FlowDocument(new Paragraph(new Run("TEST")));
    public FlowDocument Text
    {
        get => text;
        set => SetProperty(ref text, value);
    }

    /// ...
    public ICommand ClearCommand { get; set; }
    public ICommand SearchCommand { get; set; }

    public StswTextEditorContext()
    {
        ClearCommand = new StswRelayCommand(Clear);
        SearchCommand = new StswRelayCommand(Search);
    }

    /// ClearCommand
    private void Clear()
    {
        Text = new();
    }
    /// SearchCommand
    private void Search()
    {
        MessageBox.Show(new TextRange(Text.ContentStart, Text.ContentEnd).Text);
    }
}
