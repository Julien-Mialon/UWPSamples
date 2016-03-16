using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using WinRTXamlToolkit.Composition.Renderers;
using WinRTXamlToolkit.Controls.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;
using D2D = SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Device = SharpDX.Direct3D11.Device;
using WIC = SharpDX.WIC;
using Jupiter = Windows.UI.Xaml;
using Path = Windows.UI.Xaml.Shapes.Path;

namespace WinRTXamlToolkit.Composition
{
	public class CompositionEngine
	{
		// ReSharper disable InconsistentNaming
		private static readonly SharpDX.WIC.ImagingFactory _wicFactory;
		private static readonly SharpDX.Direct2D1.Factory _d2DFactory;
		private static readonly SharpDX.DirectWrite.Factory _dWriteFactory;
		private static readonly SharpDX.Direct2D1.DeviceContext _d2DDeviceContext;
		// ReSharper restore InconsistentNaming

		static CompositionEngine()
		{
			try
			{
				_wicFactory = new SharpDX.WIC.ImagingFactory();
				_dWriteFactory = new SharpDX.DirectWrite.Factory();

				Device d3DDevice = new SharpDX.Direct3D11.Device(
					DriverType.Hardware,
					DeviceCreationFlags.BgraSupport
					,
					FeatureLevel.Level_11_1,
					FeatureLevel.Level_11_0,
					FeatureLevel.Level_10_1,
					FeatureLevel.Level_10_0,
					FeatureLevel.Level_9_3,
					FeatureLevel.Level_9_2,
					FeatureLevel.Level_9_1
					);

				SharpDX.DXGI.Device dxgiDevice = ComObject.As<SharpDX.DXGI.Device>(d3DDevice.NativePointer);
				//new SharpDX.DXGI.Device2(d3DDevice.NativePointer);
				D2D.Device d2DDevice = new SharpDX.Direct2D1.Device(dxgiDevice);
				_d2DFactory = d2DDevice.Factory;
				_d2DDeviceContext = new SharpDX.Direct2D1.DeviceContext(d2DDevice, D2D.DeviceContextOptions.None);
				_d2DDeviceContext.DotsPerInch = new Size2F(LogicalDpi, LogicalDpi);
			}
			catch (Exception ex)
			{
				
			}
		}

		public static float LogicalDpi
		{
			get
			{
#if WIN81
                return Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi;
#else
				return Windows.Graphics.Display.DisplayProperties.LogicalDpi;
#endif
			}
		}

		public WIC.ImagingFactory WicFactory
		{
			get
			{
				return _wicFactory;
			}
		}

		public SharpDX.Direct2D1.Factory D2DFactory
		{
			get
			{
				return _d2DFactory;
			}
		}

		public SharpDX.DirectWrite.Factory DWriteFactory
		{
			get
			{
				return _dWriteFactory;
			}
		}

		public async Task<WriteableBitmap> RenderToWriteableBitmap(FrameworkElement fe)
		{
			int width = (int)Math.Ceiling(fe.ActualWidth);
			int height = (int)Math.Ceiling(fe.ActualHeight);

			if (width == 0 ||
				height == 0)
			{
				throw new InvalidOperationException("Can't render an empty element. ActualWidth or ActualHeight equal 0. Consider awaiting a WaitForNonZeroSizeAsync() call or invoking Measure()/Arrange() before the call to Render().");
			}

			using (D2D.Bitmap1 renderTargetBitmap = CreateRenderTargetBitmap(width, height))
			{
				_d2DDeviceContext.Target = renderTargetBitmap;
				_d2DDeviceContext.AntialiasMode = D2D.AntialiasMode.PerPrimitive;
				_d2DDeviceContext.TextAntialiasMode = D2D.TextAntialiasMode.Grayscale;

				await Compose(_d2DDeviceContext, fe);

				using (D2D.Bitmap1 cpuReadBitmap = CreateCpuReadBitmap(width, height))
				{
					cpuReadBitmap.CopyFromRenderTarget(
						_d2DDeviceContext,
						new RawPoint(0, 0),
						new RawRectangle(0, 0, width, height));
					DataRectangle mappedRect = cpuReadBitmap.Map(D2D.MapOptions.Read);

					try
					{
						using (DataStream readStream =
							new DataStream(
								userBuffer: mappedRect.DataPointer,
								sizeInBytes: mappedRect.Pitch * height,
								canRead: true,
								canWrite: false))
						{
							WriteableBitmap wb = new WriteableBitmap(width, height);

							using (Stream writeStream = wb.PixelBuffer.AsStream())
							{
								byte[] buffer = new byte[mappedRect.Pitch];

								for (int i = 0; i < height; i++)
								{
									readStream.Read(buffer, 0, mappedRect.Pitch);
									writeStream.Write(buffer, 0, width * 4);
								}
							}

							wb.Invalidate();

							return wb;
						}
					}
					finally
					{
						cpuReadBitmap.Unmap();
					}
				}
			}
		}

		internal static D2D.Bitmap1 CreateRenderTargetBitmap(int width, int height)
		{
			D2D.Bitmap1 renderTargetBitmap = new D2D.Bitmap1(
				_d2DDeviceContext,
				new Size2(width, height),
				new D2D.BitmapProperties1(
					new D2D.PixelFormat(
						Format.B8G8R8A8_UNorm, D2D.AlphaMode.Premultiplied),
						LogicalDpi,
						LogicalDpi,
						D2D.BitmapOptions.Target));

			return renderTargetBitmap;
		}

		internal static D2D.Bitmap1 CreateCpuReadBitmap(int width, int height)
		{
			D2D.Bitmap1 cpuReadBitmap = new D2D.Bitmap1(
				_d2DDeviceContext,
				new Size2(width, height),
				new D2D.BitmapProperties1(
					new D2D.PixelFormat(
						SharpDX.DXGI.Format.B8G8R8A8_UNorm, D2D.AlphaMode.Premultiplied),
						LogicalDpi,
						LogicalDpi,
						D2D.BitmapOptions.CpuRead | D2D.BitmapOptions.CannotDraw));

			return cpuReadBitmap;
		}

		public async Task<MemoryStream> RenderToPngStream(FrameworkElement fe)
		{
			int width = (int)Math.Ceiling(fe.ActualWidth);
			int height = (int)Math.Ceiling(fe.ActualHeight);

			if (width == 0 ||
				height == 0)
			{
				throw new InvalidOperationException("Can't render an empty element. ActualWidth or ActualHeight equal 0. Consider awaiting a WaitForNonZeroSizeAsync() call or invoking Measure()/Arrange() before the call to Render().");
			}

			// pixel format with transparency/alpha channel and RGB values premultiplied by alpha
			Guid pixelFormat = WIC.PixelFormat.Format32bppPRGBA;

			using (WIC.Bitmap wicBitmap = new WIC.Bitmap(
				this.WicFactory,
				width,
				height,
				pixelFormat,
				WIC.BitmapCreateCacheOption.CacheOnLoad))
			{
				D2D.RenderTargetProperties renderTargetProperties = new D2D.RenderTargetProperties(
					D2D.RenderTargetType.Default,
					new D2D.PixelFormat(
						Format.R8G8B8A8_UNorm, D2D.AlphaMode.Premultiplied),
					//new D2DPixelFormat(Format.Unknown, AlphaMode.Unknown), // use this for non-alpha, cleartype antialiased text
					0,
					0,
					D2D.RenderTargetUsage.None,
					D2D.FeatureLevel.Level_DEFAULT);
				using (D2D.WicRenderTarget renderTarget = new D2D.WicRenderTarget(
					this.D2DFactory,
					wicBitmap,
					renderTargetProperties)
				{
					//TextAntialiasMode = TextAntialiasMode.Cleartype // this only works with the pixel format with no alpha channel
					TextAntialiasMode =
							D2D.TextAntialiasMode.Grayscale
					// this is the best we can do for bitmaps with alpha channels
				})
				{
					await Compose(renderTarget, fe);
				}

				// TODO: There is no need to encode the bitmap to PNG - we could just copy the texture pixel buffer to a WriteableBitmap pixel buffer.
				return GetBitmapAsStream(wicBitmap);
			}
		}

		private MemoryStream GetBitmapAsStream(WIC.Bitmap wicBitmap)
		{
			int width = wicBitmap.Size.Width;
			int height = wicBitmap.Size.Height;
			MemoryStream ms = new MemoryStream();

			using (WIC.WICStream stream = new WIC.WICStream(
				this.WicFactory,
				ms))
			{
				using (WIC.PngBitmapEncoder encoder = new WIC.PngBitmapEncoder(WicFactory))
				{
					encoder.Initialize(stream);

					using (WIC.BitmapFrameEncode frameEncoder = new WIC.BitmapFrameEncode(encoder))
					{
						frameEncoder.Initialize();

						frameEncoder.SetSize(width, height);
						Guid format = WIC.PixelFormat.Format32bppBGRA;
						frameEncoder.SetPixelFormat(ref format);
						frameEncoder.WriteSource(wicBitmap);
						frameEncoder.Commit();
					}

					encoder.Commit();
				}
			}

			ms.Position = 0;
			return ms;
		}

		public async Task Compose(D2D.RenderTarget renderTarget, FrameworkElement fe)
		{
			renderTarget.BeginDraw();
			renderTarget.Clear(new RawColor4(0, 0, 0, 0));
			await this.Render(renderTarget, fe, fe);
			renderTarget.EndDraw();
		}

		public async Task Render(D2D.RenderTarget renderTarget, FrameworkElement rootElement, FrameworkElement fe)
		{
			TextBlock textBlock = fe as TextBlock;

			if (textBlock != null)
			{
				await TextBlockRenderer.Render(this, renderTarget, rootElement, textBlock);
				return;
			}

			Rectangle rectangle = fe as Jupiter.Shapes.Rectangle;

			if (rectangle != null)
			{
				await RectangleRenderer.Render(this, renderTarget, rootElement, rectangle);
				return;
			}

			Border border = fe as Border;

			if (border != null)
			{
				await BorderRenderer.Render(this, renderTarget, rootElement, border);
				return;
			}

			Image image = fe as Image;

			if (image != null)
			{
				await ImageRenderer.Render(this, renderTarget, rootElement, image);
				return;
			}

			Ellipse ellipse = fe as Ellipse;

			if (ellipse != null)
			{
#pragma warning disable 4014
				EllipseRenderer.Render(this, renderTarget, rootElement, ellipse);
#pragma warning restore 4014
				return;
			}

			Line line = fe as Line;

			if (line != null)
			{
				await LineRenderer.Render(this, renderTarget, rootElement, line);
				return;
			}

			Path path = fe as Jupiter.Shapes.Path;

			if (path != null)
			{
				await PathRenderer.Render(this, renderTarget, rootElement, path);
				return;
			}

			await FrameworkElementRenderer.Render(this, renderTarget, rootElement, fe);
		}

		internal async Task RenderChildren(D2D.RenderTarget renderTarget, FrameworkElement rootElement, FrameworkElement fe)
		{
			IEnumerable<DependencyObject> children = fe.GetChildrenByZIndex();

			foreach (DependencyObject dependencyObject in children)
			{
				FrameworkElement child = dependencyObject as FrameworkElement;

				Debug.Assert(child != null);

				if (child != null &&
					child.Opacity > 0 &&
					child.Visibility == Visibility.Visible)
				{
					await this.Render(renderTarget, rootElement, child);
				}
			}
		}
	}
}
