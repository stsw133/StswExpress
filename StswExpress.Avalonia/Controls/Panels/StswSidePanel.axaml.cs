using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace StswExpress.Avalonia;

/// <summary>
/// A collapsible side panel that expands on pointer hover and hides when the pointer leaves.
/// Supports always-visible mode.
/// </summary>
public class StswSidePanel : ContentControl
{
	private Border? _expandBorder;
	private ContentPresenter? _contentPresenter;
	private TranslateTransform? _contentTransform;
	private int _collapseVersion;

	static StswSidePanel()
	{
		IsAlwaysVisibleProperty.Changed.AddClassHandler<StswSidePanel>((s, _) => s.OnIsAlwaysVisibleChanged());
		IsCollapsedProperty.Changed.AddClassHandler<StswSidePanel>((s, _) => s.UpdateCollapsedState(true));
	}

	#region Dependency properties
	/// <summary>
	/// Gets or sets a value indicating whether the side panel is always visible.
	/// If <see langword="true"/>, the panel remains expanded and doesn't collapse.
	/// </summary>
	public bool IsAlwaysVisible
	{
		get => GetValue(IsAlwaysVisibleProperty);
		set => SetValue(IsAlwaysVisibleProperty, value);
	}
	public static readonly StyledProperty<bool> IsAlwaysVisibleProperty = AvaloniaProperty.Register<StswSidePanel, bool>(nameof(IsAlwaysVisible));

	/// <summary>
	/// Gets or sets a value indicating whether the panel content is collapsed.
	/// When collapsed, the side panel is hidden until the pointer hovers over it, unless it is in always-visible mode.
	/// </summary>
	internal bool IsCollapsed
	{
		get => GetValue(IsCollapsedProperty);
		set => SetValue(IsCollapsedProperty, value);
	}
	internal static readonly StyledProperty<bool> IsCollapsedProperty = AvaloniaProperty.Register<StswSidePanel, bool>(nameof(IsCollapsed), true);
	private void OnIsAlwaysVisibleChanged()
	{
		var newCollapsed = !IsAlwaysVisible;
		if (IsCollapsed != newCollapsed)
			IsCollapsed = newCollapsed;
	}
	#endregion

	#region Template
	/// <inheritdoc/>
	protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
	{
		base.OnApplyTemplate(e);

		if (_contentPresenter != null)
			_contentPresenter.SizeChanged -= OnContentPresenterSizeChanged;

		if (_expandBorder != null)
			_expandBorder.PointerEntered -= ExpandBorder_PointerEntered;

		_expandBorder = e.NameScope.Find<Border>("PART_ExpandBorder");
		_contentPresenter = e.NameScope.Find<ContentPresenter>("OPT_Content");

		if (_expandBorder != null)
			_expandBorder.PointerEntered += ExpandBorder_PointerEntered;

		if (_contentPresenter != null)
		{
			_contentPresenter.SizeChanged += OnContentPresenterSizeChanged;
			_contentTransform = _contentPresenter.RenderTransform as TranslateTransform ?? new TranslateTransform();
			_contentPresenter.RenderTransform = _contentTransform;
		}

		UpdateCollapsedState(false);
	}
	#endregion

	#region Overrides
	/// <inheritdoc/>
	protected override void OnPointerExited(PointerEventArgs e)
	{
		base.OnPointerExited(e);

		if (!IsAlwaysVisible && !IsCollapsed)
			IsCollapsed = true;
	}
	#endregion

	#region Logic
	/// <summary>
	/// Handles the pointer entered event to expand the border if it is currently collapsed and not set to always be visible.
	/// </summary>
	/// <remarks>This method sets the border to visible when the pointer enters its area, provided the border is not configured to always be visible.</remarks>
	/// <param name="sender">The source of the event, typically the UI element that triggered the pointer entered event.</param>
	/// <param name="e">The event data associated with the pointer entered event.</param>
	private void ExpandBorder_PointerEntered(object? sender, PointerEventArgs e)
	{
		if (!IsAlwaysVisible && IsCollapsed)
			IsCollapsed = false;
	}

	/// <summary>
	/// Handles the SizeChanged event of the content presenter to update the collapsed state when the size changes, ensuring that the panel collapses or expands correctly based on its new dimensions.
	/// </summary>
	/// <param name="sender">The source of the event, which is the content presenter whose size has changed.</param>
	/// <param name="e">The <see cref="SizeChangedEventArgs"/> containing information about the size change event, including the new and previous sizes of the content presenter.</param>
	private void OnContentPresenterSizeChanged(object? sender, SizeChangedEventArgs e) => UpdateCollapsedState(false);

	/// <summary>
	/// Updates the collapsed or expanded state of the content presenter, optionally animating the transition based on the specified parameter.
	/// </summary>
	/// <remarks>If the slide direction is <see cref="StswSlideDirection.None"/>, the content presenter will simply be shown or hidden without any translation or animation. For other slide directions, the content presenter will be translated off-screen when collapsed and returned to its original position when expanded, with optional animation based on application settings.</remarks>
	/// <param name="animate"><see langword="true"/> to animate the transition between collapsed and expanded states if animations are enabled in application settings; otherwise, <see langword="false"/> to update the state instantly without animation.</param>"
	private void UpdateCollapsedState(bool animate)
	{
		if (_contentPresenter is null || _contentTransform is null)
			return;

		var direction = GetSlideDirection();
		var canAnimate = animate && StswApp.Settings.AnimationsEnabled;

		if (direction == StswSlideDirection.None)
		{
			_contentPresenter.IsVisible = !IsCollapsed;
			_contentTransform.X = 0;
			_contentTransform.Y = 0;
			_contentTransform.Transitions = null;
			return;
		}

		var offset = GetCollapsedOffset(direction);
		ApplyState(direction, offset, canAnimate);
	}

	/// <summary>
	/// Applies the specified slide direction and offset to the content presenter, optionally animating the transition between collapsed and expanded states.
	/// </summary>
	/// <remarks>If the slide direction is <see cref="StswSlideDirection.None"/>, the content presenter will simply be shown or hidden without any translation or animation.</remarks>
	/// <param name="direction">The direction in which the content should slide, specified by the <see cref="StswSlideDirection"/> enum.
	/// <param name="collapsedOffset">The offset value to apply when the content is collapsed, determining the final position of the content presenter.</param>
	/// <param name="animate">A value indicating whether to animate the transition between collapsed and expanded states. If <see langword="true"/>, the content presenter will smoothly slide to the target position; if <see langword="false"/>, it will instantly jump to the target position.</param>
	private void ApplyState(StswSlideDirection direction, double collapsedOffset, bool animate)
	{
		if (_contentPresenter is null || _contentTransform is null)
			return;

		var useX = direction is StswSlideDirection.Left or StswSlideDirection.Right;
		var target = IsCollapsed ? collapsedOffset : 0d;

		_collapseVersion++;
		var version = _collapseVersion;

		if (animate)
		{
			_contentTransform.Transitions =
			[
				new DoubleTransition
				{
					Property = useX ? TranslateTransform.XProperty : TranslateTransform.YProperty,
					Duration = TimeSpan.FromMilliseconds(200),
					Easing = new CubicEaseOut(),
				}
			];
		}
		else
		{
			_contentTransform.Transitions = null;
		}

		if (!IsCollapsed)
			_contentPresenter.IsVisible = true;

		if (useX)
		{
			_contentTransform.X = target;
			_contentTransform.Y = 0;
		}
		else
		{
			_contentTransform.X = 0;
			_contentTransform.Y = target;
		}

		if (!IsCollapsed)
			return;

		if (!animate)
		{
			_contentPresenter.IsVisible = false;
			return;
		}

		DispatcherTimer.RunOnce(() =>
		{
			if (version == _collapseVersion && IsCollapsed && _contentPresenter != null)
				_contentPresenter.IsVisible = false;
		}, TimeSpan.FromMilliseconds(200));
	}

	/// <summary>
	/// Determines the slide direction based on the panel's horizontal and vertical alignment.
	/// </summary>
	/// <returns>The <see cref="StswSlideDirection"/> representing the direction in which the content should slide to collapse.</returns>
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
	/// Calculates the offset required to collapse the content in the specified slide direction.
	/// </summary>
	/// <param name="direction">The direction in which the content should slide to collapse.</param>
	/// <returns>A double value representing the offset to apply for collapsing the content.</returns>
	private double GetCollapsedOffset(StswSlideDirection direction)
	{
		if (_contentPresenter is null)
			return 0;

		return direction switch
		{
			StswSlideDirection.Left => -_contentPresenter.Bounds.Width,
			StswSlideDirection.Right => _contentPresenter.Bounds.Width,
			StswSlideDirection.Top => -_contentPresenter.Bounds.Height,
			StswSlideDirection.Bottom => _contentPresenter.Bounds.Height,
			_ => 0,
		};
	}
	#endregion
}
