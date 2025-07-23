using System;
using System.Collections.Generic;
using System.Linq;

namespace TestApp;
public partial class StswFilterTagsContext : ControlsContext
{
    public StswFilterTagsContext()
    {
        SelectedTags = ItemsSource.Where(x => new Random().Next(0, 2) == 1).Aggregate("", (current, tag) => current + tag + " ");
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        AllowCustomTags = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(AllowCustomTags)))?.Value ?? default;
    }

    [StswObservableProperty] bool _allowCustomTags;
    [StswObservableProperty] List<string> _itemsSource = new([.. Enumerable.Range(1, 15).Select(i => new string("Tag" + i))]);
    [StswObservableProperty] string _selectedTags = string.Empty;
}
