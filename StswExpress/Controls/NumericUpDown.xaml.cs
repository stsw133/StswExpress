using System;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress.Controls
{
    /// <summary>
    /// Interaction logic for NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        public NumericUpDown()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Increment
        /// </summary>
        public double Increment
        {
            get => (double)GetValue(pIncrement);
            set { SetValue(pIncrement, value); }
        }
        public static readonly DependencyProperty pIncrement
            = DependencyProperty.Register(
                  nameof(Increment),
                  typeof(double),
                  typeof(NumericUpDown),
                  new PropertyMetadata(1d)
              );

        /// <summary>
        /// Value
        /// </summary>
        public double Value
        {
            get => (double)GetValue(pValue);
            set { SetValue(pValue, value); }
        }
        public static readonly DependencyProperty pValue
            = DependencyProperty.Register(
                  nameof(Value),
                  typeof(double),
                  typeof(NumericUpDown),
                  new PropertyMetadata(0d)
              );

        /// <summary>
        /// ValueMin
        /// </summary>
        public double? ValueMin
        {
            get => (double?)GetValue(pValueMin);
            set { SetValue(pValueMin, value); }
        }
        public static readonly DependencyProperty pValueMin
            = DependencyProperty.Register(
                  nameof(ValueMin),
                  typeof(double?),
                  typeof(NumericUpDown),
                  new PropertyMetadata(null)
              );

        /// <summary>
        /// ValueMax
        /// </summary>
        public double? ValueMax
        {
            get => (double?)GetValue(pValueMax);
            set { SetValue(pValueMax, value); }
        }
        public static readonly DependencyProperty pValueMax
            = DependencyProperty.Register(
                  nameof(ValueMax),
                  typeof(double?),
                  typeof(NumericUpDown),
                  new PropertyMetadata(null)
              );

        /// <summary>
        /// Up - Click
        /// </summary>
        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            Value += Increment;
            if (ValueMax != null && Value > ValueMax)
                Value = (double)ValueMax;
        }

        /// <summary>
        /// Down - Click
        /// </summary>
        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            Value -= Increment;
            if (ValueMin != null && Value < ValueMin)
                Value = (double)ValueMin;
        }

        /// <summary>
        /// Numeric - TextChanged
        /// </summary>
        private void tbNumeric_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if ((sender as TextBox).Text == "-")
                    return;
                var val = Convert.ToDouble((sender as TextBox).Text);

                if (ValueMin != null && val < ValueMin)
                    (sender as TextBox).Text = ValueMin.ToString();
                else if (ValueMax != null && val > ValueMax)
                    (sender as TextBox).Text = ValueMax.ToString();
            }
            catch
            {
                (sender as TextBox).Text = Value.ToString();
            }
        }
    }
}
