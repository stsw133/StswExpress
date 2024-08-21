using System;
using System.Collections.Generic;

namespace StswExpress;

/// <summary>
/// Arguments for the store changed event, containing the new collection of items.
/// </summary>
/// <typeparam name="TModel">The type of the items in the store.</typeparam>
/// <param name="newItems">The new collection of items in the store.</param>
public class StswStoreChangedArgs<TModel>(IEnumerable<TModel> newItems) : EventArgs
{
    /// <summary>
    /// Gets the new collection of items in the store.
    /// </summary>
    public IEnumerable<TModel> NewItems { get; private set; } = newItems;
}
