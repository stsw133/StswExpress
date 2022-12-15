using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for ExtDatePicker.xaml
/// </summary>
public partial class ExtDatePicker : DatePicker
{
    public ExtDatePicker()
    {
        InitializeComponent();
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
