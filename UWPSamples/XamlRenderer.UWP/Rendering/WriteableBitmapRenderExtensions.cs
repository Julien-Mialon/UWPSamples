using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace XamlRenderer.Rendering
{
	public static class WriteableBitmapRenderExtensions
	{
		public static async Task<WriteableBitmap> Render(FrameworkElement fe)
		{
			return await new CompositionEngine().RenderToWriteableBitmap(fe);
		}

		public static async Task RenderToPngFile(StorageFile file, FrameworkElement fe)
		{
			WriteableBitmap image = await Render(fe);
			
			using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
			{
				BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
				// Get pixels of the WriteableBitmap object 
				Stream pixelStream = image.PixelBuffer.AsStream();
				byte[] pixels = new byte[pixelStream.Length];
				await pixelStream.ReadAsync(pixels, 0, pixels.Length);
				// Save the image file with jpg extension 
				encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)image.PixelWidth, (uint)image.PixelHeight, 96.0, 96.0, pixels);
				await encoder.FlushAsync();
				await stream.FlushAsync();
			}
		}
	}
}
