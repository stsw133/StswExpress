using System;
using System.ComponentModel;
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
        /// ButtonsAlignment
        /// </summary>
        public static readonly DependencyProperty ButtonsAlignmentProperty
            = DependencyProperty.Register(
                  nameof(ButtonsAlignment),
                  typeof(Dock),
                  typeof(NumericUpDown),
                  new PropertyMetadata(Dock.Right)
              );
        public Dock ButtonsAlignment
        {
            get => (Dock)GetValue(ButtonsAlignmentProperty);
            set => SetValue(ButtonsAlignmentProperty, value);
        }

        /// <summary>
        /// Increment
        /// </summary>
        public static readonly DependencyProperty IncrementProperty
            = DependencyProperty.Register(
                  nameof(Increment),
                  typeof(double),
                  typeof(NumericUpDown),
                  new PropertyMetadata(1d)
              );
        public double Increment
        {
            get => (double)GetValue(IncrementProperty);
            set => SetValue(IncrementProperty, value);
        }

        /// <summary>
        /// Value
        /// </summary>
        public static readonly DependencyProperty ValueProperty
            = DependencyProperty.Register(
                  nameof(Value),
                  typeof(double),
                  typeof(NumericUpDown),
                  new PropertyMetadata(default(double))
              );
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// ValueAlignment
        /// </summary>
        public static readonly DependencyProperty ValueAlignmentProperty
            = DependencyProperty.Register(
                  nameof(ValueAlignment),
                  typeof(HorizontalAlignment),
                  typeof(NumericUpDown),
                  new PropertyMetadata(default(HorizontalAlignment))
              );
        public HorizontalAlignment ValueAlignment
        {
            get => (HorizontalAlignment)GetValue(ValueAlignmentProperty);
            set => SetValue(ValueAlignmentProperty, value);
        }

        /// <summary>
        /// ValueMin
        /// </summary>
        public static readonly DependencyProperty ValueMinProperty
            = DependencyProperty.Register(
                  nameof(ValueMin),
                  typeof(double?),
                  typeof(NumericUpDown),
                  new PropertyMetadata(default(double?))
              );
        public double? ValueMin
        {
            get => (double?)GetValue(ValueMinProperty);
            set => SetValue(ValueMinProperty, value);
        }

        /// <summary>
        /// ValueMax
        /// </summary>
        public static readonly DependencyProperty ValueMaxProperty
            = DependencyProperty.Register(
                  nameof(ValueMax),
                  typeof(double?),
                  typeof(NumericUpDown),
                  new PropertyMetadata(default(double?))
              );
        public double? ValueMax
        {
            get => (double?)GetValue(ValueMaxProperty);
            set => SetValue(ValueMaxProperty, value);
        }

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
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            if (!IsLoaded)
                return;
            /*
            (sender as TextBox).TextChanged -= tbNumeric_TextChanged;
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
            (sender as TextBox).TextChanged += tbNumeric_TextChanged;
            */
        }
    }
}
