using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a navigation element that can contain sub-elements and interact with a parent navigation control.
/// </summary>
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

    #region Events and methods
    private StswNavigation? stswNavi;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        /// StswNavigation
        if (StswFn.FindVisualAncestor<StswNavigation>(this) is StswNavigation stswNavigation)
            stswNavi = stswNavigation;
        OnIsCheckedChanged(this, new DependencyPropertyChangedEventArgs());

        CheckSubItemPadding();

        base.OnApplyTemplate();
    }

    /// <summary>
    /// Checks and updates the sub-item padding based on the ancestors and compact state.
    /// </summary>
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

    /// <summary>
    /// Finds the <see cref="Popup"/> container of the navigation element if it exists.
    /// </summary>
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
    /// <summary>
    /// Gets or sets the <see cref="UIElement"/> that serves as the container for this navigation element.
    /// </summary>
    public UIElement Container
    {
        get => (UIElement)GetValue(ContainerProperty);
        internal set => SetValue(ContainerProperty, value);
    }
    public static readonly DependencyProperty ContainerProperty
        = DependencyProperty.Register(
            nameof(Container),
            typeof(UIElement),
            typeof(StswNavigationElement)
        );

    /// <summary>
    /// Gets or sets the namespace of the context associated with this navigation element.
    /// </summary>
    public string ContextNamespace
    {
        get => (string)GetValue(ContextNamespaceProperty);
        set => SetValue(ContextNamespaceProperty, value);
    }
    public static readonly DependencyProperty ContextNamespaceProperty
        = DependencyProperty.Register(
            nameof(ContextNamespace),
            typeof(string),
            typeof(StswNavigationElement)
        );

    /// <summary>
    /// Gets or sets a value indicating whether to create a new instance of the context object when the element is checked.
    /// </summary>
    public bool CreateNewInstance
    {
        get => (bool)GetValue(CreateNewInstanceProperty);
        set => SetValue(CreateNewInstanceProperty, value);
    }
    public static readonly DependencyProperty CreateNewInstanceProperty
        = DependencyProperty.Register(
            nameof(CreateNewInstance),
            typeof(bool),
            typeof(StswNavigationElement)
        );

    /// <summary>
    /// Gets or sets the geometry used for the icon.
    /// </summary>
    public Geometry? IconData
    {
        get => (Geometry?)GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }
    public static readonly DependencyProperty IconDataProperty
        = DependencyProperty.Register(
            nameof(IconData),
            typeof(Geometry),
            typeof(StswNavigationElement)
        );

    /// <summary>
    /// Gets or sets the scale of the icon.
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
            typeof(StswNavigationElement)
        );

    /// <summary>
    /// Gets or sets the source used for the icon image.
    /// </summary>
    public ImageSource? IconSource
    {
        get => (ImageSource?)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }
    public static readonly DependencyProperty IconSourceProperty
        = DependencyProperty.Register(
            nameof(IconSource),
            typeof(ImageSource),
            typeof(StswNavigationElement)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the navigation element is in the busy state.
    /// </summary>
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }
    public static readonly DependencyProperty IsBusyProperty
        = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool),
            typeof(StswNavigationElement)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the navigation element is checked.
    /// </summary>
    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }
    public static readonly DependencyProperty IsCheckedProperty
        = DependencyProperty.Register(
            nameof(IsChecked),
            typeof(bool),
            typeof(StswNavigationElement),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsCheckedChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
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

    /// <summary>
    /// Gets or sets a value indicating whether the navigation element is in the compact state.
    /// </summary>
    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        internal set => SetValue(IsCompactProperty, value);
    }
    public static readonly DependencyProperty IsCompactProperty
        = DependencyProperty.Register(
            nameof(IsCompact),
            typeof(bool),
            typeof(StswNavigationElement),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsCompactChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
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

    /// <summary>
    /// Gets or sets the collection of sub-elements.
    /// </summary>
    public ObservableCollection<StswNavigationElement> Items
    {
        get => (ObservableCollection<StswNavigationElement>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }
    public static readonly DependencyProperty ItemsProperty
        = DependencyProperty.Register(
            nameof(Items),
            typeof(ObservableCollection<StswNavigationElement>),
            typeof(StswNavigationElement)
        );

    /// <summary>
    /// Gets or sets the text content of the navigation element.
    /// </summary>
    public object? Text
    {
        get => (object?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
            nameof(Text),
            typeof(object),
            typeof(StswNavigationElement)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the degree to which the corners of the control are rounded.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswNavigationElement)
        );

    /// <summary>
    /// Gets or sets the thickness of the border for the popup.
    /// </summary>
    public Thickness PopupBorderThickness
    {
        get => (Thickness)GetValue(PopupBorderThicknessProperty);
        set => SetValue(PopupBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty PopupBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(PopupBorderThickness),
            typeof(Thickness),
            typeof(StswNavigationElement)
        );

    /// <summary>
    /// Gets or sets the thickness of the sub-item border.
    /// </summary>
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswNavigationElement)
        );

    /// <summary>
    /// Gets or sets the indentation value for sub-items.
    /// </summary>
    public double SubItemIndentation
    {
        get => (double)GetValue(SubItemIndentationProperty);
        set => SetValue(SubItemIndentationProperty, value);
    }
    public static readonly DependencyProperty SubItemIndentationProperty
        = DependencyProperty.Register(
            nameof(SubItemIndentation),
            typeof(double),
            typeof(StswNavigationElement)
        );

    /// <summary>
    /// Gets or sets the padding for the sub-item.
    /// </summary>
    internal Thickness SubItemPadding
    {
        get => (Thickness)GetValue(SubItemPaddingProperty);
        set => SetValue(SubItemPaddingProperty, value);
    }
    public static readonly DependencyProperty SubItemPaddingProperty
        = DependencyProperty.Register(
            nameof(SubItemPadding),
            typeof(Thickness),
            typeof(StswNavigationElement)
        );
    #endregion
}
