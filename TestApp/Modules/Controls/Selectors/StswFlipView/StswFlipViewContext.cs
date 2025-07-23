using System;
using System.Collections.Generic;
using System.Linq;

namespace TestApp;
public partial class StswFlipViewContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        SelectedItem = Items[new Random().Next(30)];
        IsLoopingEnabled = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsLoopingEnabled)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    [StswObservableProperty] bool _isLoopingEnabled;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] List<string> _items = [.. Enumerable.Range(1, 30).Select(i => "Option " + i)];
    [StswObservableProperty] string? _selectedItem;
}
