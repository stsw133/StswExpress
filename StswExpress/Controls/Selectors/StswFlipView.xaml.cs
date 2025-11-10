using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// A flip view control that allows users to navigate through items using arrow buttons, keyboard, or mouse wheel.
/// Supports looping, selection binding, read-only mode, and customizable corner styling.
/// </summary>
/// <remarks>
/// This control enables intuitive navigation through a collection of items, providing smooth user interaction.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswFlipView ItemsSource="{Binding NewsArticles}" IsLoopingEnabled="True"/&gt;
/// </code>
/// </example>
[TemplatePart(Name = "PART_ButtonPrevious", Type = typeof(ButtonBase))]
[TemplatePart(Name = "PART_ButtonNext", Type = typeof(ButtonBase))]
public class StswFlipView : Selector, IStswCornerControl, IStswSelectionControl
{
    private ButtonBase? _buttonPrevious, _buttonNext;

    static StswFlipView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswFlipView), new FrameworkPropertyMetadata(typeof(StswFlipView)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        if (_buttonPrevious != null)
        {
            _buttonPrevious.Click -= OnButtonPreviousClick;
            _buttonPrevious = null;
        }

        if (_buttonNext != null)
        {
            _buttonNext.Click -= OnButtonNextClick;
            _buttonNext = null;
        }

        base.OnApplyTemplate();

        /// Button: previous
        if (GetTemplateChild("PART_ButtonPrevious") is ButtonBase buttonPrevious)
        {
            buttonPrevious.Click += OnButtonPreviousClick;
            _buttonPrevious = buttonPrevious;
        }
        /// Button: next
        if (GetTemplateChild("PART_ButtonNext") is ButtonBase buttonNext)
        {
            buttonNext.Click += OnButtonNextClick;
            _buttonNext = buttonNext;
        }

        UpdateNavigationButtons();
    }

    /// <inheritdoc/>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        IStswSelectionControl.ItemsSourceChanged(this, newValue);
        base.OnItemsSourceChanged(oldValue, newValue);

        if (SelectedIndex < 0 && Items.Count > 0)
            SelectedIndex = 0;

        UpdateNavigationButtons();
    }

    /// <inheritdoc/>
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        UpdateNavigationButtons();
    }

    /// <inheritdoc/>
    protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        IStswSelectionControl.ItemTemplateChanged(this, newItemTemplate);
        base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
    }

    /// <inheritdoc/>
    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        base.OnPreviewMouseWheel(e);

        if (IsKeyboardFocusWithin && !IsReadOnly && Items.Count > 0)
        {
            ShiftBy(e.Delta > 0 ? -1 : 1);
            e.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (!IsKeyboardFocusWithin || IsReadOnly || Items.Count == 0)
            return;

        var handled = false;

        switch (e.Key)
        {
            case Key.Left:
            case Key.Up:
                ShiftBy(-1);
                handled = true;
                break;
            case Key.Down:
            case Key.Right:
                ShiftBy(1);
                handled = true;
                break;
            case Key.PageDown:
                ShiftBy(-10);
                handled = true;
                break;
            case Key.PageUp:
                ShiftBy(10);
                handled = true;
                break;
            case Key.Home:
                if (Items.Count > 0)
                {
                    SelectedIndex = 0;
                    handled = true;
                }
                break;
            case Key.End:
                if (Items.Count > 0)
                {
                    SelectedIndex = Items.Count - 1;
                    handled = true;
                }
                break;
            case Key.Tab:
                (Keyboard.FocusedElement as UIElement)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                handled = true;
                break;
        }

        if (handled)
            e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        UpdateNavigationButtons();
    }

    /// <summary>
    /// Determines whether shifting by a specified step is possible, considering item count and looping settings.
    /// </summary>
    /// <param name="step">The step value for shifting through items.</param>
    /// <returns><see langword="true"/> if shifting is possible; otherwise, <see langword="false"/>.</returns>
    private bool CanShiftBy(int step)
    {
        if (Items.Count == 0)
            return false;

        var targetIndex = SelectedIndex + step;
        return targetIndex >= 0 && targetIndex < Items.Count;
    }

    /// <summary>
    /// Changes the selected index by the specified step, considering looping and boundary conditions.
    /// </summary>
    /// <param name="step">The number of positions to shift.</param>
    private void ShiftBy(int step)
    {
        if (IsReadOnly)
            return;

        if (Items.Count == 0)
        {
            SelectedIndex = -1; return;
        }

        int targetIndex;

        if (IsLoopingEnabled)
        {
            targetIndex = (SelectedIndex + step) % Items.Count;
            if (targetIndex < 0)
                targetIndex += Items.Count;
        }
        else if (CanShiftBy(step))
        {
            targetIndex = SelectedIndex + step;
        }
        else
        {
            targetIndex = step > 0 ? Items.Count - 1 : 0;
        }

        if (targetIndex != SelectedIndex)
            SelectedIndex = targetIndex;
    }

    /// <summary>
    /// Updates the enabled state of the navigation buttons based on the current selection and control settings.
    /// </summary>
    private void UpdateNavigationButtons()
    {
        var hasItems = Items.Count > 0;
        var canLoop = IsLoopingEnabled && Items.Count > 1;

        if (_buttonPrevious != null)
            _buttonPrevious.IsEnabled = !IsReadOnly && hasItems && (canLoop || CanShiftBy(-1));

        if (_buttonNext != null)
            _buttonNext.IsEnabled = !IsReadOnly && hasItems && (canLoop || CanShiftBy(1));
    }

    /// <summary>
    /// Handles the click event for the previous button, shifting the selection backward by one item.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OnButtonPreviousClick(object sender, RoutedEventArgs e) => ShiftBy(-1);

    /// <summary>
    /// Handles the click event for the next button, shifting the selection forward by one item.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OnButtonNextClick(object sender, RoutedEventArgs e) => ShiftBy(1);
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether looping is enabled when reaching the first or last item.
    /// When enabled, navigating past the last item wraps around to the first, and vice versa.
    /// </summary>
    public bool IsLoopingEnabled
    {
        get => (bool)GetValue(IsLoopingEnabledProperty);
        set => SetValue(IsLoopingEnabledProperty, value);
    }
    public static readonly DependencyProperty IsLoopingEnabledProperty
        = DependencyProperty.Register(
            nameof(IsLoopingEnabled),
            typeof(bool),
            typeof(StswFlipView),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsLoopingEnabledChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnIsLoopingEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswFlipView stsw)
            return;

        stsw.UpdateNavigationButtons();
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswFlipView),
            new FrameworkPropertyMetadata(default(bool), OnIsReadOnlyChanged)
        );
    private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswFlipView stsw)
            return;

        stsw.UpdateNavigationButtons();
    }
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
            typeof(StswFlipView),
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
            typeof(StswFlipView),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
