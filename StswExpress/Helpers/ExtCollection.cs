using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace StswExpress;

public delegate void ListedItemPropertyChangedEventHandler(IList sourceList, object Item, PropertyChangedEventArgs e);
public class ExtCollection<T> : ObservableCollection<T>
{
    public ExtCollection() : base() => CollectionChanged += ObservableCollection_CollectionChanged;
    public ExtCollection(IEnumerable<T> c) : base(c) => CollectionChanged += ObservableCollection_CollectionChanged;
    public ExtCollection(List<T> l) : base(l) => CollectionChanged += ObservableCollection_CollectionChanged;

    public new void Clear()
    {
        foreach (var item in this)
            if (item is INotifyPropertyChanged i)
                i.PropertyChanged -= Element_PropertyChanged;
        base.Clear();
    }

    private void ObservableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
            foreach (var item in e.OldItems)
            {
                if (item != null && item is INotifyPropertyChanged i)
                    i.PropertyChanged -= Element_PropertyChanged;
                /*
                var pi = item?.GetType().GetProperty(nameof(StswModel.ItemState));
                if (pi != null)
                {
                    if (e.Action == NotifyCollectionChangedAction.Remove && (DataRowState)pi.GetValue(item) != DataRowState.Added)
                        pi.SetValue(item, DataRowState.Deleted);
                }
                */
            }

        if (e.NewItems != null)
            foreach (var item in e.NewItems)
            {
                if (item != null && item is INotifyPropertyChanged i)
                {
                    i.PropertyChanged -= Element_PropertyChanged;
                    i.PropertyChanged += Element_PropertyChanged;
                }
                /*
                var pi = item?.GetType().GetProperty(nameof(StswModel.ItemState));
                if (pi != null)
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                        pi.SetValue(item, DataRowState.Added);
                    else if (e.Action == NotifyCollectionChangedAction.Replace && (DataRowState)pi.GetValue(item) != DataRowState.Added)
                        pi.SetValue(item, DataRowState.Modified);
                }
                */
            }
    }

    private void Element_PropertyChanged(object sender, PropertyChangedEventArgs e) => ItemPropertyChanged?.Invoke(this, sender, e);

    public ListedItemPropertyChangedEventHandler ItemPropertyChanged;
}