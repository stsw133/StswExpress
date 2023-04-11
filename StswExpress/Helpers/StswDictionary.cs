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

/// <summary>
/// Custom implementation of a dictionary that is designed to raise events when its contents are modified, allowing for better integration with data binding.
/// </summary>
[XmlRoot("Dictionary")]
public class StswDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IXmlSerializable, INotifyCollectionChanged, INotifyPropertyChanged
{
    private const string CountString = "Count";
    private const string IndexerName = "Item[]";
    private const string KeysName = "Keys";
    private const string ValuesName = "Values";

    private IDictionary<TKey, TValue> _Dictionary;
    protected IDictionary<TKey, TValue> Dictionary => _Dictionary;

    public StswDictionary() => _Dictionary = new Dictionary<TKey, TValue>();
    public StswDictionary(IDictionary<TKey, TValue> dictionary) => _Dictionary = new Dictionary<TKey, TValue>(dictionary);
    public StswDictionary(IEqualityComparer<TKey> comparer) => _Dictionary = new Dictionary<TKey, TValue>(comparer);
    public StswDictionary(int capacity) => _Dictionary = new Dictionary<TKey, TValue>(capacity);
    public StswDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) => _Dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
    public StswDictionary(int capacity, IEqualityComparer<TKey> comparer) => _Dictionary = new Dictionary<TKey, TValue>(capacity, comparer);

    #region Members
    /// <see cref="IDictionary{TKey, TValue}"/> members
    public void Add(TKey key, TValue value) => Insert(key, value, true);
    public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);
    public ICollection<TKey> Keys => Dictionary.Keys;
    public bool Remove(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

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

    /// <see cref="ICollection{KeyValuePair{TKey, TValue}}"/> members
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

    /// <see cref="IEnumerable{KeyValuePair{TKey, TValue}}"/> members
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();

    /// <see cref="IEnumerable"/> members
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Dictionary).GetEnumerator();

    /// <see cref="INotifyCollectionChanged"/> members
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <see cref="INotifyPropertyChanged"/> members
    public event PropertyChangedEventHandler? PropertyChanged;
    #endregion

    /// AddRange
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

    /// Insert
    private void Insert(TKey key, TValue value, bool add)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        if (Dictionary.TryGetValue(key, out TValue? item))
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

    /// OnPropertyChanged
    private void OnPropertyChanged()
    {
        OnPropertyChanged(CountString);
        OnPropertyChanged(IndexerName);
        OnPropertyChanged(KeysName);
        OnPropertyChanged(ValuesName);
    }
    protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// OnCollectionChanged
    private void OnCollectionChanged()
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> changedItem)
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, changedItem));
    }
    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> newItem, KeyValuePair<TKey, TValue> oldItem)
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
    }
    private void OnCollectionChanged(NotifyCollectionChangedAction action, IList newItems)
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItems));
    }

    /// GetSchema
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
