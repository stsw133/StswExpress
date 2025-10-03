using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
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
public class StswFlipView : Selector, IStswCornerControl, IStswSelectionControl
{
    private ButtonBase? _buttonPrevious, _buttonNext;

    public StswFlipView()
    {
        DependencyPropertyDescriptor.FromProperty(IsReadOnlyProperty, typeof(Selector)).AddValueChanged(this, CheckButtonAccessibility);
        DependencyPropertyDescriptor.FromProperty(SelectedIndexProperty, typeof(Selector)).AddValueChanged(this, CheckButtonAccessibility);
    }
    static StswFlipView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswFlipView), new FrameworkPropertyMetadata(typeof(StswFlipView)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: previous
        if (GetTemplateChild("PART_ButtonPrevious") is ButtonBase buttonPrevious)
        {
            buttonPrevious.IsEnabled = CanShiftBy(-1) && !IsReadOnly;
            buttonPrevious.Click += (_, _) => ShiftBy(-1);
            _buttonPrevious = buttonPrevious;
        }
        /// Button: next
        if (GetTemplateChild("PART_ButtonNext") is ButtonBase buttonNext)
        {
            buttonNext.IsEnabled = CanShiftBy(1) && !IsReadOnly;
            buttonNext.Click += (_, _) => ShiftBy(1);
            _buttonNext = buttonNext;
        }
    }

    /// <inheritdoc/>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        IStswSelectionControl.ItemsSourceChanged(this, newValue);
        base.OnItemsSourceChanged(oldValue, newValue);
        if (SelectedIndex < 0 && Items.Count > 0)
            SelectedIndex = 0;
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

        if (IsKeyboardFocusWithin && !IsReadOnly)
            ShiftBy(e.Delta > 0 ? -1 : 1);

        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (IsKeyboardFocusWithin && !IsReadOnly)
        {
            switch (e.Key)
            {
                case Key.Left:
                case Key.Up:
                    ShiftBy(-1);
                    break;
                case Key.Down:
                case Key.Right:
                    ShiftBy(1);
                    break;
                case Key.PageDown:
                    ShiftBy(-10);
                    break;
                case Key.PageUp:
                    ShiftBy(10);
                    break;
                case Key.Home:
                    if (Items.Count > 0)
                        SelectedIndex = 0;
                    break;
                case Key.End:
                    if (Items.Count > 0)
                        SelectedIndex = Items.Count - 1;
                    break;
                case Key.Tab:
                    (Keyboard.FocusedElement as UIElement)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    break;
            }
        }
        e.Handled = true;
    }

    /// <summary>
    /// Determines whether shifting by a specified step is possible, considering item count and looping settings.
    /// </summary>
    /// <param name="step">The step value for shifting through items.</param>
    /// <returns><see langword="true"/> if shifting is possible; otherwise, <see langword="false"/>.</returns>
    private bool CanShiftBy(int step)
    {
        if (SelectedIndex + step >= Items.Count || SelectedIndex + step < 0)
            return false;
        return true;
    }

    /// <summary>
    /// Changes the selected index by the specified step, considering looping and boundary conditions.
    /// </summary>
    /// <param name="step">The number of positions to shift.</param>
    private void ShiftBy(int step)
    {
        if (Items.Count == 0)
            SelectedIndex = -1;
        else if (IsLoopingEnabled || CanShiftBy(step))
            SelectedIndex = (SelectedIndex + step + Items.Count) % Items.Count;
        else
            SelectedIndex = step > 0 ? Items.Count - 1 : 0;
    }

    /// <summary>
    /// Updates the enabled state of the previous and next buttons based on current selection, looping, and read-only state.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">Event arguments.</param>
    private void CheckButtonAccessibility(object? sender, EventArgs e)
    {
        if (_buttonPrevious != null && _buttonNext != null)
        {
            _buttonPrevious.IsEnabled = (IsLoopingEnabled || CanShiftBy(-1)) && !IsReadOnly;
            _buttonNext.IsEnabled = (IsLoopingEnabled || CanShiftBy(1)) && !IsReadOnly;
        }
    }
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
    public static void OnIsLoopingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswFlipView stsw)
            return;

        stsw.CheckButtonAccessibility(null, EventArgs.Empty);
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
            typeof(StswFlipView)
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
