using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress;
/// <summary>
/// A control that displays a collection of filter tags, allowing users to include or exclude specific tags.
/// </summary>
[Stsw("0.17.0", Changes = StswPlannedChanges.Refactor | StswPlannedChanges.VisualChanges)]
public class StswFilterTags : ItemsControl, IStswCornerControl
{
    static StswFilterTags()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswFilterTags), new FrameworkPropertyMetadata(typeof(StswFilterTags)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswFilterTagsItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswFilterTagsItem;

    #region Events & methods
    /// <summary>
    /// Sets the selected tag as included or excluded based on the provided item.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public string? GetTagDisplayValue(object? item)
    {
        if (item == null)
            return null;

        if (item is string str)
            return str;

        if (!string.IsNullOrWhiteSpace(DisplayMemberPath))
        {
            var prop = item.GetType().GetProperty(DisplayMemberPath);
            if (prop != null && prop.GetValue(item) is string value)
                return value;
        }

        return null;
    }

    /// <summary>
    /// Updates the string representation of the selected tags based on the included and excluded tags.
    /// </summary>
    public void UpdateSelectedTagsString() => SelectedTags = string.Join(" ", IncludedTags.Concat(ExcludedTags.Select(t => "-" + t)));
    #endregion

    #region Logic properties
    /// <summary>
    /// Determines whether the user is allowed to enter custom tags that are not part of the ItemsSource.
    /// </summary>
    public bool AllowCustomTags
    {
        get => (bool)GetValue(AllowCustomTagsProperty);
        set => SetValue(AllowCustomTagsProperty, value);
    }
    public static readonly DependencyProperty AllowCustomTagsProperty =
        DependencyProperty.Register(
            nameof(AllowCustomTags),
            typeof(bool),
            typeof(StswFilterTags)
        );

    /// <summary>
    /// The string representation of the selected tags.
    /// </summary>
    public string SelectedTags
    {
        get => (string)GetValue(SelectedTagsProperty);
        set => SetValue(SelectedTagsProperty, value);
    }
    public static readonly DependencyProperty SelectedTagsProperty
        = DependencyProperty.Register(
            nameof(SelectedTags),
            typeof(string),
            typeof(StswFilterTags),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedTagsChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnSelectedTagsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswFilterTags stsw)
            return;

        if (e.NewValue is not string str)
            return;

        var rawTags = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var included = new HashSet<string>();
        var excluded = new HashSet<string>();

        foreach (var tag in rawTags)
        {
            if (tag.StartsWith('-'))
                excluded.Add(tag[1..]);
            else
                included.Add(tag);
        }

        foreach (var conflict in included.Intersect(excluded).ToList())
        {
            included.Remove(conflict);
            excluded.Remove(conflict);
        }

        if (!stsw.AllowCustomTags)
        {
            var validTags = stsw.ItemsSource?.Cast<object>()
                .Select(stsw.GetTagDisplayValue)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .ToHashSet();

            included.RemoveWhere(tag => !validTags?.Contains(tag) ?? true);
            excluded.RemoveWhere(tag => !validTags?.Contains(tag) ?? true);
        }

        stsw.IncludedTags.Clear();
        stsw.ExcludedTags.Clear();

        foreach (var tag in included)
            stsw.IncludedTags.Add(tag);
        foreach (var tag in excluded)
            stsw.ExcludedTags.Add(tag);

        var cleaned = string.Join(' ', included.Concat(excluded.Select(t => "-" + t)));
        if (cleaned != str)
            stsw.Dispatcher.BeginInvoke(() =>
            {
                stsw.SetCurrentValue(SelectedTagsProperty, cleaned);
                BindingExpression? be = BindingOperations.GetBindingExpression(stsw, SelectedTagsProperty);
                be?.UpdateTarget();
            });
    }
    public IList<string> IncludedTags { get; } = [];
    public IList<string> ExcludedTags { get; } = [];
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
            typeof(StswFilterTags),
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
            typeof(StswFilterTags),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswFilterTags Header="Name" FilterType="Text" FilterMode="Contains" FilterValuePath="Name"/>

*/
