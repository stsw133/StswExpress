using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a panel control that functions as a component and displays an icon. 
/// It can expand to show additional components when the mouse is over an icon.
/// </summary>
[ContentProperty(nameof(Items))]
public class StswComponentPanel : Expander
{
    public StswComponentPanel()
    {
        SetValue(ItemsProperty, new ObservableCollection<IStswComponent>());
    }
    static StswComponentPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswComponentPanel), new FrameworkPropertyMetadata(typeof(StswComponentPanel)));
    }

    #region Main properties
    /// <summary>
    /// Gets or sets the scale of the arrow icon.
    /// </summary>
    public GridLength? IconScale
    {
        get => (GridLength?)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }
    public static readonly DependencyProperty IconScaleProperty
        = DependencyProperty.Register(
            nameof(IconScale),
            typeof(GridLength?),
            typeof(StswComponentPanel)
        );

    /// <summary>
    /// Gets or sets the collection of components displayed in the expand portion of the panel.
    /// </summary>
    public ObservableCollection<IStswComponent>? Items
    {
        get => (ObservableCollection<IStswComponent>?)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }
    public static readonly DependencyProperty ItemsProperty
        = DependencyProperty.Register(
            nameof(Items),
            typeof(ObservableCollection<IStswComponent>),
            typeof(StswComponentPanel)
        );
    #endregion

    #region Style properties
    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Brush? BorderBrush { get; private set; }

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
}
