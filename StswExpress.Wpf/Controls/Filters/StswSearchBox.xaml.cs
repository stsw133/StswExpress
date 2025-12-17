using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress.Wpf;
/// <summary>
/// A text-based search control that filters the provided <see cref="ItemsSource"/> collection based on the entered <see cref="FilterText"/>.
/// </summary>
public class StswSearchBox : StswTextBox, IStswCornerControl
{
    private ICollectionView? _itemsView;

    static StswSearchBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSearchBox), new FrameworkPropertyMetadata(typeof(StswSearchBox)));
    }

    #region Events & methods
    /// <inheritdoc/>
    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);

        if (FilterText != Text)
            FilterText = Text;
    }

    /// <summary>
    /// Filters the collection based on <see cref="FilterText"/> and <see cref="FilterMemberPath"/>.
    /// </summary>
    /// <param name="obj">The object to evaluate.</param>
    /// <returns><see langword="true"/> when the object should be included.</returns>
    private bool CollectionViewFilter(object obj)
    {
        return MatchesFilter(obj);
    }

    /// <summary>
    /// Determines if the given object matches the current filter criteria.
    /// </summary>
    /// <param name="obj">The object to check against the filter.</param>
    /// <returns><see langword="true"/> if the object matches the filter; otherwise, <see langword="false"/>.</returns>
    private bool MatchesFilter(object obj)
    {
        var filterText = FilterText?.Trim();
        if (string.IsNullOrEmpty(filterText))
            return true;

        var candidate = string.IsNullOrEmpty(FilterMemberPath)
            ? obj?.ToString()
            : obj.GetPropertyValue(FilterMemberPath)?.ToString();

        return candidate?.IndexOf(filterText, System.StringComparison.CurrentCultureIgnoreCase) >= 0;
    }

    /// <summary>
    /// Re-applies the filter to the collection view.
    /// </summary>
    private void RefreshFilter()
    {
        if (_itemsView is null)
            return;

        DetachFilter();

        if (_itemsView.CanFilter)
            _itemsView.Filter += CollectionViewFilter;

        _itemsView.Refresh();
    }

    /// <summary>
    /// Detaches the filter from the collection view.
    /// </summary>
    private void DetachFilter()
    {
        if (_itemsView is null || !_itemsView.CanFilter)
            return;

        _itemsView.Filter -= CollectionViewFilter;
    }

    /// <summary>
    /// Called when <see cref="ItemsSource"/> changes.
    /// </summary>
    /// <param name="oldValue">The old collection.</param>
    /// <param name="newValue">The new collection.</param>
    protected virtual void OnItemsSourceChanged(IEnumerable? oldValue, IEnumerable? newValue)
    {
        DetachFilter();
        _itemsView = newValue != null ? CollectionViewSource.GetDefaultView(newValue) : null;
        RefreshFilter();
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// The path to the property used for filtering when the object is not a string.
    /// </summary>
    public string? FilterMemberPath
    {
        get => (string?)GetValue(FilterMemberPathProperty);
        set => SetValue(FilterMemberPathProperty, value);
    }
    public static readonly DependencyProperty FilterMemberPathProperty
        = DependencyProperty.Register(
            nameof(FilterMemberPath),
            typeof(string),
            typeof(StswSearchBox),
            new PropertyMetadata(null, OnFilterMemberPathChanged)
        );
    private static void OnFilterMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswSearchBox)d;
        stsw.RefreshFilter();
    }

    /// <summary>
    /// The current text used to filter <see cref="ItemsSource"/>.
    /// </summary>
    public string? FilterText
    {
        get => (string?)GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }
    public static readonly DependencyProperty FilterTextProperty
        = DependencyProperty.Register(
            nameof(FilterText),
            typeof(string),
            typeof(StswSearchBox),
            new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFilterTextChanged)
        );
    private static void OnFilterTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswSearchBox)d;
        if (stsw.Text != stsw.FilterText)
            stsw.Text = stsw.FilterText;
        stsw.RefreshFilter();
    }

    /// <summary>
    /// Gets or sets the collection that is used to generate the content of the StswSelectionBox.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    public static readonly DependencyProperty ItemsSourceProperty
        = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(StswSearchBox),
            new PropertyMetadata(null, OnItemsSourceChanged)
        );
    public static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswSearchBox)d;
        stsw.OnItemsSourceChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
    }
    #endregion
}
