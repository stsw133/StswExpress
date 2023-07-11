using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

[ContentProperty(nameof(Items))]
public class StswNavigationElement : UserControl
{
    public StswNavigationElement()
    {
        SetValue(ItemsProperty, new ObservableCollection<StswNavigationElement>());
    }
    static StswNavigationElement()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigationElement), new FrameworkPropertyMetadata(typeof(StswNavigationElement)));
    }

    #region Events
    private StswNavigation? stswNavi;

    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        /// StswNavigation
        if (StswFn.FindVisualAncestor<StswNavigation>(this) is StswNavigation stswNavigation)
            stswNavi = stswNavigation;
        OnIsCheckedChanged(this, new DependencyPropertyChangedEventArgs());

        CheckSubItemPadding();

        base.OnApplyTemplate();
    }

    /// CheckSubItemPadding
    public void CheckSubItemPadding()
    {
        var padding = Padding;
        var ancestorElement = this;

        while (ancestorElement != null)
        {
            ancestorElement = StswFn.FindVisualAncestor<StswNavigationElement>(ancestorElement);
            if (ancestorElement != null && ancestorElement.Items.Count > 0 && !ancestorElement.IsCompact && ancestorElement.ContextNamespace == null)
                padding = new Thickness(padding.Left + ancestorElement.SubItemIndentation, padding.Top, padding.Right, padding.Bottom);
        }
        SubItemPadding = padding;
    }

    /// FindPopupContainer
    public Popup? FindPopupContainer()
    {
        var ancestorElement = this;
        while (ancestorElement != null)
        {
            if (ancestorElement != null && ancestorElement.Container is Popup popup)
                return popup;
            ancestorElement = StswFn.FindVisualAncestor<StswNavigationElement>(ancestorElement);
        }
        return null;
    }
    #endregion

    #region Main properties
    /// Container
    public static readonly DependencyProperty ContainerProperty
        = DependencyProperty.Register(
            nameof(Container),
            typeof(UIElement),
            typeof(StswNavigationElement)
        );
    public UIElement Container
    {
        get => (UIElement)GetValue(ContainerProperty);
        internal set => SetValue(ContainerProperty, value);
    }

    /// ContextNamespace
    public static readonly DependencyProperty ContextNamespaceProperty
        = DependencyProperty.Register(
            nameof(ContextNamespace),
            typeof(string),
            typeof(StswNavigationElement)
        );
    public string ContextNamespace
    {
        get => (string)GetValue(ContextNamespaceProperty);
        set => SetValue(ContextNamespaceProperty, value);
    }

    /// CreateNewInstance
    public static readonly DependencyProperty CreateNewInstanceProperty
        = DependencyProperty.Register(
            nameof(CreateNewInstance),
            typeof(bool),
            typeof(StswNavigationElement)
        );
    public bool CreateNewInstance
    {
        get => (bool)GetValue(CreateNewInstanceProperty);
        set => SetValue(CreateNewInstanceProperty, value);
    }

    /// IconData
    public static readonly DependencyProperty IconDataProperty
        = DependencyProperty.Register(
            nameof(IconData),
            typeof(Geometry),
            typeof(StswNavigationElement)
        );
    public Geometry? IconData
    {
        get => (Geometry?)GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }
    /// IconScale
    public static readonly DependencyProperty IconScaleProperty
        = DependencyProperty.Register(
            nameof(IconScale),
            typeof(GridLength?),
            typeof(StswNavigationElement)
        );
    public GridLength? IconScale
    {
        get => (GridLength?)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }
    /// IconSource
    public static readonly DependencyProperty IconSourceProperty
        = DependencyProperty.Register(
            nameof(IconSource),
            typeof(ImageSource),
            typeof(StswNavigationElement)
        );
    public ImageSource? IconSource
    {
        get => (ImageSource?)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// IsBusy
    public static readonly DependencyProperty IsBusyProperty
        = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool),
            typeof(StswNavigationElement)
        );
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    /// IsChecked
    public static readonly DependencyProperty IsCheckedProperty
        = DependencyProperty.Register(
            nameof(IsChecked),
            typeof(bool),
            typeof(StswNavigationElement),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsCheckedChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }
    public static void OnIsCheckedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNavigationElement stsw)
        {
            if (stsw.IsChecked && stsw.stswNavi != null && stsw.ContextNamespace != null)
            {
                if (!stsw.stswNavi.IsExtended && stsw.FindPopupContainer() is Popup popup)
                    popup.IsOpen = false;

                stsw.IsBusy = true;
                stsw.stswNavi.ChangeContext(stsw.ContextNamespace, stsw.CreateNewInstance);
                stsw.IsBusy = false;
            }
        }
    }

    /// IsCompact
    public static readonly DependencyProperty IsCompactProperty
        = DependencyProperty.Register(
            nameof(IsCompact),
            typeof(bool),
            typeof(StswNavigationElement),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsCompactChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        internal set => SetValue(IsCompactProperty, value);
    }
    public static void OnIsCompactChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNavigationElement stsw)
        {
            if (stsw.IsCompact && stsw.Items.Count > 0)
                stsw.IsChecked = false;

            foreach (var stswNE in StswFn.FindVisualChildren<StswNavigationElement>(stsw.Container))
                stswNE.CheckSubItemPadding();
        }
    }

    /// Items
    public static readonly DependencyProperty ItemsProperty
        = DependencyProperty.Register(
            nameof(Items),
            typeof(ObservableCollection<StswNavigationElement>),
            typeof(StswNavigationElement)
        );
    public ObservableCollection<StswNavigationElement> Items
    {
        get => (ObservableCollection<StswNavigationElement>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    /// Text
    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
            nameof(Text),
            typeof(object),
            typeof(StswNavigationElement)
        );
    public object? Text
    {
        get => (object?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    #endregion

    #region Spatial properties
    /// > BorderThickness ...
    /// PopupBorderThickness
    public static readonly DependencyProperty PopupBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(PopupBorderThickness),
            typeof(Thickness),
            typeof(StswNavigationElement)
        );
    public Thickness PopupBorderThickness
    {
        get => (Thickness)GetValue(PopupBorderThicknessProperty);
        set => SetValue(PopupBorderThicknessProperty, value);
    }
    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswNavigationElement)
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }

    /// > CornerRadius ...
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswNavigationElement)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// > Padding ...
    /// SubItemIndentation
    public static readonly DependencyProperty SubItemIndentationProperty
        = DependencyProperty.Register(
            nameof(SubItemIndentation),
            typeof(double),
            typeof(StswNavigationElement)
        );
    public double SubItemIndentation
    {
        get => (double)GetValue(SubItemIndentationProperty);
        set => SetValue(SubItemIndentationProperty, value);
    }
    /// SubItemPadding
    public static readonly DependencyProperty SubItemPaddingProperty
        = DependencyProperty.Register(
            nameof(SubItemPadding),
            typeof(Thickness),
            typeof(StswNavigationElement)
        );
    internal Thickness SubItemPadding
    {
        get => (Thickness)GetValue(SubItemPaddingProperty);
        set => SetValue(SubItemPaddingProperty, value);
    }
    #endregion
}
