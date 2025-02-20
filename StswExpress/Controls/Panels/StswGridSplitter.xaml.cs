using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// A resizable splitter control used to adjust the size of adjacent elements in a grid.
/// This control allows users to dynamically resize grid columns or rows by dragging the splitter.
/// </summary>
public class StswGridSplitter : GridSplitter
{
    static StswGridSplitter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswGridSplitter), new FrameworkPropertyMetadata(typeof(StswGridSplitter)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswGridSplitter), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }
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
