using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// Wrapper for a collection of items that implements <see cref="IStswCollectionItem"/> and provides a <see cref="CollectionViewSource"/> for data binding.
/// </summary>
/// <typeparam name="T">Type of the items in the collection, which must implement <see cref="IStswCollectionItem"/>.</typeparam>
[StswInfo("0.20.0", "0.20.1")]
public class StswCollectionViewWrapper<T> : StswObservableObject where T : IStswCollectionItem
{
    public StswObservableCollection<T> Items { get; }
    public CollectionViewSource Source { get; }
    public ICollectionView View => Source.View;

    /// <summary>
    /// Initializes a new instance of the <see cref="StswCollectionViewWrapper{T}"/> class with an empty collection.
    /// </summary>
    public StswCollectionViewWrapper() : this([])
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="StswCollectionViewWrapper{T}"/> class with the specified items.
    /// </summary>
    /// <param name="items">The initial items to populate the collection.</param>
    /// <param name="trackItems">If set to <see langword="true"/>, the collection will track changes to the items.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="items"/> is <see langword="null"/>.</exception>
    [StswInfo("0.20.0", "0.20.1")]
    public StswCollectionViewWrapper(IEnumerable<T> items, bool trackItems = true)
    {
        Items = new(items ?? throw new ArgumentNullException(nameof(items)), trackItems);
        Source = new CollectionViewSource { Source = Items };
    }

    /// <summary>
    /// Replaces the current items in the collection with the specified items.
    /// </summary>
    /// <param name="items">The new items to replace the current items.</param>
    [StswInfo("0.20.0")]
    public void ReplaceWith(IEnumerable<T> items)
    {
        Items.Clear();
        Items.AddRangeFast(items);
    }
}
