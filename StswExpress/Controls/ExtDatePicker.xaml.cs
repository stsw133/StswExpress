using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for ExtDatePicker.xaml
    /// </summary>
    public partial class ExtDatePicker : DatePicker
    {
        public ExtDatePicker()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var box = base.GetTemplateChild("PART_TextBox") as DatePickerTextBox;
            box.ApplyTemplate();

            var watermark = box.Template.FindName("PART_Watermark", box) as ContentControl;
            watermark.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#B777");
            //watermark.Content = "…";

            ((Border)watermark.Parent).BorderThickness = new Thickness(0);
            ((Border)((Grid)((Border)watermark.Parent).Parent).Children[0]).BorderThickness = new Thickness(0);
        }
    }
}
