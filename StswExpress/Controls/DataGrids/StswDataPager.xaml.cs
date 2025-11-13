using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;
/// <summary>
/// Represents a pagination control for navigating through a large dataset by displaying a subset of items per page.
/// Provides navigation buttons for switching pages, including automatic handling of page limits.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDataPager ItemsSource="{Binding LargeDataset}" ItemsPerPage="10" CurrentPage="1"/&gt;
/// </code>
/// </example>
public class StswDataPager : ContentControl, IStswCornerControl
{
    private INotifyCollectionChanged? _observableItemsSource;

    public StswDataPager()
    {
        _pageChangeCommand = new StswCommand<int>(OnPageChangeRequested);
        SetValue(PagesProperty, new ObservableCollection<StswDataPagerPage>());
        Loaded += OnLoaded;
    }
    static StswDataPager()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDataPager), new FrameworkPropertyMetadata(typeof(StswDataPager)));
        //StswControl.OverrideBaseBorderThickness<StswDataPager>(getExt: c => c.BorderThickness, setExt: (c, st) => c.BorderThickness = st);
    }

    #region Events & methods
    /// <summary>
    /// Handles the Loaded event to initialize pagination when the control is first loaded.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        RefreshPagination(true);
    }

    /// <summary>
    /// Gets the command that changes the current page.
    /// The command parameter is the target page number.
    /// </summary>
    public StswCommand<int> PageChangeCommand => _pageChangeCommand;
    private readonly StswCommand<int> _pageChangeCommand;

    /// <summary>
    /// Handles page change requests by updating <see cref="CurrentPage"/> to the specified target page.
    /// </summary>
    /// <param name="targetPage">The target page number to navigate to.</param>
    private void OnPageChangeRequested(int targetPage)
    {
        var clampedPage = Math.Clamp(targetPage, 1, TotalPages);
        if (clampedPage != CurrentPage)
            SetCurrentValue(CurrentPageProperty, clampedPage);
    }

    /// <summary>
    /// Refreshes the pagination state.
    /// </summary>
    /// <param name="recalculateTotalPages">Indicates whether to recalculate the total number of pages.</param>
    private void RefreshPagination(bool recalculateTotalPages)
    {
        if (!IsLoaded)
            return;

        if (recalculateTotalPages)
        {
            var itemsPerPage = Math.Max(1, ItemsPerPage);
            var totalItems = ItemsSource?.Count ?? 0;
            var totalPages = totalItems > 0
                ? (int)Math.Ceiling(totalItems / (double)itemsPerPage)
                : 1;

            SetCurrentValue(TotalPagesProperty, totalPages);
        }

        var currentPage = Math.Clamp(CurrentPage, 1, TotalPages);
        if (currentPage != CurrentPage)
        {
            SetCurrentValue(CurrentPageProperty, currentPage);
            return;
        }

        UpdateItemsOnPage(currentPage);
        UpdateNavigation(currentPage, TotalPages);
    }

    /// <summary>
    /// Updates the items displayed on the current page based on the specified page number.
    /// </summary>
    /// <param name="currentPage">The current page number.</param>
    private void UpdateItemsOnPage(int currentPage)
    {
        if (ItemsSource == null || ItemsSource.Count == 0)
        {
            SetCurrentValue(ItemsOnPageProperty, Array.Empty<object?>());
            return;
        }

        var itemsPerPage = Math.Max(1, ItemsPerPage);
        var startIndex = (currentPage - 1) * itemsPerPage;
        if (startIndex >= ItemsSource.Count)
        {
            SetCurrentValue(ItemsOnPageProperty, Array.Empty<object?>());
            return;
        }

        var endIndex = Math.Min(startIndex + itemsPerPage, ItemsSource.Count);
        var count = endIndex - startIndex;

        var pageItems = new List<object?>(count);
        for (var i = startIndex; i < endIndex; i++)
            pageItems.Add(ItemsSource[i]);

        SetCurrentValue(ItemsOnPageProperty, pageItems);
    }

    /// <summary>
    /// Updates the navigation buttons based on the current page and total pages.
    /// </summary>
    /// <param name="currentPage">The current page number.</param>
    /// <param name="totalPages">The total number of pages.</param>
    private void UpdateNavigation(int currentPage, int totalPages)
    {
        totalPages = Math.Max(1, totalPages);
        var pages = BuildNavigationPages(currentPage, totalPages);
        if (Pages is not ObservableCollection<StswDataPagerPage> collection)
        {
            collection = [];
            SetValue(PagesProperty, collection);
        }
        else
        {
            collection.Clear();
        }

        foreach (var page in pages)
            collection.Add(page);
    }

    /// <summary>
    /// Builds the navigation pages including previous/next buttons and page numbers.
    /// </summary>
    /// <param name="currentPage">The current page number.</param>
    /// <param name="totalPages">The total number of pages.</param>
    /// <returns>An enumerable of <see cref="StswDataPagerPage"/> representing the navigation buttons.</returns>
    private static IEnumerable<StswDataPagerPage> BuildNavigationPages(int currentPage, int totalPages)
    {
        var previousPage = Math.Max(1, currentPage - 1);
        yield return new StswDataPagerPage("   🡄   ", previousPage, currentPage > 1);

        foreach (var descriptor in EnumeratePageNumbers(currentPage, totalPages))
        {
            if (descriptor.HasValue)
            {
                yield return new StswDataPagerPage(descriptor.Value.ToString(), descriptor.Value, true)
                {
                    IsSelected = descriptor.Value == currentPage
                };
            }
            else
            {
                yield return new StswDataPagerPage("...", 0, false);
            }
        }

        var nextPage = Math.Min(totalPages, currentPage + 1);
        yield return new StswDataPagerPage("   🡆   ", nextPage, currentPage < totalPages);
    }

    /// <summary>
    /// Generates a sequence of page numbers for navigation, including "..." placeholders for large datasets.
    /// </summary>
    /// <param name="currentPage">The current page number.</param>
    /// <param name="totalPages">The total number of pages.</param>
    /// <returns>An enumerable of nullable integers representing page numbers or null for "..." placeholders.</returns>
    private static IEnumerable<int?> EnumeratePageNumbers(int currentPage, int totalPages)
    {
        if (totalPages <= 0)
            yield break;

        if (totalPages <= 9)
        {
            for (var i = 1; i <= totalPages; i++)
                yield return i;
            yield break;
        }

        yield return 1;

        const int windowSize = 5;
        var start = Math.Max(2, currentPage - windowSize / 2);
        var end = Math.Min(totalPages - 1, start + windowSize - 1);
        start = Math.Max(2, end - windowSize + 1);

        if (start > 2)
            yield return null;
        else
            start = 2;

        for (var i = start; i <= end; i++)
            yield return i;

        if (end < totalPages - 1)
            yield return null;

        yield return totalPages;
    }

    /// <summary>
    /// Updates the subscription to collection change events for the items source.
    /// </summary>
    /// <param name="newSource">The new items source collection.</param>
    private void UpdateItemsSourceSubscription(IList? newSource)
    {
        if (_observableItemsSource is not null)
            CollectionChangedEventManager.RemoveHandler(_observableItemsSource, ItemsSourceCollectionChanged);

        _observableItemsSource = newSource as INotifyCollectionChanged;

        if (_observableItemsSource is not null)
            CollectionChangedEventManager.AddHandler(_observableItemsSource, ItemsSourceCollectionChanged);
    }

    /// <summary>
    /// Handles changes in the items source collection by refreshing the pagination.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void ItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshPagination(true);
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the currently selected page number.
    /// Changing this property updates the displayed items accordingly.
    /// </summary>
    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }
    public static readonly DependencyProperty CurrentPageProperty
        = DependencyProperty.Register(
            nameof(CurrentPage),
            typeof(int),
            typeof(StswDataPager),
            new FrameworkPropertyMetadata(default(int),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnCurrentPageChanged, CoerceCurrentPage, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswDataPager stsw)
            return;

        stsw.RefreshPagination(false);
    }
    private static object CoerceCurrentPage(DependencyObject d, object baseValue)
    {
        if (d is not StswDataPager pager)
            return baseValue;

        var value = (int)baseValue;
        var maximum = Math.Max(1, pager.TotalPages);
        return Math.Clamp(value, 1, maximum);
    }

    /// <summary>
    /// Gets or sets the collection of items currently displayed on the selected page.
    /// This property updates automatically when the page changes.
    /// </summary>
    public IList ItemsOnPage
    {
        get => (IList)GetValue(ItemsOnPageProperty);
        set => SetValue(ItemsOnPageProperty, value);
    }
    public static readonly DependencyProperty ItemsOnPageProperty
        = DependencyProperty.Register(
            nameof(ItemsOnPage),
            typeof(IList),
            typeof(StswDataPager),
            new FrameworkPropertyMetadata(Array.Empty<object?>())
        );

    /// <summary>
    /// Gets or sets the number of items displayed per page.
    /// Changing this property triggers a recalculation of pagination.
    /// </summary>
    public int ItemsPerPage
    {
        get => (int)GetValue(ItemsPerPageProperty);
        set => SetValue(ItemsPerPageProperty, value);
    }
    public static readonly DependencyProperty ItemsPerPageProperty
        = DependencyProperty.Register(
            nameof(ItemsPerPage),
            typeof(int),
            typeof(StswDataPager),
            new FrameworkPropertyMetadata(10, OnItemsPerPageChanged, CoerceItemsPerPage)
        );
    private static void OnItemsPerPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswDataPager stsw)
            return;

        stsw.RefreshPagination(true);
    }
    private static object CoerceItemsPerPage(DependencyObject d, object baseValue)
    {
        var value = (int)baseValue;
        return Math.Max(1, value);
    }

    /// <summary>
    /// Gets or sets the full collection of items to be paginated.
    /// The data pager divides this collection into multiple pages.
    /// </summary>
    public IList ItemsSource
    {
        get => (IList)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    public static readonly DependencyProperty ItemsSourceProperty
        = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IList),
            typeof(StswDataPager),
            new FrameworkPropertyMetadata(default(IList),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnItemsSourceChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswDataPager stsw)
            return;

        stsw.UpdateItemsSourceSubscription(e.NewValue as IList);
        stsw.RefreshPagination(true);
    }

    /// <summary>
    /// Gets the collection of navigation buttons used for paging.
    /// Includes numbered buttons, previous/next controls, and optional "..." buttons for large datasets.
    /// </summary>
    internal ObservableCollection<StswDataPagerPage> Pages
    {
        get => (ObservableCollection<StswDataPagerPage>)GetValue(PagesProperty);
        set => SetValue(PagesProperty, value);
    }
    internal static readonly DependencyProperty PagesProperty
        = DependencyProperty.Register(
            nameof(Pages),
            typeof(ObservableCollection<StswDataPagerPage>),
            typeof(StswDataPager)
        );

    /// <summary>
    /// Gets or sets the total number of available pages based on <see cref="ItemsSource"/> and <see cref="ItemsPerPage"/>.
    /// </summary>
    internal int TotalPages
    {
        get => (int)GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }
    public static readonly DependencyProperty TotalPagesProperty
        = DependencyProperty.Register(
            nameof(TotalPages),
            typeof(int),
            typeof(StswDataPager),
            new FrameworkPropertyMetadata(1, null, CoerceTotalPages)
        );
    private static object CoerceTotalPages(DependencyObject d, object baseValue)
    {
        var value = (int)baseValue;
        return Math.Max(1, value);
    }
    #endregion

    #region Style properties
    /*
    /// <summary>
    /// Gets or sets the thickness of the border, including the inner separator value.
    /// </summary>
    public new StswThickness BorderThickness
    {
        get => (StswThickness)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }
    public new static readonly DependencyProperty BorderThicknessProperty
        = DependencyProperty.Register(
            nameof(BorderThickness),
            typeof(StswThickness),
            typeof(StswDataPager),
            new FrameworkPropertyMetadata(default(StswThickness),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                StswControl.CreateExtendedChangedCallback<StswDataPager>((c, th) => c.SetCurrentValue(Control.BorderThicknessProperty, th)))
        );
    */

    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswDataPager),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswDataPager),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between the page panel and the items section.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty
        = DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswDataPager),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
