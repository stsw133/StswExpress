using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace StswExpress.Commons;

/// <summary>
/// Represents a dictionary that provides detailed change notifications, useful for data binding in WPF.
/// </summary>
/// <typeparam name="TKey">Type of dictionary keys.</typeparam>
/// <typeparam name="TValue">Type of dictionary values.</typeparam>
[Stsw("0.16.0", Changes = StswPlannedChanges.None)]
public class StswObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _dictionary = [];

    /// <summary>
    /// Determines whether missing keys should be automatically added when accessed via the indexer.
    /// If set to false, accessing a missing key will throw a KeyNotFoundException.
    /// Default is true.
    /// </summary>
    public bool AutoAddOnGet { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class.
    /// </summary>
    public StswObservableDictionary() => _dictionary = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class with a specified comparer.
    /// </summary>
    public StswObservableDictionary(IEqualityComparer<TKey> comparer) => _dictionary = new Dictionary<TKey, TValue>(comparer);

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class with a specified capacity and comparer.
    /// </summary>
    public StswObservableDictionary(int capacity, IEqualityComparer<TKey> comparer) => _dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class with an existing dictionary.
    /// </summary>
    public StswObservableDictionary(IDictionary<TKey, TValue> dictionary) => _dictionary = new Dictionary<TKey, TValue>(dictionary);

    /// <inheritdoc/>
    public TValue this[TKey key]
    {
        get
        {
            if (!_dictionary.TryGetValue(key, out TValue? value))
            {
                if (!AutoAddOnGet)
                    throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

                value = default!;
                _dictionary[key] = value;
                NotifyChanges(NotifyCollectionChangedAction.Add, value);
            }
            return value;
        }
        set => Insert(key, value, false);
    }

    /// <inheritdoc/>
    public ICollection<TKey> Keys => _dictionary.Keys;

    /// <inheritdoc/>
    public ICollection<TValue> Values => _dictionary.Values;

    /// <inheritdoc/>
    public int Count => _dictionary.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public void Add(TKey key, TValue value) => Insert(key, value, true);

    /// <inheritdoc/>
    public bool Remove(TKey key)
    {
        if (_dictionary.TryGetValue(key, out var removedValue) && _dictionary.Remove(key))
        {
            NotifyChanges(NotifyCollectionChangedAction.Remove, removedValue);
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    /// <inheritdoc/>
    public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value!);

    /// <inheritdoc/>
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    /// <inheritdoc/>
    public void Clear()
    {
        if (_dictionary.Count > 0)
        {
            _dictionary.Clear();
            NotifyChanges(NotifyCollectionChangedAction.Reset);
        }
    }

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary.ContainsKey(item.Key);

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

    /// <summary>
    /// Adds a range of key-value pairs to the dictionary.
    /// </summary>
    public void AddRange(IDictionary<TKey, TValue> items)
    {
        foreach (var item in items)
            _dictionary[item.Key] = item.Value;
        NotifyChanges(NotifyCollectionChangedAction.Add);
    }

    /// <summary>
    /// Handles item insertion and updates.
    /// </summary>
    private void Insert(TKey key, TValue value, bool add)
    {
        if (_dictionary.TryGetValue(key, out var oldValue))
        {
            if (add) throw new ArgumentException("An item with the same key has already been added.");
            if (Equals(oldValue, value)) return;
            _dictionary[key] = value;
            NotifyChanges(NotifyCollectionChangedAction.Replace, value, oldValue);
        }
        else
        {
            _dictionary[key] = value;
            NotifyChanges(NotifyCollectionChangedAction.Add, value);
        }
    }

    /// <summary>
    /// Triggers change notifications for property and collection changes.
    /// </summary>
    private void NotifyChanges(NotifyCollectionChangedAction action, TValue? newValue = default, TValue? oldValue = default)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));

        CollectionChanged?.Invoke(this, action switch
        {
            NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(action, newValue!),
            NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(action, oldValue!),
            NotifyCollectionChangedAction.Replace => new NotifyCollectionChangedEventArgs(action, newValue!, oldValue!),
            NotifyCollectionChangedAction.Reset => new NotifyCollectionChangedEventArgs(action),
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        });
    }

    /// <inheritdoc/>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;
}
