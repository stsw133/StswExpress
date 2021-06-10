using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace StswExpress
{
	/// <summary>
	/// Interaction logic for ExtDataGrid.xaml
	/// </summary>
	public partial class ExtDataGrid : DataGrid
	{
		/// <summary>
		/// Background color of the column headers
		/// </summary>
		public static readonly DependencyProperty HeaderBackgroundProperty
			= DependencyProperty.Register(
				  nameof(HeaderBackground),
				  typeof(string),
				  typeof(ExtDataGrid),
				  new PropertyMetadata("#F9F9F9")
			  );
		public string HeaderBackground
		{
			get => (string)GetValue(HeaderBackgroundProperty);
			set => SetValue(HeaderBackgroundProperty, value);
		}

		/// <summary>
		/// Loaded
		/// </summary>
		public virtual void DataGrid_Loaded(object sender, RoutedEventArgs e) => Load(sender, HeaderBackground);

		/// <summary>
		/// Load
		/// </summary>
		public static void Load(object sender, string headerBackground)
		{
			var win = sender as DataGrid;

			win.AutoGenerateColumns = false;
			win.HeadersVisibility = DataGridHeadersVisibility.Column;
			win.HorizontalGridLinesBrush = win.VerticalGridLinesBrush = Brushes.LightGray;

			var style = new Style(typeof(DataGridColumnHeader));
			style.Setters.Add(new Setter(BackgroundProperty, (Brush)new BrushConverter().ConvertFromString(headerBackground)));
			style.Setters.Add(new Setter(BorderBrushProperty, (Brush)new BrushConverter().ConvertFromString(new conv_Color().Convert(headerBackground, typeof(Brush), -0.1, CultureInfo.InvariantCulture).ToString())));
			style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(1, 0, 1, 1)));
			style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
			style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
			style.Setters.Add(new Setter(PaddingProperty, new Thickness(4, 3, 4, 3)));
			foreach (var col in win.Columns)
				col.HeaderStyle = style;
		}
	}
}
