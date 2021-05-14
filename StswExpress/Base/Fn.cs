using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
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
		/// Load image from byte[] to BitmapImage
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
	}
}
