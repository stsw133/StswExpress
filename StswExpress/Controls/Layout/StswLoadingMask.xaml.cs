using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Windows.Shapes;

namespace StswExpress;

/// <summary>
/// Represents a ...
/// </summary>
public class StswLoadingMask : ContentControl
{
    public StswLoadingMask()
    {
        SetValue(MaskProperty, new ObservableCollection<Shape>());
    }
    static StswLoadingMask()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswLoadingMask), new FrameworkPropertyMetadata(typeof(StswLoadingMask)));
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets a ...
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
            typeof(StswLoadingMask)
        );

    /// <summary>
    /// Gets or sets a ...
    /// </summary>
    public ObservableCollection<Shape> Mask
    {
        get => (ObservableCollection<Shape>)GetValue(MaskProperty);
        set => SetValue(MaskProperty, value);
    }
    public static readonly DependencyProperty MaskProperty
        = DependencyProperty.Register(
            nameof(Mask),
            typeof(ObservableCollection<Shape>),
            typeof(StswLoadingMask),
            new FrameworkPropertyMetadata(default(ObservableCollection<Shape>),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnMaskChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnMaskChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswLoadingMask stsw)
        {
            
        }
    }
    #endregion
}
