using System;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a custom grid control that automatically manages RowDefinitions and ColumnDefinitions based on the child elements.
/// </remarks>
public class StswGrid : Grid
{
    #region Events & methods
    /// <summary>
    /// Overrides the MeasureOverride method to ensure proper layout when AutoDefinitions is enabled.
    /// </summary>
    /// <param name="constraint">The size constraint for the control.</param>
    /// <returns>The desired size of the control.</returns>
    protected override Size MeasureOverride(Size constraint)
    {
        if (AutoDefinitions)
            EnsureDefinitions();
        return base.MeasureOverride(constraint);
    }

    /// <summary>
    /// Ensures the correct number of RowDefinitions and ColumnDefinitions based on the child elements.
    /// </summary>
    private void EnsureDefinitions()
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
            RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

        while (ColumnDefinitions.Count <= maxColumn)
            ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether RowDefinitions and ColumnDefinitions are automatically managed.
    /// </summary>
    public bool AutoDefinitions
    {
        get => (bool)GetValue(AutoDefinitionsProperty);
        set => SetValue(AutoDefinitionsProperty, value);
    }
    public static readonly DependencyProperty AutoDefinitionsProperty
        = DependencyProperty.Register(
            nameof(AutoDefinitions),
            typeof(bool),
            typeof(StswGrid)
        );
    #endregion
}
