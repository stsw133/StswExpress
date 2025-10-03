using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// Represents an individual item inside the <see cref="StswFilterTags"/>.
/// Supports selection state binding and corner customization.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswFilterTags&gt;
///     &lt;se:StswFilterTagsItem Content="Tag 1"/&gt;
///     &lt;se:StswFilterTagsItem Content="Tag 2"/&gt;
/// &lt;/se:StswListBox&gt;
/// </code>
/// </example>
public class StswFilterTagsItem : ContentControl, IStswCornerControl
{
    static StswFilterTagsItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswFilterTagsItem), new FrameworkPropertyMetadata(typeof(StswFilterTagsItem)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Add
        if (GetTemplateChild("PART_Add") is Button btnAdd)
            btnAdd.Click += (_, _) => SetIncluded();

        /// Remove
        if (GetTemplateChild("PART_Remove") is Button btnRemove)
            btnRemove.Click += (_, _) => SetExcluded();

        /// Select
        if (GetTemplateChild("PART_Select") is Button btnSelect)
            btnSelect.Click += (_, _) => SelectOnlyThis();
    }

    /// <summary>
    /// Sets the current item as included.
    /// </summary>
    private void SetIncluded()
    {
        if (StswFnUI.FindVisualAncestor<StswFilterTags>(this) is StswFilterTags parent)
        {
            var tag = parent.GetTagDisplayValue(Content);
            if (tag is null)
                return;

            parent.IncludedTags.AddIfNotContains(tag);
            parent.ExcludedTags.Remove(tag);
            parent.UpdateSelectedTagsString();
        }
    }

    /// <summary>
    /// Sets the current item as excluded.
    /// </summary>
    private void SetExcluded()
    {
        if (StswFnUI.FindVisualAncestor<StswFilterTags>(this) is StswFilterTags parent)
        {
            var tag = parent.GetTagDisplayValue(Content);
            if (tag is null)
                return;

            parent.ExcludedTags.AddIfNotContains(tag);
            parent.IncludedTags.Remove(tag);
            parent.UpdateSelectedTagsString();
        }
    }

    /// <summary>
    /// Selects the current item and clears all other selections.
    /// </summary>
    private void SelectOnlyThis()
    {
        if (StswFnUI.FindVisualAncestor<StswFilterTags>(this) is StswFilterTags parent)
        {
            var tag = parent.GetTagDisplayValue(Content);
            if (tag is null)
                return;

            parent.IncludedTags.Clear();
            parent.ExcludedTags.Clear();
            parent.IncludedTags.Add(tag);
            parent.UpdateSelectedTagsString();
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the item is in read-only mode.
    /// When set to <see langword="true"/>, the item becomes unselectable and unclickable.
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswFilterTagsItem)
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
            typeof(StswFilterTagsItem),
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
            typeof(StswFilterTagsItem),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
