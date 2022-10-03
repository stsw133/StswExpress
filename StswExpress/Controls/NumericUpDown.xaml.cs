using System;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress
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

        /// BoxAlignment
        public static readonly DependencyProperty BoxAlignmentProperty
            = DependencyProperty.Register(
                  nameof(BoxAlignment),
                  typeof(HorizontalAlignment),
                  typeof(NumericUpDown),
                  new PropertyMetadata(default(HorizontalAlignment))
              );
        public HorizontalAlignment BoxAlignment
        {
            get => (HorizontalAlignment)GetValue(BoxAlignmentProperty);
            set => SetValue(BoxAlignmentProperty, value);
        }

        /// ButtonsAlignment
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

        /// Increment
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

        /// Max
        public static readonly DependencyProperty MaxProperty
            = DependencyProperty.Register(
                  nameof(Max),
                  typeof(double?),
                  typeof(NumericUpDown),
                  new PropertyMetadata(default(double?))
              );
        public double? Max
        {
            get => (double?)GetValue(MaxProperty);
            set => SetValue(MaxProperty, value);
        }

        /// Min
        public static readonly DependencyProperty MinProperty
            = DependencyProperty.Register(
                  nameof(Min),
                  typeof(double?),
                  typeof(NumericUpDown),
                  new PropertyMetadata(default(double?))
              );
        public double? Min
        {
            get => (double?)GetValue(MinProperty);
            set => SetValue(MinProperty, value);
        }

        /// Value
        public static readonly DependencyProperty ValueProperty
            = DependencyProperty.Register(
                  nameof(Value),
                  typeof(double?),
                  typeof(NumericUpDown),
                  new PropertyMetadata(default(double?))
              );
        public double? Value
        {
            get => (double?)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// Up - Click
        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            Value += Increment;
            if (Value == null)
            {
                if (((double?)0).Between(Min, Max))
                    Value = 0;
                else
                    Value = Math.Min(Math.Abs(Min ?? 0d), Math.Abs(Max ?? 0d));
            }
            else if (Max != null && Value > Max)
                Value = (double)Max;
        }

        /// Down - Click
        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            Value -= Increment;
            if (Value == null)
            {
                if (((double?)0).Between(Min, Max))
                    Value = 0;
                else
                    Value = Math.Min(Math.Abs(Min ?? 0d), Math.Abs(Max ?? 0d));
            }
            else if (Min != null && Value < Min)
                Value = (double)Min;
        }

        /// TextBox - LostFocus
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Min != null && Value < Min)
                Value = (double)Min;
            if (Max != null && Value > Max)
                Value = (double)Max;
        }
    }
}
