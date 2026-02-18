using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace StswExpress.Wpf;

/// <summary>
/// A collapsible side panel that expands on pointer hover and hides when the pointer leaves.
/// Supports always-visible mode.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswSidePanel IsAlwaysVisible="True"&gt;
///     &lt;StackPanel&gt;
///         &lt;Button Content="Option 1"/&gt;
///         &lt;Button Content="Option 2"/&gt;
///     &lt;/StackPanel&gt;
/// &lt;/se:StswSidePanel&gt;
/// </code>
/// </example>
public class StswSidePanel : ContentControl
{
    static StswSidePanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSidePanel), new FrameworkPropertyMetadata(typeof(StswSidePanel)));
    }

	#region Dependency properties
	/// <summary>
	/// Gets or sets a value indicating whether the side panel is always visible.
	/// If <see langword="true"/>, the panel remains expanded and doesn't collapse.
	/// </summary>
	public bool IsAlwaysVisible
    {
        get => (bool)GetValue(IsAlwaysVisibleProperty);
        set => SetValue(IsAlwaysVisibleProperty, value);
    }
    public static readonly DependencyProperty IsAlwaysVisibleProperty
        = DependencyProperty.Register(
            nameof(IsAlwaysVisible),
            typeof(bool),
            typeof(StswSidePanel),
            new PropertyMetadata(default(bool), OnIsAlwaysVisibleChanged)
        );
    public static void OnIsAlwaysVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswSidePanel)d;
        var newIsCollapsed = !stsw.IsAlwaysVisible;
        if (stsw.IsCollapsed != newIsCollapsed)
            stsw.IsCollapsed = newIsCollapsed;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the control is collapsed.
    /// When collapsed, the side panel is hidden until the pointer hovers over it, unless it is in always-visible mode.
    /// </summary>
    internal bool IsCollapsed
    {
        get => (bool)GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }
    internal static readonly DependencyProperty IsCollapsedProperty
        = DependencyProperty.Register(
            nameof(IsCollapsed),
            typeof(bool),
            typeof(StswSidePanel),
            new PropertyMetadata(true, OnIsCollapsedChanged)
        );
    private static void OnIsCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswSidePanel)d;
        stsw.UpdateCollapsedState(true);
    }
    #endregion

    #region Template
    private Border? _expandBorder;
    private ContentPresenter? _contentPresenter;
    private TranslateTransform? _contentTransform;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        /// content presenter
        if (_contentPresenter != null)
            _contentPresenter.SizeChanged -= OnContentPresenterSizeChanged;

        _contentPresenter = GetTemplateChild("OPT_Content") as ContentPresenter;
        if (_contentPresenter != null)
        {
            _contentPresenter.SizeChanged += OnContentPresenterSizeChanged;
            if (_contentPresenter.RenderTransform is not TranslateTransform transform)
            {
                transform = new TranslateTransform();
                _contentPresenter.RenderTransform = transform;
            }
            _contentTransform = transform;
        }

        DetachTemplateEvents();
        _expandBorder = GetTemplateChild("PART_ExpandBorder") as Border;
        AttachTemplateEvents();

        UpdateCollapsedState(false);
    }

    /// <summary>
    /// Attaches event handlers to the template parts.
    /// </summary>
    private void AttachTemplateEvents()
    {
        if (_expandBorder != null)
            _expandBorder.MouseEnter += ExpandBorder_MouseEnter;
    }

    /// <summary>
    /// Detaches event handlers from the template parts.
    /// </summary>
    private void DetachTemplateEvents()
    {
        if (_expandBorder != null)
            _expandBorder.MouseEnter -= ExpandBorder_MouseEnter;
    }
    #endregion

    #region Overrides
    /// <inheritdoc/>
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (!IsAlwaysVisible && !IsCollapsed)
            IsCollapsed = true;
    }
    #endregion

    #region Logic
    /// <summary>
    /// Handles the MouseEnter event on the expand border to expand the panel when hovered.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void ExpandBorder_MouseEnter(object sender, MouseEventArgs e)
    {
        if (!IsAlwaysVisible && IsCollapsed)
            IsCollapsed = false;
    }
    #endregion

    #region Animations
    /// <summary>
    /// Handles size changes of the content presenter to update the collapsed state accordingly.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnContentPresenterSizeChanged(object sender, SizeChangedEventArgs e) => UpdateCollapsedState(false);

    /// <summary>
    /// Updates the visual state of the side panel based on its collapsed state, with optional animation.
    /// </summary>
    /// <param name="animate">Indicates whether to animate the transition.</param>
    private void UpdateCollapsedState(bool animate)
    {
        if (_contentPresenter is null || _contentTransform is null)
            return;

        var direction = GetSlideDirection();

        var canAnimate = animate
            && StswApp.Settings.AnimationsEnabled
            && StswControl.GetEnableAnimations(this);

        if (direction == StswSlideDirection.None)
        {
            _contentPresenter.Visibility = IsCollapsed ? Visibility.Collapsed : Visibility.Visible;
            _contentTransform.X = 0;
            _contentTransform.Y = 0;
            return;
        }

        var collapsedOffset = GetCollapsedOffset(direction);

        if (!canAnimate)
        {
            ApplyCollapsedStateWithoutAnimation(direction, collapsedOffset);
            return;
        }

        AnimateCollapsedState(direction, collapsedOffset);
    }

    /// <summary>
    /// Applies the collapsed state instantly without animation.
    /// </summary>
    /// <param name="direction">The direction in which the panel collapses.</param>
    /// <param name="collapsedOffset">The offset value when collapsed.</param>
    private void ApplyCollapsedStateWithoutAnimation(StswSlideDirection direction, double collapsedOffset)
    {
        if (_contentPresenter is null || _contentTransform is null)
            return;

        _contentTransform.BeginAnimation(TranslateTransform.XProperty, null);
        _contentTransform.BeginAnimation(TranslateTransform.YProperty, null);
        _contentPresenter.Visibility = IsCollapsed ? Visibility.Collapsed : Visibility.Visible;

        if (direction is StswSlideDirection.Left or StswSlideDirection.Right)
        {
            _contentTransform.X = IsCollapsed ? collapsedOffset : 0;
            _contentTransform.Y = 0;
        }
        else
        {
            _contentTransform.X = 0;
            _contentTransform.Y = IsCollapsed ? collapsedOffset : 0;
        }
    }

    /// <summary>
    /// Animates the transition to the collapsed or expanded state.
    /// </summary>
    /// <param name="direction">The direction in which the panel collapses.</param>
    /// <param name="collapsedOffset">The offset value when collapsed.</param>
    private void AnimateCollapsedState(StswSlideDirection direction, double collapsedOffset)
    {
        if (_contentPresenter is null || _contentTransform is null)
            return;

        var targetProperty = direction is StswSlideDirection.Left or StswSlideDirection.Right
            ? TranslateTransform.XProperty
            : TranslateTransform.YProperty;

        var targetValue = IsCollapsed ? collapsedOffset : 0d;

        if (!IsCollapsed)
            _contentPresenter.Visibility = Visibility.Visible;

        var animation = new DoubleAnimation
        {
            To = targetValue,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };

        if (IsCollapsed)
        {
            void OnAnimationCompleted(object? sender, EventArgs args)
            {
                animation.Completed -= OnAnimationCompleted;
                if (_contentPresenter is not null)
                    _contentPresenter.Visibility = Visibility.Collapsed;
            }

            animation.Completed += OnAnimationCompleted;
        }

        _contentTransform.BeginAnimation(targetProperty, animation, HandoffBehavior.SnapshotAndReplace);
    }

    /// <summary>
    /// Determines the slide direction based on the control's alignment properties.
    /// </summary>
    /// <returns>The slide direction.</returns>
    private StswSlideDirection GetSlideDirection()
    {
        return HorizontalAlignment switch
        {
            HorizontalAlignment.Left => StswSlideDirection.Left,
            HorizontalAlignment.Right => StswSlideDirection.Right,
            _ => VerticalAlignment switch
            {
                VerticalAlignment.Top => StswSlideDirection.Top,
                VerticalAlignment.Bottom => StswSlideDirection.Bottom,
                _ => StswSlideDirection.None,
            },
        };
    }

    /// <summary>
    /// Calculates the offset value for the collapsed state based on the slide direction.
    /// </summary>
    /// <param name="direction">The direction in which the panel collapses.</param>
    /// <returns>The offset value for the collapsed state.</returns>
    private double GetCollapsedOffset(StswSlideDirection direction)
    {
        if (_contentPresenter is null)
            return 0;

        return direction switch
        {
            StswSlideDirection.Left => -_contentPresenter.ActualWidth,
            StswSlideDirection.Right => _contentPresenter.ActualWidth,
            StswSlideDirection.Top => -_contentPresenter.ActualHeight,
            StswSlideDirection.Bottom => _contentPresenter.ActualHeight,
            _ => 0,
        };
    }
    #endregion
}
