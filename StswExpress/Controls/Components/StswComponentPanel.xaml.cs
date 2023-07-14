using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

[ContentProperty(nameof(Items))]
public class StswComponentPanel : Expander, IStswComponent
{
    public StswComponentPanel()
    {
        SetValue(ItemsProperty, new ObservableCollection<IStswComponent>());
    }
    static StswComponentPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswComponentPanel), new FrameworkPropertyMetadata(typeof(StswComponentPanel)));
    }

    #region Events
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        /// Component: header
        if (GetTemplateChild("PART_Header") is StswComponentCheck cmpHeader)
            cmpHeader.MouseEnter += PART_Header_MouseEnter;

        base.OnApplyTemplate();
    }

    /// PART_Header_MouseEnter
    private void PART_Header_MouseEnter(object sender, MouseEventArgs e)
    {
        IsExpanded = true;
        e.Handled = true;
    }

    /// OnMouseLeave
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        IsExpanded = false;
        base.OnMouseLeave(e);
    }
    #endregion

    #region Main properties
    /// IconScale
    public static readonly DependencyProperty IconScaleProperty
        = DependencyProperty.Register(
            nameof(IconScale),
            typeof(GridLength?),
            typeof(StswComponentPanel)
        );
    public GridLength? IconScale
    {
        get => (GridLength?)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }

    /// Items
    public static readonly DependencyProperty ItemsProperty
        = DependencyProperty.Register(
            nameof(Items),
            typeof(ObservableCollection<IStswComponent>),
            typeof(StswComponentPanel)
        );
    public ObservableCollection<IStswComponent>? Items
    {
        get => (ObservableCollection<IStswComponent>?)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }
    #endregion

    #region Spatial properties
    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Thickness? BorderThickness { get; private set; }

    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Thickness? Padding { get; private set; }
    #endregion

    #region Style properties
    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Brush? BorderBrush { get; private set; }
    #endregion
}
