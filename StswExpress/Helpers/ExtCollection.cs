using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace StswExpress;
public delegate void ListedItemPropertyChangedEventHandler(IList sourceList, object Item, PropertyChangedEventArgs e);
public class ExtCollection<T> : ObservableCollection<T>
{
    #region Constructors
    public ExtCollection() : base()
    {
        CollectionChanged += ObservableCollection_CollectionChanged;
    }
    public ExtCollection(IEnumerable<T> c) : base(c)
    {
        CollectionChanged += ObservableCollection_CollectionChanged;
    }
    public ExtCollection(List<T> l) : base(l)
    {
        CollectionChanged += ObservableCollection_CollectionChanged;
    }
    #endregion

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

                if (e.Action == NotifyCollectionChangedAction.Remove && (item as BaseM).ItemState != System.Data.DataRowState.Added)
                    (item as BaseM).ItemState = System.Data.DataRowState.Deleted;
            }

        if (e.NewItems != null)
            foreach (var item in e.NewItems)
            {
                if (item != null && item is INotifyPropertyChanged i)
                {
                    i.PropertyChanged -= Element_PropertyChanged;
                    i.PropertyChanged += Element_PropertyChanged;
                }

                if (e.Action == NotifyCollectionChangedAction.Add)
                    (item as BaseM).ItemState = System.Data.DataRowState.Added;
                else if (e.Action == NotifyCollectionChangedAction.Replace && (item as BaseM).ItemState != System.Data.DataRowState.Added)
                    (item as BaseM).ItemState = System.Data.DataRowState.Modified;
            }
    }

    private void Element_PropertyChanged(object sender, PropertyChangedEventArgs e) => ItemPropertyChanged?.Invoke(this, sender, e);

    public ListedItemPropertyChangedEventHandler ItemPropertyChanged;
}