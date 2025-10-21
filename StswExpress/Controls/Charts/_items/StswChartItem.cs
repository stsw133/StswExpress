using System;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a base class for chart items used in various chart controls.
/// </summary>
public abstract class StswChartItem : ContentControl
{
    #region Events & methods
    /// <summary>
    /// Event that is raised when the Value property changes.
    /// </summary>
    public event EventHandler? ValueChanged;
    protected virtual void OnValueChanged() => ValueChanged?.Invoke(this, EventArgs.Empty);
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the description associated with the item.
    /// </summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    public static readonly DependencyProperty DescriptionProperty
        = DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(StswChartItem)
        );

    /// <summary>
    /// Gets or sets the percentage value of the item.
    /// </summary>
    public double Percentage
    {
        get => (double)GetValue(PercentageProperty);
        set => SetValue(PercentageProperty, value);
    }
    public static readonly DependencyProperty PercentageProperty
        = DependencyProperty.Register(
            nameof(Percentage),
            typeof(double),
            typeof(StswChartItem)
        );

    /// <summary>
    /// Gets or sets the numeric value of the item.
    /// </summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public static readonly DependencyProperty TitleProperty
        = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(StswChartItem)
        );

    /// <summary>
    /// Gets or sets the numeric value of the item.
    /// </summary>
    public decimal Value
    {
        get => (decimal)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    public static readonly DependencyProperty ValueProperty
        = DependencyProperty.Register(
            nameof(Value),
            typeof(decimal),
            typeof(StswChartItem),
            new FrameworkPropertyMetadata(default(decimal), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, static (d, _) => ((StswChartItem)d).OnValueChanged())
        );
    #endregion
}
