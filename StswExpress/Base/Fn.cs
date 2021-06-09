using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace StswExpress
{
	public static class Fn
	{
		/// App version
		public static string AppVersion() => Assembly.GetEntryAssembly().GetName().Version.ToString().TrimEnd(".0".ToCharArray());

		/// App name + version
		public static string AppName => $"{Assembly.GetEntryAssembly().GetName().Name} {AppVersion()}";

		/// App database
		public static DB AppDatabase { get; set; } = new DB();

		/// <summary>
		/// Proxy
		/// </summary>
		public class BindingProxy : Freezable
		{
			protected override Freezable CreateInstanceCore() => new BindingProxy();

			public object Data
			{
				get => GetValue(DataProperty);
				set => SetValue(DataProperty, value);
			}

			public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
		}

		/// <summary>
		/// Loads image from byte[] to BitmapImage
		/// </summary>
		/// <param name="imageData">Byte array data</param>
		/// <returns>Image</returns>
		public static BitmapImage LoadImage(byte[] imageData)
		{
			if (imageData == null || imageData.Length == 0)
				return null;

			var image = new BitmapImage();
			using (var mem = new MemoryStream(imageData))
			{
				mem.Position = 0;
				image.BeginInit();
				image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.UriSource = null;
				image.StreamSource = mem;
				image.EndInit();
			}
			image.Freeze();

			return image;
		}

		/// <summary>
		/// Opens context menu of framework element
		/// </summary>
		/// <param name="sender">Framework element</param>
		public static void OpenContextMenu(object sender)
		{
			if (sender is FrameworkElement f)
				f.ContextMenu.IsOpen = true;
		}

		/// <summary>
		/// Opens file from path
		/// </summary>
		/// <param name="path">Path to file</param>
		public static void OpenFile(string path)
		{
			var process = new Process();
			process.StartInfo.FileName = path;
			process.StartInfo.UseShellExecute = true;
			process.StartInfo.Verb = "open";
			process.Start();
		}

		/// <summary>
		/// Gets column filters from DataGrid
		/// </summary>
		/// <param name="dg">DataGrid</param>
		/// <param name="filter">SQL filter as string</param>
		/// <param name="parameters">SQL parameters</param>
		public static void GetColumnFilters(DataGrid dg, out string filter, out List<(string name, object val)> parameters)
		{
			filter = string.Empty;
			parameters = new List<(string name, object val)>();

			foreach (var col in dg.Columns)
				if (col.Header is ColumnFilter c && c?.FilterSQL != null)
				{
					filter += c.FilterSQL + " and ";
					parameters.Add((c.ParamSQL + "1", (c.Value1 is List<object> ? null : c.Value1) ?? DBNull.Value));
					parameters.Add((c.ParamSQL + "2", (c.Value2 is List<object> ? null : c.Value2) ?? DBNull.Value));
				}
			filter = filter.TrimEnd(" and ".ToCharArray());
			if (string.IsNullOrWhiteSpace(filter))
				filter = "true";
		}

		/// <summary>
		/// Clears column filters in DataGrid
		/// </summary>
		/// <param name="dg">DataGrid</param>
		public static void ClearColumnFilters(DataGrid dg)
		{
			foreach (var col in dg.Columns)
				if (col.Header is ColumnFilter c)
				{
					c.Value1 = c.ValueDef;
					c.Value2 = c.ValueDef;
				}
		}
	}
}
