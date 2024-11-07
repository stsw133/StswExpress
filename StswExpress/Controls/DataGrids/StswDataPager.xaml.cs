using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;
/// <summary>
/// Represents a control that provides pagination for a collection of items with navigation buttons.
/// </summary>
public class StswDataPager : ContentControl, IStswCornerControl
{
    public StswDataPager()
    {
        SetValue(PagesProperty, new ObservableCollection<StswDataPagerPage>());
    }
    static StswDataPager()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDataPager), new FrameworkPropertyMetadata(typeof(StswDataPager)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswDataPager), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <summary>
    /// 
    /// </summary>
    public StswCommand<int> PageChangeCommand => new((x) => CurrentPage = x);

    /// <summary>
    /// Calculates the total number of pages based on <see cref="ItemsSource"/> and <see cref="ItemsPerPage"/> values.
    /// Sets <see cref="CurrentPage"/> to the highest valid page if it exceeds the total.
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
    /// Displays items on the currently selected page by setting <see cref="ItemsOnPage"/> 
    /// and updates the navigation buttons to reflect the current page selection.
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
    /// Generates the collection of navigation buttons based on the current page and total pages,
    /// including "..." buttons when there are too many pages to display at once.
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
                OnPageCurrentChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnPageCurrentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswDataPager stsw)
        {
            stsw.RefreshCurrentPageItems();
        }
    }

    /// <summary>
    /// Gets or sets the list of items currently displayed on the page.
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
        if (obj is StswDataPager stsw)
        {
            stsw.RecalculatePagination();
        }
    }

    /// <summary>
    /// Gets the collection of navigation buttons for the data pager.
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
    /// Total number of available pages based on <see cref="ItemsSource"/> and <see cref="ItemsPerPage"/>.
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
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
    /// </summary>
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
    /// Gets or sets the thickness of the separator between box and drop-down button.
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

/// <summary>
/// Data model for <see cref="StswDataPager"/>'s page buttons.
/// </summary>
internal struct StswDataPagerPage(string description, int page, bool isEnabled)
{
    /// <summary>
    /// Gets or sets the content displayed on the button.
    /// </summary>
    public string Description { get; set; } = description;

    /// <summary>
    /// Gets or sets the page number to which the button navigates when clicked.
    /// </summary>
    public int Page { get; set; } = page;

    /// <summary>
    /// Gets or sets a value indicating whether the button is enabled for interaction.
    /// </summary>
    public bool IsEnabled { get; set; } = isEnabled;

    /// <summary>
    /// Gets or sets a value indicating whether the button is selected.
    /// </summary>
    public bool IsSelected { get; set; }
}
