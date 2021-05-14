using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace StswExpress
{
	public static class Functions
	{
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
