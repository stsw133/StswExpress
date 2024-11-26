using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// Represents a custom grid control that automatically manages RowDefinitions and ColumnDefinitions based on the child elements.
/// </remarks>
public class StswGrid : Grid
{
    public StswGrid()
    {
        SetValue(ColumnWidthsProperty, new List<GridLength>());
        SetValue(RowHeightsProperty, new List<GridLength>());
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        EnsureDefinitions();
    }

    /// <summary>
    /// Overrides the MeasureOverride method to ensure proper layout when AutoDefinitions is enabled.
    /// </summary>
    /// <param name="constraint">The size constraint for the control.</param>
    /// <returns>The desired size of the control.</returns>
    protected override Size MeasureOverride(Size constraint)
    {
        EnsureDefinitions();
        return base.MeasureOverride(constraint);
    }

    /// <summary>
    /// Ensures the correct number of RowDefinitions and ColumnDefinitions based on the child elements.
    /// </summary>
    private void EnsureDefinitions()
    {
        if (AutoLayoutMode == StswAutoLayoutMode.None)
            return;

        switch (AutoLayoutMode)
        {
            case StswAutoLayoutMode.IncrementColumns:
                {
                    if (ColumnDefinitions.Count != Children.Count)
                    {
                        if (ColumnDefinitions.Count > Children.Count)
                            ColumnDefinitions.RemoveRange(Children.Count, ColumnDefinitions.Count - Children.Count);
                        else
                            while (ColumnDefinitions.Count < Children.Count)
                                ColumnDefinitions.Add(new ColumnDefinition());
                    }

                    for (var i = 0; i < Children.Count; i++)
                    {
                        var desiredWidth = ColumnWidths?.Count > 0
                            ? ColumnWidths.Count > i ? ColumnWidths[i] : ColumnWidths.Last()
                            : GridLength.Auto;

                        if (ColumnDefinitions[i].Width != desiredWidth)
                            ColumnDefinitions[i].Width = desiredWidth;

                        SetColumn(Children[i], i);
                    }

                    break;
                }

            case StswAutoLayoutMode.IncrementRows:
                {
                    if (RowDefinitions.Count != Children.Count)
                    {
                        if (RowDefinitions.Count > Children.Count)
                            RowDefinitions.RemoveRange(Children.Count, RowDefinitions.Count - Children.Count);
                        else
                            while (RowDefinitions.Count < Children.Count)
                                RowDefinitions.Add(new RowDefinition());
                    }

                    for (var i = 0; i < Children.Count; i++)
                    {
                        var desiredHeight = RowHeights?.Count > 0
                            ? RowHeights.Count > i ? RowHeights[i] : RowHeights.Last()
                            : GridLength.Auto;

                        if (RowDefinitions[i].Height != desiredHeight)
                            RowDefinitions[i].Height = desiredHeight;

                        SetRow(Children[i], i);
                    }

                    break;
                }

            case StswAutoLayoutMode.AutoDefinitions:
                {
                    var maxRow = 0;
                    var maxColumn = 0;

                    foreach (UIElement element in Children)
                    {
                        maxRow = Math.Max(maxRow, GetRow(element));
                        maxColumn = Math.Max(maxColumn, GetColumn(element));
                    }

                    if (ColumnDefinitions.Count != maxColumn + 1)
                    {
                        if (ColumnDefinitions.Count > (maxColumn + 1))
                            ColumnDefinitions.RemoveRange(maxColumn + 1, ColumnDefinitions.Count - (maxColumn + 1));
                        else
                            while (ColumnDefinitions.Count < (maxColumn + 1))
                                ColumnDefinitions.Add(new ColumnDefinition());
                    }

                    if (RowDefinitions.Count != maxRow + 1)
                    {
                        if (RowDefinitions.Count > (maxRow + 1))
                            RowDefinitions.RemoveRange(maxRow + 1, RowDefinitions.Count - (maxRow + 1));
                        else
                            while (RowDefinitions.Count < (maxRow + 1))
                                RowDefinitions.Add(new RowDefinition());
                    }

                    for (var i = 0; i < ColumnDefinitions.Count; i++)
                    {
                        var desiredWidth = ColumnWidths?.Count > 0
                            ? ColumnWidths.Count > i ? ColumnWidths[i] : ColumnWidths.Last()
                            : GridLength.Auto;

                        if (ColumnDefinitions[i].Width != desiredWidth)
                            ColumnDefinitions[i].Width = desiredWidth;
                    }

                    for (var i = 0; i < RowDefinitions.Count; i++)
                    {
                        var desiredHeight = RowHeights?.Count > 0
                            ? RowHeights.Count > i ? RowHeights[i] : RowHeights.Last()
                            : GridLength.Auto;

                        if (RowDefinitions[i].Height != desiredHeight)
                            RowDefinitions[i].Height = desiredHeight;
                    }

                    break;
                }
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether RowDefinitions and ColumnDefinitions are automatically managed.
    /// </summary>
    public StswAutoLayoutMode AutoLayoutMode
    {
        get => (StswAutoLayoutMode)GetValue(AutoLayoutModeProperty);
        set => SetValue(AutoLayoutModeProperty, value);
    }
    public static readonly DependencyProperty AutoLayoutModeProperty
        = DependencyProperty.Register(
            nameof(AutoLayoutMode),
            typeof(StswAutoLayoutMode),
            typeof(StswGrid),
            new PropertyMetadata(default(StswAutoLayoutMode), OnAutoLayoutModeChanged)
        );
    public static void OnAutoLayoutModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswGrid stsw)
        {
            stsw.InvalidateMeasure();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether RowDefinitions and ColumnDefinitions are automatically managed.
    /// </summary>
    public List<GridLength> ColumnWidths
    {
        get => (List<GridLength>)GetValue(ColumnWidthsProperty);
        set => SetValue(ColumnWidthsProperty, value);
    }
    public static readonly DependencyProperty ColumnWidthsProperty
        = DependencyProperty.Register(
            nameof(ColumnWidths),
            typeof(List<GridLength>),
            typeof(StswGrid),
            new PropertyMetadata(default(List<GridLength>), OnAutoLayoutModeChanged)
        );

    /// <summary>
    /// Gets or sets a value indicating whether RowDefinitions and ColumnDefinitions are automatically managed.
    /// </summary>
    public List<GridLength> RowHeights
    {
        get => (List<GridLength>)GetValue(RowHeightsProperty);
        set => SetValue(RowHeightsProperty, value);
    }
    public static readonly DependencyProperty RowHeightsProperty
        = DependencyProperty.Register(
            nameof(RowHeights),
            typeof(List<GridLength>),
            typeof(StswGrid),
            new PropertyMetadata(default(List<GridLength>), OnAutoLayoutModeChanged)
        );
    #endregion
}
