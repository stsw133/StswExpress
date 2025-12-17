using System;
using System.Collections.Generic;
using System.Linq;

namespace TestApp.Wpf;
public partial class StswSearchBoxContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    [StswCommand] void Clear() => Text = string.Empty;
    [StswCommand] void Randomize() => Text = Guid.NewGuid().ToString();

    [StswObservableProperty] int _criteriaCount;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] List<string> _items = [.. Enumerable.Range(1, 3000).Select(i => "Option " + i)];
    [StswObservableProperty] bool _subControls = false;
    [StswObservableProperty] string _text = string.Empty;
    partial void OnTextChanged(string oldValue, string newValue)
    {
        //CriteriaCount = ???
    }
}
