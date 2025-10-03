using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public StswDataPager()
    {
        SetValue(PagesProperty, new ObservableCollection<StswDataPagerPage>());
    }
    static StswDataPager()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDataPager), new FrameworkPropertyMetadata(typeof(StswDataPager)));
    }

    #region Events & methods
    /// <summary>
    /// Gets the command that changes the current page.
    /// The command parameter is the target page number.
    /// </summary>
    public StswCommand<int> PageChangeCommand => new((x) => CurrentPage = x);

    /// <summary>
    /// Recalculates the total number of pages based on <see cref="ItemsSource"/> and <see cref="ItemsPerPage"/>.
    /// Ensures that <see cref="CurrentPage"/> remains within valid bounds and updates displayed items.
    /// </summary>
    private void RecalculatePagination()
    {
        if (ItemsSource == null || ItemsPerPage <= 0)
        {
            TotalPages = 1;
            CurrentPage = 1;
            return;
        }

        TotalPages = (int)Math.Ceiling((double)ItemsSource.Count / ItemsPerPage);
        CurrentPage = Math.Min(CurrentPage, TotalPages);
        RefreshCurrentPageItems();
    }

    /// <summary>
    /// Updates the displayed items for the currently selected page.
    /// Populates <see cref="ItemsOnPage"/> with the correct subset of data and regenerates navigation buttons.
    /// </summary>
    private void RefreshCurrentPageItems()
    {
        if (ItemsSource == null || !CurrentPage.Between(1, TotalPages))
            return;

        var min = (CurrentPage - 1) * ItemsPerPage;
        var max = Math.Min(CurrentPage * ItemsPerPage, ItemsSource.Count);

        var temp = new List<object?>();
        for (var i = min; i < max; i++)
            temp.Add(ItemsSource[i]);
        
        ItemsOnPage = temp;

        GenerateNavigationButtons();
    }

    /// <summary>
    /// Generates navigation buttons dynamically based on the total number of pages.
    /// Includes "..." buttons for compact pagination when the page count is large.
    /// </summary>
    private void GenerateNavigationButtons()
    {
        var pages = new List<StswDataPagerPage>
        {
            new StswDataPagerPage("   🡄   ", CurrentPage - 1, CurrentPage > 1),
            new StswDataPagerPage("   🡆   ", CurrentPage + 1, CurrentPage < TotalPages)
        };

        for (var i = Math.Min(TotalPages, 9); i >= 1; i--)
        {
            pages.Insert(1, new StswDataPagerPage(i.ToString(), i, true)
            {
                IsSelected = i == CurrentPage
            });
        }

        if (TotalPages > 9)
        {
            if (CurrentPage.Between(6, TotalPages - 5))
            {
                for (var i = 3; i <= 7; i++)
                {
                    pages[i] = new StswDataPagerPage((CurrentPage - 5 + i).ToString(), CurrentPage - 5 + i, true)
                    {
                        IsSelected = CurrentPage == (CurrentPage - 5 + i)
                    };
                }
            }
            else if (CurrentPage > (TotalPages - 5))
            {
                for (var i = 3; i <= 8; i++)
                {
                    pages[i] = new StswDataPagerPage((TotalPages - 9 + i).ToString(), TotalPages - 9 + i, true)
                    {
                        IsSelected = CurrentPage == (TotalPages - 9 + i)
                    };
                }
            }

            if (CurrentPage >= 6)
                pages[2] = new StswDataPagerPage("...", default, false);
            if (CurrentPage <= (TotalPages - 5))
                pages[8] = new StswDataPagerPage("...", default, false);

            pages[9] = new StswDataPagerPage(TotalPages.ToString(), TotalPages, true)
            {
                IsSelected = CurrentPage == TotalPages
            };
        }

        Pages = new ObservableCollection<StswDataPagerPage>(pages);
    }
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
                OnCurrentPageChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnCurrentPageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswDataPager stsw)
            return;

        stsw.RefreshCurrentPageItems();
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
            typeof(StswDataPager)
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
            new FrameworkPropertyMetadata(default(int), OnItemsSourceChanged)
        );

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
    public static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswDataPager stsw)
            return;

        stsw.RecalculatePagination();
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
            typeof(StswDataPager)
        );
    #endregion

    #region Style properties
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
