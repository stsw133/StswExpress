using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace StswExpress;
/// <summary>
/// A resizable splitter control used to adjust the size of adjacent elements in a grid.
/// This control allows users to dynamically resize grid columns or rows by dragging the splitter.
/// </summary>
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
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswGridSplitter), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <summary>
    /// Handles the Loaded event for the splitter. It retrieves the parent grid and the index of the splitter within the grid.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            _parentGrid = Parent as Grid;
            _isVertical = DetermineIsVertical();
            InitOriginalLength();
        }, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Handles the MouseLeftButtonDown event for the splitter. It captures the mouse and sets the original length of the adjacent column or row.
    /// </summary>
    /// <param name="e"></param>
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
    /// <returns></returns>
    private bool DetermineIsVertical()
    {
        if (_parentGrid == null)
            return true;

        int row = Grid.GetRow(this);
        int col = Grid.GetColumn(this);

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
