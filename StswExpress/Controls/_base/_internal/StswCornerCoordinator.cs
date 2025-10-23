using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Coordinates the synchronization between BorderThickness and CornerRadius properties.
/// </summary>
internal static class StswCornerCoordinator
{
    /*
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    internal static void Initialize()
    {
        EventManager.RegisterClassHandler(typeof(FrameworkElement),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler(OnLoaded), true);

        EventManager.RegisterClassHandler(typeof(FrameworkElement),
            FrameworkElement.UnloadedEvent,
            new RoutedEventHandler(OnUnloaded), true);
    }
    */

    /// <summary>
    /// Holds subscription details for a control.
    /// </summary>
    /// <param name="borderDesc">Descriptor for the BorderThickness property.</param>
    /// <param name="radiusDesc">Descriptor for the CornerRadius property.</param>
    /// <param name="borderHandler">Event handler for BorderThickness changes.</param>
    /// <param name="radiusHandler">Event handler for CornerRadius changes.</param>
    private sealed class Subscription(
        DependencyPropertyDescriptor borderDesc,
        DependencyPropertyDescriptor radiusDesc,
        EventHandler borderHandler,
        EventHandler radiusHandler)
    {
        public int ReentrancyGuard;
        public DependencyPropertyDescriptor BorderDesc { get; } = borderDesc;
        public DependencyPropertyDescriptor RadiusDesc { get; } = radiusDesc;
        public EventHandler BorderHandler { get; } = borderHandler;
        public EventHandler RadiusHandler { get; } = radiusHandler;
    }
    private static readonly ConditionalWeakTable<DependencyObject, Subscription> _subs = [];

    /// <summary>
    /// Handles the Loaded event to set up property change subscriptions.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not DependencyObject d || sender is not IStswCornerControl)
            return;

        if (_subs.TryGetValue(d, out _))
            return;

        var borderDesc = DependencyPropertyDescriptor.FromProperty(Control.BorderThicknessProperty, sender.GetType())
            ?? DependencyPropertyDescriptor.FromName(nameof(Control.BorderThickness), sender.GetType(), sender.GetType());
        var radiusDesc = DependencyPropertyDescriptor.FromName(nameof(IStswCornerControl.CornerRadius), sender.GetType(), sender.GetType());

        if (borderDesc is null || radiusDesc is null)
            return;

        EventHandler borderHandler = (_, _) => OnBorderChanged(d);
        EventHandler radiusHandler = (_, _) => OnRadiusChanged(d);

        borderDesc.AddValueChanged(d, borderHandler);
        radiusDesc.AddValueChanged(d, radiusHandler);

        var sub = new Subscription(borderDesc, radiusDesc, borderHandler, radiusHandler);
        _subs.Add(d, sub);

        OnBorderChanged(d);
        OnRadiusChanged(d);
    }

    /// <summary>
    /// Handles the Unloaded event to clean up property change subscriptions.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private static void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (sender is not DependencyObject d) return;
        if (!_subs.TryGetValue(d, out var sub)) return;

        sub.BorderDesc.RemoveValueChanged(d, sub.BorderHandler);
        sub.RadiusDesc.RemoveValueChanged(d, sub.RadiusHandler);
        _subs.Remove(d);
    }

    /// <summary>
    /// Handles changes to the BorderThickness property.
    /// </summary>
    /// <param name="d">The dependency object.</param>
    private static void OnBorderChanged(DependencyObject d)
    {
        if (d is not IStswCornerControl c) return;
        if (!_subs.TryGetValue(d, out var sub)) return;

        if (System.Threading.Interlocked.Exchange(ref sub.ReentrancyGuard, 1) == 1) return;
        try
        {
            var t = c.BorderThickness;
            if (IsZero(t.Left) && IsZero(t.Top) && IsZero(t.Right) && IsZero(t.Bottom))
            {
                var r = c.CornerRadius;
                if (!(IsZero(r.TopLeft) && IsZero(r.TopRight) && IsZero(r.BottomRight) && IsZero(r.BottomLeft)))
                    c.CornerRadius = new CornerRadius(0);
            }
        }
        finally
        {
            System.Threading.Interlocked.Exchange(ref sub.ReentrancyGuard, 0);
        }
    }

    /// <summary>
    /// Handles changes to the CornerRadius property.
    /// </summary>
    /// <param name="d">The dependency object.</param>
    private static void OnRadiusChanged(DependencyObject d)
    {
        if (d is not IStswCornerControl c) return;
        if (!_subs.TryGetValue(d, out var sub)) return;

        if (System.Threading.Interlocked.Exchange(ref sub.ReentrancyGuard, 1) == 1) return;
        try
        {
            var r = c.CornerRadius;
            if (IsZero(r.TopLeft) && IsZero(r.TopRight) && IsZero(r.BottomRight) && IsZero(r.BottomLeft))
            {
                if (c.CornerClipping)
                    c.CornerClipping = false;
            }
        }
        finally
        {
            System.Threading.Interlocked.Exchange(ref sub.ReentrancyGuard, 0);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsZero(double v) => Math.Abs(v) < 0.0001;
}
