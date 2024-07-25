using System;
using System.Collections.Generic;
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

        if (AutoLayoutMode != StswAutoLayoutMode.None)
            EnsureDefinitions();
    }

    /// <summary>
    /// Overrides the MeasureOverride method to ensure proper layout when AutoDefinitions is enabled.
    /// </summary>
    /// <param name="constraint">The size constraint for the control.</param>
    /// <returns>The desired size of the control.</returns>
    protected override Size MeasureOverride(Size constraint)
    {
        if (AutoLayoutMode != StswAutoLayoutMode.None)
            EnsureDefinitions();
        return base.MeasureOverride(constraint);
    }

    /// <summary>
    /// Ensures the correct number of RowDefinitions and ColumnDefinitions based on the child elements.
    /// </summary>
    private void EnsureDefinitions()
    {
        ColumnDefinitions.Clear();

        if (AutoLayoutMode == StswAutoLayoutMode.IncrementColumns)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (ColumnDefinitions.Count <= i)
                    ColumnDefinitions.Add(new ColumnDefinition() { Width = ColumnWidths?.Count > 0 ? ColumnWidths[Math.Min(ColumnDefinitions.Count, ColumnWidths.Count - 1)] : GridLength.Auto });
                SetColumn(Children[i], i);
            }
        }
        else if (AutoLayoutMode == StswAutoLayoutMode.IncrementRows)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (RowDefinitions.Count <= i)
                    RowDefinitions.Add(new RowDefinition() { Height = RowHeights?.Count > 0 ? RowHeights[Math.Min(RowDefinitions.Count, RowHeights.Count - 1)] : GridLength.Auto });
                SetRow(Children[i], i);
            }
        }
        else if (AutoLayoutMode == StswAutoLayoutMode.AutoDefinitions)
        {
            int maxRow = 0;
            int maxColumn = 0;

            foreach (UIElement element in Children)
            {
                int row = GetRow(element);
                int column = GetColumn(element);
                maxRow = Math.Max(maxRow, row);
                maxColumn = Math.Max(maxColumn, column);
            }

            while (RowDefinitions.Count <= maxRow)
                RowDefinitions.Add(new RowDefinition() { Height = RowHeights?.Count > 0 ? RowHeights[Math.Min(RowDefinitions.Count, RowHeights.Count - 1)] : GridLength.Auto });

            while (ColumnDefinitions.Count <= maxColumn)
                ColumnDefinitions.Add(new ColumnDefinition() { Width = ColumnWidths?.Count > 0 ? ColumnWidths[Math.Min(ColumnDefinitions.Count, ColumnWidths.Count - 1)] : GridLength.Auto });
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
