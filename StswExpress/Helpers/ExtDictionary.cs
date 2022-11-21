using System.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace StswExpress;

[XmlRoot("Dictionary")]
public class ExtDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IXmlSerializable, INotifyCollectionChanged, INotifyPropertyChanged
{
    private const string CountString = "Count";
    private const string IndexerName = "Item[]";
    private const string KeysName = "Keys";
    private const string ValuesName = "Values";

    private IDictionary<TKey, TValue> _Dictionary;
    protected IDictionary<TKey, TValue> Dictionary => _Dictionary;

    public ExtDictionary() => _Dictionary = new Dictionary<TKey, TValue>();
    public ExtDictionary(IDictionary<TKey, TValue> dictionary) => _Dictionary = new Dictionary<TKey, TValue>(dictionary);
    public ExtDictionary(IEqualityComparer<TKey> comparer) => _Dictionary = new Dictionary<TKey, TValue>(comparer);
    public ExtDictionary(int capacity) => _Dictionary = new Dictionary<TKey, TValue>(capacity);
    public ExtDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) => _Dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
    public ExtDictionary(int capacity, IEqualityComparer<TKey> comparer) => _Dictionary = new Dictionary<TKey, TValue>(capacity, comparer);

    #region IDictionary<TKey,TValue> Members
    public void Add(TKey key, TValue value) => Insert(key, value, true);
    public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);
    public ICollection<TKey> Keys => Dictionary.Keys;
    public bool Remove(TKey key)
    {
        if (key == null) throw new ArgumentNullException("key");

        Dictionary.TryGetValue(key, out TValue value);
        var removed = Dictionary.Remove(key);
        if (removed)
            //OnCollectionChanged(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value));
            OnCollectionChanged();

        return removed;
    }
    public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value);
    public ICollection<TValue> Values => Dictionary.Values;
    public TValue this[TKey key]
    {
        get
        {
            if (!Dictionary.ContainsKey(key))
            {
                Dictionary.TryGetValue(key, out var value);
                return value;
            }
            return Dictionary[key];
        }
        set => Insert(key, value, false);
    }
    #endregion

    #region ICollection<KeyValuePair<TKey,TValue>> Members
    public void Add(KeyValuePair<TKey, TValue> item) => Insert(item.Key, item.Value, true);
    public void Clear()
    {
        if (Dictionary.Count > 0)
        {
            Dictionary.Clear();
            OnCollectionChanged();
        }
    }
    public bool Contains(KeyValuePair<TKey, TValue> item) => Dictionary.Contains(item);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => Dictionary.CopyTo(array, arrayIndex);
    public int Count => Dictionary.Count;
    public bool IsReadOnly => Dictionary.IsReadOnly;
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
    #endregion

    #region IEnumerable<KeyValuePair<TKey,TValue>> Members
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();
    #endregion

    #region IEnumerable Members
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Dictionary).GetEnumerator();
    #endregion

    #region INotifyCollectionChanged Members
    public event NotifyCollectionChangedEventHandler CollectionChanged;
    #endregion

    #region INotifyPropertyChanged Members
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    public void AddRange(IDictionary<TKey, TValue> items)
    {
        if (items == null) throw new ArgumentNullException("items");

        if (items.Count > 0)
        {
            if (Dictionary.Count > 0)
            {
                if (items.Keys.Any((k) => Dictionary.ContainsKey(k)))
                    throw new ArgumentException("An item with the same key has already been added.");
                else
                    foreach (var item in items) Dictionary.Add(item);
            }
            else
                _Dictionary = new Dictionary<TKey, TValue>(items);

            OnCollectionChanged(NotifyCollectionChangedAction.Add, items.ToArray());
        }
    }

    private void Insert(TKey key, TValue value, bool add)
    {
        if (key == null) throw new ArgumentNullException("key");

        if (Dictionary.TryGetValue(key, out TValue item))
        {
            if (add) throw new ArgumentException("An item with the same key has already been added.");
            if (Equals(item, value)) return;
            Dictionary[key] = value;

            OnCollectionChanged(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(key, value), new KeyValuePair<TKey, TValue>(key, item));
        }
        else
        {
            Dictionary[key] = value;

            OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value));
        }
    }

    private void OnPropertyChanged()
    {
        OnPropertyChanged(CountString);
        OnPropertyChanged(IndexerName);
        OnPropertyChanged(KeysName);
        OnPropertyChanged(ValuesName);
    }

    protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void OnCollectionChanged()
    {
        OnPropertyChanged();
        if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> changedItem)
    {
        OnPropertyChanged();
        if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, changedItem));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> newItem, KeyValuePair<TKey, TValue> oldItem)
    {
        OnPropertyChanged();
        if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, IList newItems)
    {
        OnPropertyChanged();
        if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, newItems));
    }

    public XmlSchema? GetSchema() => null;

    /// ReadXml
    public void ReadXml(XmlReader reader)
    {
        var keySerializer = new XmlSerializer(typeof(TKey));
        var valueSerializer = new XmlSerializer(typeof(TValue));

        var wasEmpty = reader.IsEmptyElement;
        reader.Read();
        
        if (wasEmpty)
            return;

        while (reader.NodeType != XmlNodeType.EndElement)
        {
            reader.ReadStartElement("item");

            reader.ReadStartElement("key");
            var key = (TKey?)keySerializer.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("value");
            var value = (TValue?)valueSerializer.Deserialize(reader);
            reader.ReadEndElement();

            Add(key, value);

            reader.ReadEndElement();
            reader.MoveToContent();
        }
        reader.ReadEndElement();
    }

    /// WriteXml
    public void WriteXml(XmlWriter writer)
    {
        var keySerializer = new XmlSerializer(typeof(TKey));
        var valueSerializer = new XmlSerializer(typeof(TValue));

        foreach (TKey key in Keys)
        {
            writer.WriteStartElement("item");

            writer.WriteStartElement("key");
            keySerializer.Serialize(writer, key);
            writer.WriteEndElement();

            writer.WriteStartElement("value");
            TValue value = this[key];
            valueSerializer.Serialize(writer, value);
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
    }
}
