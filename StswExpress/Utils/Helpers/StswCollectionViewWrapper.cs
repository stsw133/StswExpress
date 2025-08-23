using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Wrapper for a collection of items that implements <see cref="IStswCollectionItem"/> and provides a <see cref="CollectionViewSource"/> for data binding.
/// </summary>
/// <typeparam name="T">Type of the items in the collection, which must implement <see cref="IStswCollectionItem"/>.</typeparam>
public class StswCollectionViewWrapper<T> : StswObservableObject where T : IStswCollectionItem
{
    public StswObservableCollection<T> Items { get; }
    public CollectionViewSource Source { get; }
    public ICollectionView View => Source.View;

    public StswCollectionViewWrapper() : this([])
    { }

    public StswCollectionViewWrapper(IEnumerable<T> items)
    {
        Items = new(items ?? throw new ArgumentNullException(nameof(items)));
        Source = new CollectionViewSource { Source = Items };
    }

    /// <summary>
    /// Replaces the current items in the collection with the specified items.
    /// </summary>
    /// <param name="items">The new items to replace the current items.</param>
    public void ReplaceWith(IEnumerable<T> items)
    {
        Items.Clear();
        Items.AddRangeFast(items);
    }
}
