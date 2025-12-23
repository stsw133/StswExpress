using System.Windows;
using System.Windows.Controls;

namespace StswExpress.Wpf;
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

    #region Dependency properties
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
            typeof(StswFilterTagsItem)
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
            typeof(StswFilterTagsItem)
        );

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

    #region Template
    private Button? _btnAdd;
    private Button? _btnRemove;
    private Button? _btnSelect;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        DetachTemplateEvents();
        _btnAdd = GetTemplateChild("PART_Add") as Button;
        _btnRemove = GetTemplateChild("PART_Remove") as Button;
        _btnSelect = GetTemplateChild("PART_Select") as Button;
        AttachTemplateEvents();
    }

    /// <summary>
    /// Attaches event handlers to the template parts.
    /// </summary>
    private void AttachTemplateEvents()
    {
        if (_btnAdd != null)
            _btnAdd.Click += BtnAdd_Click;
        if (_btnRemove != null)
            _btnRemove.Click += BtnRemove_Click;
        if (_btnSelect != null)
            _btnSelect.Click += BtnSelect_Click;
    }

    /// <summary>
    /// Detaches event handlers from the template parts.
    /// </summary>
    private void DetachTemplateEvents()
    {
        if (_btnAdd != null)
            _btnAdd.Click -= BtnAdd_Click;
        if (_btnRemove != null)
            _btnRemove.Click -= BtnRemove_Click;
        if (_btnSelect != null)
            _btnSelect.Click -= BtnSelect_Click;
    }
    #endregion

    #region Logic
    /// <summary>
    /// Sets the current item as included.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void BtnAdd_Click(object sender, RoutedEventArgs e) => SetIncluded();

    /// <summary>
    /// Sets the current item as excluded.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void BtnRemove_Click(object sender, RoutedEventArgs e) => SetExcluded();

    /// <summary>
    /// Selects only this item.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void BtnSelect_Click(object sender, RoutedEventArgs e) => SelectOnlyThis();

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
}
