using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for ColumnFilter.xaml
    /// </summary>
    public partial class ColumnFilter : StackPanel
    {
        public ColumnFilter()
        {
            InitializeComponent();
        }

        public enum Type
        {
            Text,
            Number,
            Date,
            Bool,
            List
        }

        public enum Mode
        {
            Equal,
            NotEqual,
            Contains,
            NotContains,
            Like,
            NotLike,
            StartsWith,
            EndsWith,
            Greater,
            GreaterEqual,
            Less,
            LessEqual,
            Between
        }

        /// <summary>
        /// FilterMode
        /// </summary>
        public static readonly DependencyProperty FilterModeProperty
            = DependencyProperty.Register(
                  nameof(FilterMode),
                  typeof(Mode),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(Mode))
              );
        public Mode FilterMode
        {
            get => (Mode)GetValue(FilterModeProperty);
            set => SetValue(FilterModeProperty, value);
        }

        /// <summary>
        /// FilterType
        /// </summary>
        public static readonly DependencyProperty FilterTypeProperty
            = DependencyProperty.Register(
                  nameof(FilterType),
                  typeof(Type),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(Type))
              );
        public Type FilterType
        {
            get => (Type)GetValue(FilterTypeProperty);
            set => SetValue(FilterTypeProperty, value);
        }

        /// <summary>
        /// Header
        /// </summary>
        public static readonly DependencyProperty HeaderProperty
            = DependencyProperty.Register(
                  nameof(Header),
                  typeof(string),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(string))
              );
        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        /// <summary>
        /// IsFilterVisible
        /// </summary>
        public static readonly DependencyProperty IsFilterVisibleProperty
            = DependencyProperty.Register(
                  nameof(IsFilterVisible),
                  typeof(bool),
                  typeof(ColumnFilter),
                  new PropertyMetadata(true)
              );
        public bool IsFilterVisible
        {
            get => (bool)GetValue(IsFilterVisibleProperty);
            set => SetValue(IsFilterVisibleProperty, value);
        }

        /// <summary>
        /// Value
        /// </summary>
        public static readonly DependencyProperty ValueProperty
            = DependencyProperty.Register(
                  nameof(Value),
                  typeof(object),
                  typeof(ColumnFilter),
                  new PropertyMetadata(default(object))
              );
        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Loaded
        /// </summary>
        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var items = Extensions.FindVisualChildren<MenuItem>(lblMode.ContextMenu).ToList();
            var binding = new Binding()
            {
                Path = new PropertyPath("Value"),
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(StackPanel), 1),
                TargetNullValue = "",
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            var inputbinding = new KeyBinding()
            {
                Command = Commands.Refresh,
                Key = Key.Return
            };
            var dp = lblMode.Parent as DockPanel;

            if (FilterType == Type.Text)
            {
                var valuecontainer = new TextBox();
                valuecontainer.InputBindings.Add(inputbinding);
                valuecontainer.SetBinding(TextBox.TextProperty, binding);
                dp.Children.Add(valuecontainer);

                FilterMode = Mode.Contains;
                lblMode.Content = items.First(x => (string)x.Tag == Mode.Contains.ToString()).Icon;
            }
            else if (FilterType == Type.Number)
            {
                var valuecontainer = new NumericUpDown();
                valuecontainer.InputBindings.Add(inputbinding);
                valuecontainer.SetBinding(NumericUpDown.ValueProperty, binding);
                dp.Children.Add(valuecontainer);

                FilterMode = Mode.Equal;
                lblMode.Content = items.First(x => (string)x.Tag == Mode.Equal.ToString()).Icon;
            }
        }

        /// <summary>
        /// Refresh
        /// </summary>
        private void cmdRefresh_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Commands.Refresh.Execute(null, Parent as UIElement);
            }
            catch { }
        }

        /// <summary>
        /// Filter mode change
        /// </summary>
        private void miFilterMode_Click(object sender, RoutedEventArgs e)
        {
            lblMode.Content = (sender as MenuItem).Icon;
            FilterMode = (Mode)Enum.Parse(typeof(Mode), (sender as MenuItem).Tag.ToString());
        }
    }
}
