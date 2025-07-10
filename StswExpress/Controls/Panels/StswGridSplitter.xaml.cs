using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace StswExpress;
/// <summary>
/// A resizable splitter control used to adjust the size of adjacent elements in a grid.
/// This control allows users to dynamically resize grid columns or rows by dragging the splitter.
/// </summary>
[StswInfo("0.16.0")]
public class StswGridSplitter : GridSplitter
{
    private Grid? _parentGrid;
    private GridLength? _originalLength;
    private int _targetIndex = -1;
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
    [StswInfo("0.18.0")]
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
    [StswInfo("0.18.0")]
    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        base.OnMouseDoubleClick(e);

        if (_parentGrid == null || _originalLength == null || _targetIndex < 0)
            return;

        if (_isVertical)
        {
            if (_targetIndex < _parentGrid.ColumnDefinitions.Count)
                _parentGrid.ColumnDefinitions[_targetIndex].Width = _originalLength.Value;
        }
        else
        {
            if (_targetIndex < _parentGrid.RowDefinitions.Count)
                _parentGrid.RowDefinitions[_targetIndex].Height = _originalLength.Value;
        }
    }

    /// <summary>
    /// Handles the MouseMove event for the splitter. It resizes the adjacent column or row based on the mouse position.
    /// </summary>
    /// <returns>Returns <see langword="true"/> if the resizing was successful, otherwise <see langword="false"/>.</returns>
    [StswInfo("0.18.0")]
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
    [StswInfo("0.18.0")]
    private void InitOriginalLength()
    {
        if (_parentGrid == null)
            return;

        if (_isVertical)
        {
            var col = Grid.GetColumn(this);
            _targetIndex = col - 1;

            if (_targetIndex >= 0 && _targetIndex < _parentGrid.ColumnDefinitions.Count)
                _originalLength = _parentGrid.ColumnDefinitions[_targetIndex].Width;
        }
        else
        {
            var row = Grid.GetRow(this);
            _targetIndex = row - 1;

            if (_targetIndex >= 0 && _targetIndex < _parentGrid.RowDefinitions.Count)
                _originalLength = _parentGrid.RowDefinitions[_targetIndex].Height;
        }
    }
    #endregion
}

/* usage:

<Grid>
    <ColumnDefinition Width="*"/>
    <ColumnDefinition Width="auto"/>
    <ColumnDefinition Width="*"/>

    <TextBlock Text="Left Pane" Grid.Column="0"/>
    <se:StswGridSplitter Grid.Column="1" Width="5"/>
    <TextBlock Text="Right Pane" Grid.Column="2"/>
</Grid>

*/
