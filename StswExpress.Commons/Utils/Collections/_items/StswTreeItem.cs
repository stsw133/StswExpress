using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StswExpress.Commons;

/// <summary>
/// Represents a hierarchical tree node used to store arbitrarily deep nested data.
/// Wraps a value of type <typeparamref name="T"/> and exposes a collection of children.
/// Designed for MVVM / WPF scenarios (ObservableCollection + INotifyPropertyChanged).
/// </summary>
/// <typeparam name="T">Type of the payload stored in this node.</typeparam>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// var root = new StswTreeItem&lt;MyModel&gt;(new MyModel { Name = "Root" });
/// 
/// var child1 = root.AddChild(new MyModel { Name = "Child 1" });
/// var child2 = root.AddChild(new MyModel { Name = "Child 2" });
/// 
/// child1.AddChild(new MyModel { Name = "Child 1.1" });
/// child1.AddChild(new MyModel { Name = "Child 1.2" });
/// 
/// child2.AddChild(new MyModel { Name = "Child 2.1" });
/// </code>
/// </example>
public class StswTreeItem<T> : INotifyPropertyChanged, IEnumerable<StswTreeItem<T>>
{
    public StswTreeItem(T value)
    {
        _value = value;
        Children = [];
        Children.CollectionChanged += OnChildrenCollectionChanged;
    }

    /// <summary>
    /// Value stored in this node. It can be any model used in your domain.
    /// </summary>
    public T Value
    {
        get => _value;
        set
        {
            if (!Equals(_value, value))
            {
                _value = value;
                OnPropertyChanged();
            }
        }
    }
    private T _value;

    /// <summary>
    /// Parent node in the tree. Null for a root node.
    /// </summary>
    public StswTreeItem<T>? Parent
    {
        get => _parent;
        private set
        {
            if (!ReferenceEquals(_parent, value))
            {
                _parent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Root));
                OnPropertyChanged(nameof(Depth));
            }
        }
    }
    private StswTreeItem<T>? _parent;

    /// <summary>
    /// Children of this node. Each child is itself a tree item that can hold further children.
    /// </summary>
    public ObservableCollection<StswTreeItem<T>> Children { get; }

    /// <summary>
    /// Returns the root of the tree this node belongs to (the highest ancestor).
    /// </summary>
    public StswTreeItem<T> Root => Parent is null ? this : Parent.Root;

    /// <summary>
    /// Depth of this node in the tree (0 = root).
    /// </summary>
    public int Depth => Parent is null ? 0 : Parent.Depth + 1;

    /// <summary>
    /// Adds a new child with the given value and returns the created node.
    /// </summary>
    /// <returns>The newly created child node.</returns>
    public StswTreeItem<T> AddChild(T value)
    {
        var child = new StswTreeItem<T>(value);
        Children.Add(child);
        return child;
    }

    /// <summary>
    /// Adds an existing node as a child of this node. Parent is updated automatically.
    /// </summary>
    public void AddChild(StswTreeItem<T> child)
    {
        ArgumentNullException.ThrowIfNull(child);

        // If the child already had a parent, detach from previous parent.
        if (child.Parent is not null && !ReferenceEquals(child.Parent, this))
            child.Parent!.Children.Remove(child);

        Children.Add(child);
    }

    /// <summary>
    /// Removes the specified child from this node's children.
    /// </summary>
    /// <returns><see langword="true"/> if the child was found and removed; otherwise, <see langword="false"/>.</returns>
    public bool RemoveChild(StswTreeItem<T> child)
    {
        if (child is null)
            return false;

        var removed = Children.Remove(child);
        if (removed && ReferenceEquals(child.Parent, this))
            child.Parent = null;

        return removed;
    }

    /// <summary>
    /// Enumerates this node and all its descendants in pre-order.
    /// </summary>
    /// <param name="includeSelf">If <see langword="true"/>, iteration starts with this node. Otherwise, only descendants are returned.</param>
    public IEnumerable<StswTreeItem<T>> Traverse(bool includeSelf = true)
    {
        if (includeSelf)
            yield return this;

        foreach (var child in Children)
        {
            foreach (var descendant in child.Traverse(true))
                yield return descendant;
        }
    }

    /// <summary>
    /// Returns the first node in this subtree that matches the given predicate, or <see langword="null"/>.
    /// </summary>
    /// <returns>The first matching node, or <see langword="null"/> if none found.</returns>
    public StswTreeItem<T>? Find(Predicate<StswTreeItem<T>> match)
    {
        ArgumentNullException.ThrowIfNull(match);

        foreach (var node in Traverse())
            if (match(node))
                return node;

        return null;
    }

    /// <summary>
    /// Enumerates all ancestor nodes, starting from the parent and walking up to the root.
    /// </summary>
    /// <returns>Enumerator over ancestor nodes.</returns>
    public IEnumerable<StswTreeItem<T>> Ancestors()
    {
        var current = Parent;
        while (current is not null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    /// <summary>
    /// Enumerates all descendant nodes (children, grandchildren, etc.).
    /// </summary>
    public IEnumerable<StswTreeItem<T>> Descendants() => Traverse(includeSelf: false);

    /// <summary>
    /// Enumerates this node and all its descendants (pre-order). Equivalent to Traverse(true).
    /// </summary>
    /// <returns>Enumerator over this node and its descendants.</returns>
    public IEnumerator<StswTreeItem<T>> GetEnumerator() => Traverse().GetEnumerator();

    /// <summary>
    /// Non-generic enumerator implementation.
    /// </summary>
    /// <returns>Enumerator over this node and its descendants.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Handles changes to the Children collection to maintain parent-child relationships.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event args describing the change.</param>
    private void OnChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
            foreach (StswTreeItem<T> item in e.NewItems)
                if (!ReferenceEquals(item.Parent, this))
                    item.Parent = this;

        if (e.OldItems is not null)
            foreach (StswTreeItem<T> item in e.OldItems)
                if (ReferenceEquals(item.Parent, this))
                    item.Parent = null;

        if (e.Action == NotifyCollectionChangedAction.Reset)
            foreach (var child in Children)
                if (ReferenceEquals(child.Parent, this))
                    child.Parent = this;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
