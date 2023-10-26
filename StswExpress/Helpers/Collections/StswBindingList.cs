using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace StswExpress;

/// <summary>
/// An extension of <see cref="BindingList{T}"/> that adds tracking of changes to the collection items' states.
/// </summary>
public class StswBindingList<T> : BindingList<T>, INotifyPropertyChanged where T : IStswCollectionItem
{
    public StswBindingList() : base()
    {
    }
    public StswBindingList(IEnumerable<T> items) : base(items.ToList())
    {
    }
    public StswBindingList(IList<T> items) : base(items)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnListChanged(ListChangedEventArgs e)
    {
        base.OnListChanged(e);

        if (e == null || e.ListChangedType == ListChangedType.ItemDeleted)
            return;

        if (Items[e.NewIndex] is T item && item.ItemState == StswItemState.Unchanged)
        {
            if (e.PropertyDescriptor?.Name?.In(IgnoredProperties) == true)
                return;

            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                _itemStates[Items[e.NewIndex]] = Items[e.NewIndex].ItemState = StswItemState.Modified;
                OnPropertyChanged(nameof(Modified));
            }
            else if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                _itemStates[Items[e.NewIndex]] = Items[e.NewIndex].ItemState = StswItemState.Added;
                OnPropertyChanged(nameof(Added));
                OnPropertyChanged(nameof(Count));
            }
        }
    }

    /// <summary>
    /// Clears the collection and marks all items as "Deleted" if it was not previously "Added".
    /// </summary>
    protected override void ClearItems()
    {
        foreach (var item in this)
        {
            if (item.ItemState != StswItemState.Added)
                _itemStates[item] = item.ItemState = StswItemState.Deleted;
            else
                _itemStates.Remove(item);
        }

        base.ClearItems();
        NotifyStateChanges();
    }
    
    /// <summary>
    /// Removes the item at the specified index from the collection and marks it as "Deleted" if it was not previously "Added".
    /// </summary>
    protected override void RemoveItem(int index)
    {
        var item = this[index];

        if (item.ItemState != StswItemState.Added)
            _itemStates[item] = item.ItemState = StswItemState.Deleted;
        else
            _itemStates.Remove(item);

        base.RemoveItem(index);
        NotifyStateChanges();
    }

    /// <summary>
    /// Gets the state of a specific collection item.
    /// </summary>
    public StswItemState GetStateOfItem(T item)
    {
        if (_itemStates.TryGetValue(item, out var state))
            return state;

        return StswItemState.Unchanged;
    }
    private readonly Dictionary<T, StswItemState> _itemStates = new();

    /// <summary>
    /// Gets a list of collection items that match the specified DataRowState.
    /// </summary>
    public IEnumerable<T> GetItemsByState(StswItemState state) => _itemStates.Where(x => x.Value == state).Select(x => x.Key);
    public int Added => GetItemsByState(StswItemState.Added).Count();
    public int Modified => GetItemsByState(StswItemState.Modified).Count();
    public int Deleted => GetItemsByState(StswItemState.Deleted).Count();

    public List<string> IgnoredProperties = new List<string>()
    {
        nameof(IStswCollectionItem.ItemMessage),
        nameof(IStswCollectionItem.ItemState),
        nameof(IStswCollectionItem.ShowDetails)
    };



    /// Notify the view that the ItemStates property has changed
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void NotifyStateChanges()
    {
        OnPropertyChanged(nameof(Added));
        OnPropertyChanged(nameof(Modified));
        OnPropertyChanged(nameof(Deleted));
        OnPropertyChanged(nameof(Count));
    }
}
