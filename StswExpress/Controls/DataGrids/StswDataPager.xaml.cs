using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;
/// <summary>
/// Represents a control that displays a page navigation for a collection of items.
/// </summary>
public class StswDataPager : ContentControl, IStswCornerControl
{
    public StswDataPager()
    {
        SetValue(PageButtonsProperty, new ObservableCollection<StswDataPagerButtonModel>());
    }
    static StswDataPager()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDataPager), new FrameworkPropertyMetadata(typeof(StswDataPager)));
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the list of items displayed on the current page.
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
    /// Gets or sets the collection of items to be paginated.
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
            if (stsw.ItemsSource != null && stsw.ItemsPerPage != 0)
                stsw.PageLast = (int)Math.Ceiling((double)stsw.ItemsSource.Count / stsw.ItemsPerPage);
            else
                stsw.PageLast = 1;

            if (stsw.PageCurrent > stsw.PageLast)
                stsw.PageCurrent = stsw.PageLast;
            else
                OnPageCurrentChanged(stsw, new DependencyPropertyChangedEventArgs());
        }
    }

    /// <summary>
    /// Gets the collection of page buttons for navigation within the data pager.
    /// </summary>
    internal ObservableCollection<StswDataPagerButtonModel> PageButtons
    {
        get => (ObservableCollection<StswDataPagerButtonModel>)GetValue(PageButtonsProperty);
        set => SetValue(PageButtonsProperty, value);
    }
    internal static readonly DependencyProperty PageButtonsProperty
        = DependencyProperty.Register(
            nameof(PageButtons),
            typeof(ObservableCollection<StswDataPagerButtonModel>),
            typeof(StswDataPager)
        );

    /// <summary>
    /// Gets or sets the current page number being displayed.
    /// </summary>
    public int PageCurrent
    {
        get => (int)GetValue(PageCurrentProperty);
        internal set => SetValue(PageCurrentProperty, value);
    }
    public static readonly DependencyProperty PageCurrentProperty
        = DependencyProperty.Register(
            nameof(PageCurrent),
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
            if (stsw.ItemsSource != null)
            {
                int currPage = stsw.PageCurrent, lastPage = stsw.PageLast;
                var min = Math.Max((currPage - 1) * stsw.ItemsPerPage, 0);
                var max = Math.Min(currPage * stsw.ItemsPerPage, stsw.ItemsSource.Count);

                /// generate items on page
                var temp = new List<object?>();
                for (int i = min; i < max; i++)
                    temp.Add(stsw.ItemsSource[i]);
                stsw.ItemsOnPage = temp;

                /// generate pages buttons
                var pages = new ObservableCollection<StswDataPagerButtonModel>()
                {
                    new() { Description = "   🡄   ", Page = currPage - 1, IsEnabled = currPage > 1 },
                    new() { Description = "   🡆   ", Page = currPage + 1, IsEnabled = currPage < lastPage },
                };
                for (int i = Math.Min(lastPage, 9); i >= 1; i--)
                    pages.Insert(1, new() { Description = i.ToString(), Page = i, IsSelected = i == currPage });

                if (lastPage > 9)
                {
                    if (currPage.Between(6, lastPage - 5))
                        for (int i = 3; i <= 7; i++)
                            pages[i] = new() { Description = (currPage - 5 + i).ToString(), Page = currPage - 5 + i, IsSelected = currPage == currPage - 5 + i };
                    else if (currPage > lastPage - 5)
                        for (int i = 3; i <= 8; i++)
                            pages[i] = new() { Description = (lastPage - 9 + i).ToString(), Page = lastPage - 9 + i, IsSelected = currPage == lastPage - 9 + i };

                    if (currPage >= 6)
                        pages[2] = new() { Description = "...", IsEnabled = false };
                    if (currPage <= lastPage - 5)
                        pages[8] = new() { Description = "...", IsEnabled = false };

                    pages[9] = new() { Description = lastPage.ToString(), Page = lastPage, IsSelected = currPage == lastPage };
                }

                stsw.PageButtons = pages;
            }
        }
    }
    public StswCommand<int> PageChangeCommand => new((x) => PageCurrent = x);

    /// <summary>
    /// Gets the last page number available based on the provided <see cref="ItemsSource"/> and <see cref="ItemsPerPage"/>.
    /// </summary>
    public int PageLast
    {
        get => (int)GetValue(PageLastProperty);
        internal set => SetValue(PageLastProperty, value);
    }
    public static readonly DependencyProperty PageLastProperty
        = DependencyProperty.Register(
            nameof(PageLast),
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
            typeof(StswDataPager)
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
            typeof(StswDataPager)
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
            typeof(StswDataPager)
        );
    #endregion
}

/// <summary>
/// Data model for <see cref="StswDataPager"/>'s page buttons.
/// </summary>
internal class StswDataPagerButtonModel
{
    /// <summary>
    /// Gets or sets the content displayed on the button.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the button is selected.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the button is enabled for interaction.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the page number to which the button navigates when clicked.
    /// </summary>
    public int Page { get; set; }
}
