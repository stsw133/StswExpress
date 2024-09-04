using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// Represents a control that functions as a label container with configurable layout and label styling options.
/// </summary>
public class StswLabelPanel : Grid
{
    #region Events & methods
    /// <summary>
    /// Called when the visual children of this element change.
    /// This method updates the layout of the panel based on its current orientation and label properties.
    /// </summary>
    /// <param name="visualAdded">The child element added.</param>
    /// <param name="visualRemoved">The child element removed.</param>
    protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
    {
        base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        RefreshLayout();
    }

    /// <summary>
    /// Updates the layout of the panel by arranging the child elements according to the current orientation and label width.
    /// </summary>
    private void RefreshLayout()
    {
        RowDefinitions.Clear();
        ColumnDefinitions.Clear();

        if (Orientation == Orientation.Vertical)
        {
            for (var i = 0; i < Children.Count; i++)
            {
                if (i >= RowDefinitions.Count)
                    RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                
                SetRow(Children[i], i);
                SetColumn(Children[i], 0);
            }
        }
        else
        {
            if (ColumnDefinitions.Count == 0)
            {
                ColumnDefinitions.Add(new ColumnDefinition { Width = LabelWidth });
                ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            for (var i = 0; i < Children.Count; i++)
            {
                var row = i / 2;
                var column = i % 2;

                if (row >= RowDefinitions.Count)
                    RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                SetRow(Children[i], row);
                SetColumn(Children[i], column);
            }
        }

        UpdateLabelProperties();
    }

    /// <summary>
    /// Updates the properties of the Label elements within the panel, applying the configured font weight and horizontal alignment.
    /// </summary>
    private void UpdateLabelProperties()
    {
        for (int i = 0; i < Children.Count; ++i)
        {
            if ((i % 2) == 0 && Children[i] is Control control)
            {
                control.HorizontalContentAlignment = LabelHorizontalAlignment;
                control.FontWeight = LabelFontWeight;
            }
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the orientation of the panel, which determines the layout of the child elements.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StswLabelPanel),
            new FrameworkPropertyMetadata(default(Orientation), FrameworkPropertyMetadataOptions.AffectsArrange, OnOrientationChanged)
        );
    private static void OnOrientationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswLabelPanel stsw)
        {
            stsw.RefreshLayout();
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the font weight of the <see cref="Label"/> elements within the panel.
    /// </summary>
    public FontWeight LabelFontWeight
    {
        get => (FontWeight)GetValue(LabelFontWeightProperty);
        set => SetValue(LabelFontWeightProperty, value);
    }
    public static readonly DependencyProperty LabelFontWeightProperty
        = DependencyProperty.Register(
            nameof(LabelFontWeight),
            typeof(FontWeight),
            typeof(StswLabelPanel),
            new FrameworkPropertyMetadata(default(FontWeight), FrameworkPropertyMetadataOptions.AffectsMeasure, OnLabelFontWeightChanged)
        );
    private static void OnLabelFontWeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswLabelPanel stsw)
        {
            stsw.UpdateLabelProperties();
        }
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of the <see cref="Label"/> elements within the panel.
    /// </summary>
    public HorizontalAlignment LabelHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(LabelHorizontalAlignmentProperty);
        set => SetValue(LabelHorizontalAlignmentProperty, value);
    }
    public static readonly DependencyProperty LabelHorizontalAlignmentProperty
        = DependencyProperty.Register(
            nameof(LabelHorizontalAlignment),
            typeof(HorizontalAlignment),
            typeof(StswLabelPanel),
            new FrameworkPropertyMetadata(default(HorizontalAlignment), FrameworkPropertyMetadataOptions.AffectsArrange, OnLabelHorizontalAlignmentChanged)
        );
    private static void OnLabelHorizontalAlignmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswLabelPanel stsw)
        {
            stsw.UpdateLabelProperties();
        }
    }

    /// <summary>
    /// Gets or sets the width of the first column in the panel when in horizontal orientation.
    /// </summary>
    public GridLength LabelWidth
    {
        get => (GridLength)GetValue(LabelWidthProperty);
        set => SetValue(LabelWidthProperty, value);
    }
    public static readonly DependencyProperty LabelWidthProperty
        = DependencyProperty.Register(
            nameof(LabelWidth),
            typeof(GridLength),
            typeof(StswLabelPanel),
            new FrameworkPropertyMetadata(default(GridLength), FrameworkPropertyMetadataOptions.AffectsMeasure, OnLabelWidthChanged)
        );
    private static void OnLabelWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswLabelPanel stsw)
        {
            stsw.RefreshLayout();
        }
    }
    #endregion
}
