using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswDatePicker.xaml
/// </summary>
public partial class StswDatePicker : DatePicker
{
    public StswDatePicker()
    {
        InitializeComponent();
    }

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(double?),
            typeof(StswDatePicker),
            new PropertyMetadata(default(double?))
        );
    public double? CornerRadius
    {
        get => (double?)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        var box = (DatePickerTextBox)GetTemplateChild("PART_TextBox");
        box.ApplyTemplate();

        var watermark = (ContentControl)box.Template.FindName("PART_Watermark", box);
        watermark.Foreground = new SolidColorBrush(Color.FromArgb(191, 127, 127, 127));
        //watermark.Content = "…";

        ((Border)watermark.Parent).BorderThickness = new Thickness(0);
        ((Border)((Grid)((Border)watermark.Parent).Parent).Children[0]).BorderThickness = new Thickness(0);
    }
}
