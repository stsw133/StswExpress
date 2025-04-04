using System;
using System.Collections.Generic;
using System.Linq;

namespace TestApp;

public class StswFilterTagsContext : ControlsContext
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

    /// AllowCustomTags
    public bool AllowCustomTags
    {
        get => _allowCustomTags;
        set => SetProperty(ref _allowCustomTags, value);
    }
    private bool _allowCustomTags;
    
    /// ItemsSource
    public List<string> ItemsSource
    {
        get => _itemsSource;
        set => SetProperty(ref _itemsSource, value);
    }
    private List<string> _itemsSource = new([.. Enumerable.Range(1, 15).Select(i => new string("Tag" + i))]);

    /// SelectedTags
    public string SelectedTags
    {
        get => _selectedTags;
        set => SetProperty(ref _selectedTags, value);
    }
    private string _selectedTags = string.Empty;
}
