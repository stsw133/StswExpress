using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace StswExpress;
/// <summary>
/// A resizable splitter control used to adjust the size of adjacent elements in a grid.
/// This control allows users to dynamically resize grid columns or rows by dragging the splitter.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;Grid&gt;
///     &lt;ColumnDefinition Width="*"/&gt;
///     &lt;ColumnDefinition Width="auto"/&gt;
///     &lt;ColumnDefinition Width="*"/&gt;
/// 
///     &lt;TextBlock Text="Left Pane" Grid.Column="0"/&gt;
///     &lt;se:StswGridSplitter Grid.Column="1" Width="5"/&gt;
///     &lt;TextBlock Text="Right Pane" Grid.Column="2"/&gt;
/// &lt;/Grid&gt;
/// </code>
/// </example>
public class StswGridSplitter : GridSplitter
{
    private Grid? _parentGrid;
    private readonly Dictionary<int, GridLength> _originalColumnLengths = [];
    private readonly Dictionary<int, GridLength> _originalRowLengths = [];
    private bool _isVertical;

    public StswGridSplitter()
    {
        Loaded += OnLoaded;
    }
    static StswGridSplitter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswGridSplitter), new FrameworkPropertyMetadata(typeof(StswGridSplitter)));
    }

    #region Events & methods
    /// <summary>
    /// Handles the Loaded event for the splitter. It retrieves the parent grid and the index of the splitter within the grid.
    /// </summary>
    /// <param name="sender">The sender of the event, typically the splitter itself.</param>
    /// <param name="e">The event arguments containing the routed event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            _parentGrid = Parent as Grid;
            _isVertical = DetermineIsVertical();
            InitOriginalLength();
        }, DispatcherPriority.Loaded);
    }

    /// <inheritdoc/>
    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        base.OnMouseDoubleClick(e);

        if (_parentGrid == null)
            return;

        if (_isVertical)
        {
            foreach (var (index, length) in _originalColumnLengths)
                if (index >= 0 && index < _parentGrid.ColumnDefinitions.Count)
                    _parentGrid.ColumnDefinitions[index].Width = length;
        }
        else
        {
            foreach (var (index, length) in _originalRowLengths)
                if (index >= 0 && index < _parentGrid.RowDefinitions.Count)
                    _parentGrid.RowDefinitions[index].Height = length;
        }
    }

    /// <summary>
    /// Handles the MouseMove event for the splitter. It resizes the adjacent column or row based on the mouse position.
    /// </summary>
    /// <returns>Returns <see langword="true"/> if the resizing was successful, otherwise <see langword="false"/>.</returns>
    private bool DetermineIsVertical()
    {
        if (_parentGrid == null)
            return true;

        var row = Grid.GetRow(this);
        var col = Grid.GetColumn(this);

        if (_parentGrid.RowDefinitions.Count > row)
        {
            var rd = _parentGrid.RowDefinitions[row];
            if (rd.Height.IsAuto || rd.Height.IsAbsolute)
                return false;
        }

        if (_parentGrid.ColumnDefinitions.Count > col)
        {
            var cd = _parentGrid.ColumnDefinitions[col];
            if (cd.Width.IsAuto || cd.Width.IsAbsolute)
                return true;
        }

        return ActualHeight >= ActualWidth;
    }

    /// <summary>
    /// Initializes the original length of the adjacent column or row based on the splitter's position in the grid.
    /// </summary>
    private void InitOriginalLength()
    {
        if (_parentGrid == null)
            return;

        _originalColumnLengths.Clear();
        _originalRowLengths.Clear();
        var resizeBehavior = GetEffectiveResizeBehavior();

        if (_isVertical)
        {
            var col = Grid.GetColumn(this);
            var previousCol = col - 1;
            var nextCol = col + 1;

            switch (resizeBehavior)
            {
                case GridResizeBehavior.CurrentAndNext:
                    CaptureColumnLength(col);
                    CaptureColumnLength(nextCol);
                    break;
                case GridResizeBehavior.PreviousAndCurrent:
                    CaptureColumnLength(previousCol);
                    CaptureColumnLength(col);
                    break;
                default:
                    CaptureColumnLength(previousCol);
                    CaptureColumnLength(nextCol);
                    break;
            }
        }
        else
        {
            var row = Grid.GetRow(this);
            var previousRow = row - 1;
            var nextRow = row + 1;

            switch (resizeBehavior)
            {
                case GridResizeBehavior.CurrentAndNext:
                    CaptureRowLength(row);
                    CaptureRowLength(nextRow);
                    break;
                case GridResizeBehavior.PreviousAndCurrent:
                    CaptureRowLength(previousRow);
                    CaptureRowLength(row);
                    break;
                default:
                    CaptureRowLength(previousRow);
                    CaptureRowLength(nextRow);
                    break;
            }
        }
    }

    /// <summary>
    /// Captures the original length of the specified column in the parent grid.
    /// </summary>
    /// <param name="index">The index of the column to capture.</param>
    private void CaptureColumnLength(int index)
    {
        if (_parentGrid == null || index < 0 || index >= _parentGrid.ColumnDefinitions.Count)
            return;

        _originalColumnLengths[index] = _parentGrid.ColumnDefinitions[index].Width;
    }

    /// <summary>
    /// Captures the original length of the specified row in the parent grid.
    /// </summary>
    /// <param name="index">The index of the row to capture.</param>
    private void CaptureRowLength(int index)
    {
        if (_parentGrid == null || index < 0 || index >= _parentGrid.RowDefinitions.Count)
            return;

        _originalRowLengths[index] = _parentGrid.RowDefinitions[index].Height;
    }

    /// <summary>
    /// Determines the effective resize behavior based on the current settings and alignment.
    /// </summary>
    /// <returns>The effective <see cref="GridResizeBehavior"/>.</returns>
    private GridResizeBehavior GetEffectiveResizeBehavior()
    {
        if (ResizeBehavior != GridResizeBehavior.BasedOnAlignment)
            return ResizeBehavior;

        if (_isVertical)
        {
            return HorizontalAlignment switch
            {
                HorizontalAlignment.Left => GridResizeBehavior.PreviousAndCurrent,
                HorizontalAlignment.Right => GridResizeBehavior.CurrentAndNext,
                _ => GridResizeBehavior.PreviousAndNext,
            };
        }

        return VerticalAlignment switch
        {
            VerticalAlignment.Top => GridResizeBehavior.PreviousAndCurrent,
            VerticalAlignment.Bottom => GridResizeBehavior.CurrentAndNext,
            _ => GridResizeBehavior.PreviousAndNext,
        };
    }
    #endregion
}
