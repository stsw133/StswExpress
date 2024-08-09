using System;
using System.Collections;
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
public class StswDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IXmlSerializable, INotifyCollectionChanged, INotifyPropertyChanged where TKey : notnull
{
    private const string CountString = "Count";
    private const string IndexerName = "Item[]";
    private const string KeysName = "Keys";
    private const string ValuesName = "Values";

    protected IDictionary<TKey, TValue> Dictionary => _dictionary;
    private IDictionary<TKey, TValue> _dictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="StswDictionary{TKey, TValue}"/> class.
    /// </summary>
    public StswDictionary() => _dictionary = new Dictionary<TKey, TValue>();

    /// <summary>
    /// Initializes a new instance of the <see cref="StswDictionary{TKey, TValue}"/> class with the specified dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary whose elements are copied to the new dictionary.</param>
    public StswDictionary(IDictionary<TKey, TValue> dictionary) => _dictionary = new Dictionary<TKey, TValue>(dictionary);
    
    /// <summary>
    /// Initializes a new instance of the <see cref="StswDictionary{TKey, TValue}"/> class with the specified comparer.
    /// </summary>
    /// <param name="comparer">The comparer to use when comparing keys.</param>
    public StswDictionary(IEqualityComparer<TKey> comparer) => _dictionary = new Dictionary<TKey, TValue>(comparer);

    /// <summary>
    /// Initializes a new instance of the <see cref="StswDictionary{TKey, TValue}"/> class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The initial number of elements that the dictionary can contain.</param>
    public StswDictionary(int capacity) => _dictionary = new Dictionary<TKey, TValue>(capacity);

    /// <summary>
    /// Initializes a new instance of the <see cref="StswDictionary{TKey, TValue}"/> class with the specified dictionary and comparer.
    /// </summary>
    /// <param name="dictionary">The dictionary whose elements are copied to the new dictionary.</param>
    /// <param name="comparer">The comparer to use when comparing keys.</param>
    public StswDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) => _dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);

    /// <summary>
    /// Initializes a new instance of the <see cref="StswDictionary{TKey, TValue}"/> class with the specified capacity and comparer.
    /// </summary>
    /// <param name="capacity">The initial number of elements that the dictionary can contain.</param>
    /// <param name="comparer">The comparer to use when comparing keys.</param>
    public StswDictionary(int capacity, IEqualityComparer<TKey> comparer) => _dictionary = new Dictionary<TKey, TValue>(capacity, comparer);

    /// <summary>
    /// Adds an element with the provided key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    public void Add(TKey key, TValue value) => Insert(key, value, true);

    /// <summary>
    /// Determines whether the dictionary contains a specific key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns><see langword="true"/> if the dictionary contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
    public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);

    /// <summary>
    /// Gets a collection containing the keys in the dictionary.
    /// </summary>
    public ICollection<TKey> Keys => Dictionary.Keys;

    /// <summary>
    /// Removes the element with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>true if the element is successfully removed; otherwise, false. This method also returns false if key is not found in the dictionary.</returns>
    public bool Remove(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        Dictionary.TryGetValue(key, out TValue? value);
        var removed = Dictionary.Remove(key);
        if (removed)
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value!));

        return removed;
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns>true if the dictionary contains an element with the specified key; otherwise, false.</returns>
    public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value!);

    /// <summary>
    /// Gets a collection containing the values in the dictionary.
    /// </summary>
    public ICollection<TValue> Values => Dictionary.Values;

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get or set.</param>
    /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="KeyNotFoundException"/>, and a set operation creates a new element with the specified key.</returns>
    public TValue this[TKey key]
    {
        get => Dictionary.TryGetValue(key, out var value) ? value : default!;
        set => Insert(key, value, false);
    }

    /// <summary>
    /// Adds the specified key-value pair to the dictionary.
    /// </summary>
    /// <param name="item">The key-value pair to add.</param>
    public void Add(KeyValuePair<TKey, TValue> item) => Insert(item.Key, item.Value, true);

    /// <summary>
    /// Removes all items from the dictionary.
    /// </summary>
    public void Clear()
    {
        if (Dictionary.Count > 0)
        {
            Dictionary.Clear();
            OnCollectionChanged();
        }
    }

    /// <summary>
    /// Determines whether the dictionary contains the specified key-value pair.
    /// </summary>
    /// <param name="item">The key-value pair to locate in the dictionary.</param>
    /// <returns>true if the dictionary contains an element with the specified key-value pair; otherwise, false.</returns>
    public bool Contains(KeyValuePair<TKey, TValue> item) => Dictionary.Contains(item);

    /// <summary>
    /// Copies the elements of the dictionary to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements copied from the dictionary. The array must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => Dictionary.CopyTo(array, arrayIndex);

    /// <summary>
    /// Gets the number of elements contained in the dictionary.
    /// </summary>
    public int Count => Dictionary.Count;

    /// <summary>
    /// Gets a value indicating whether the dictionary is read-only.
    /// </summary>
    public bool IsReadOnly => Dictionary.IsReadOnly;

    /// <summary>
    /// Removes the specified key-value pair from the dictionary.
    /// </summary>
    /// <param name="item">The key-value pair to remove.</param>
    /// <returns>true if the key-value pair is successfully removed; otherwise, false. This method also returns false if the key-value pair was not found in the dictionary.</returns>
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

    /// <summary>
    /// Returns an enumerator that iterates through the dictionary.
    /// </summary>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the dictionary.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Dictionary).GetEnumerator();

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Adds a range of key-value pairs to the dictionary.
    /// </summary>
    /// <param name="items">The key-value pairs to add.</param>
    public void AddRange(IDictionary<TKey, TValue> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count > 0)
        {
            if (Dictionary.Count > 0)
            {
                if (items.Keys.Any(Dictionary.ContainsKey))
                    throw new ArgumentException("An item with the same key has already been added.");
                else
                    foreach (var item in items) Dictionary.Add(item);
            }
            else
                _dictionary = new Dictionary<TKey, TValue>(items);

            OnCollectionChanged(NotifyCollectionChangedAction.Add, items.ToArray());
        }
    }

    /// <summary>
    /// Adds the specified key-value pair to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    /// <param name="add">true to add the element; false to update the element if it already exists.</param>
    private void Insert(TKey key, TValue value, bool add)
    {
        ArgumentNullException.ThrowIfNull(key);

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

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    private void OnPropertyChanged()
    {
        OnPropertyChanged(CountString);
        OnPropertyChanged(IndexerName);
        OnPropertyChanged(KeysName);
        OnPropertyChanged(ValuesName);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Raises the <see cref="CollectionChanged"/> event.
    /// </summary>
    private void OnCollectionChanged()
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Raises the <see cref="CollectionChanged"/> event with the specified action and changed item.
    /// </summary>
    /// <param name="action">The action that caused the event.</param>
    /// <param name="changedItem">The item that was changed.</param>
    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> changedItem)
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, changedItem));
    }

    /// <summary>
    /// Raises the <see cref="CollectionChanged"/> event with the specified action, new item, and old item.
    /// </summary>
    /// <param name="action">The action that caused the event.</param>
    /// <param name="newItem">The new item that was added.</param>
    /// <param name="oldItem">The old item that was replaced.</param>
    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> newItem, KeyValuePair<TKey, TValue> oldItem)
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
    }

    /// <summary>
    /// Raises the <see cref="CollectionChanged"/> event with the specified action and new items.
    /// </summary>
    /// <param name="action">The action that caused the event.</param>
    /// <param name="newItems">The new items that were added.</param>
    private void OnCollectionChanged(NotifyCollectionChangedAction action, IList newItems)
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItems));
    }

    /// <summary>
    /// Gets the XML schema for the dictionary serialization.
    /// </summary>
    public XmlSchema? GetSchema() => null;

    /// <summary>
    /// Reads XML configuration and builds the dictionary.
    /// </summary>
    /// <param name="reader">The XML reader to read from.</param>
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

            Add(key!, value!);

            reader.ReadEndElement();
            reader.MoveToContent();
        }
        reader.ReadEndElement();
    }

    /// <summary>
    /// Writes the dictionary as XML.
    /// </summary>
    /// <param name="writer">The XML writer to write to.</param>
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
