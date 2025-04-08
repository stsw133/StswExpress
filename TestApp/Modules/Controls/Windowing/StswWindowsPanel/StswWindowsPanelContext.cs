using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TestApp;

public class StswWindowsPanelContext : ControlsContext
{
    public StswWindowsPanelContext()
    {
        CloseWindowCommand = new(OnCloseWindow);
    }

    /// Items
    public ObservableCollection<object> Contexts
    {
        get => _contexts;
        set => SetProperty(ref _contexts, value);
    }
    private ObservableCollection<object> _contexts = new()
    {
        new StswListBoxContext(), new StswComboBoxContext(), new StswButtonContext(), 
        new StswContentDialogContext()
    };

    public StswCommand<object> CloseWindowCommand { get; set; }

    private void OnCloseWindow(object? obj)
    {
        if (obj is ControlsContext context)
            Contexts.Remove(context);
    }
}
