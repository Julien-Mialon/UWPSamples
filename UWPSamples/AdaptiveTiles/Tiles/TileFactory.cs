using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media.Imaging;
using AdaptiveTiles.Model;
using WinRTXamlToolkit.AwaitableUI;
using System.Runtime.InteropServices.WindowsRuntime;
using XamlRenderer.Rendering;

namespace AdaptiveTiles.Tiles
{
	public static class TileFactory
	{
		public static async Task<Border> SaveLargeTile(Todo item)
		{
			StorageFile tilefile = await Package.Current.InstalledLocation.GetFileAsync("tile.xml");
			string xamlContent = await FileIO.ReadTextAsync(tilefile);

			Border border = XamlReader.Load(xamlContent) as Border;
			TextBlock title = border.FindName("TitleText") as TextBlock;
			TextBlock description = border.FindName("DescriptionText") as TextBlock;
			Image stateImage = border.FindName("StateImage") as Image;

			title.Text = item.Title;
			description.Text = item.Description;
			string state = item.State == State.Running ? "green" : "red";

			stateImage.Source = new BitmapImage(new Uri($"ms-appx:///Resources/States/{state}.png"));

			/*
			Grid rootGrid = new Grid
			{
				Width = 310,
				Height = 150,
			};
			rootGrid.RowDefinitions.Add(new RowDefinition
			{
				Height = new GridLength(1, GridUnitType.Star)
			});
			rootGrid.RowDefinitions.Add(new RowDefinition
			{
				Height = new GridLength(1, GridUnitType.Star)
			});
			rootGrid.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1, GridUnitType.Star)
			});
			rootGrid.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1, GridUnitType.Star)
			});

			Image backgroundImage = new Image
			{
				Source = new BitmapImage(new Uri("ms-appx:///Resources/background-310x150.scale-100.png"))
			};
			backgroundImage.SetValue(Grid.RowProperty, 0);
			backgroundImage.SetValue(Grid.RowSpanProperty, 2);
			backgroundImage.SetValue(Grid.ColumnProperty, 0);
			backgroundImage.SetValue(Grid.ColumnSpanProperty, 2);
			rootGrid.Children.Add(backgroundImage);

			string stateFile = item.State == State.Running ? "green" : "red";
			Image stateImage = new Image
			{
				Source = new BitmapImage(new Uri($"ms-appx:///Resources/States/{stateFile}.png"))
			};
			stateImage.SetValue(Grid.RowProperty, 1);
			stateImage.SetValue(Grid.ColumnProperty, 0);
			rootGrid.Children.Add(stateImage);

			Image logoImage = new Image
			{
				Source = new BitmapImage(new Uri("ms-appx:///Resources/logo.png"))
			};
			logoImage.SetValue(Grid.RowProperty, 1);
			logoImage.SetValue(Grid.ColumnProperty, 1);
			rootGrid.Children.Add(logoImage);
			*/

			border.Measure(new Size(border.Width, border.Height));
			border.Arrange(new Rect(0, 0, border.Width, border.Height));

			await stateImage.WaitForImagesToLoadAsync();

			border.Measure(new Size(border.Width, border.Height));
			border.Arrange(new Rect(0, 0, border.Width, border.Height));

			await Save(border, "test_large.png", 310, 150);

			return border;
		}

		private static async Task Save(FrameworkElement ui, string filename, int width, int height)
		{
			StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
			await WriteableBitmapRenderExtensions.RenderToPngFile(file, ui);

			/*
			WriteableBitmap image = await WriteableBitmapRenderExtensions.Render(ui);


			StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

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
			// */
		}
	}
}
