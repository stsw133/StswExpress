using System.IO;
using System.Windows.Media.Imaging;

namespace StswExpress.Globals
{
	internal static class Functions
	{
		/// <summary>
		/// Load image from byte[] to BitmapImage
		/// </summary>
		/// <param name="imageData"></param>
		/// <returns></returns>
		internal static BitmapImage LoadImage(byte[] imageData)
		{
			if (imageData == null || imageData.Length == 0) return null;
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
	}
}
