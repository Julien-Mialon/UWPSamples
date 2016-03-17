using System.Threading.Tasks;
using Windows.Foundation;
using SharpDX.Mathematics.Interop;
using WinRTXamlToolkit.Controls.Extensions;
using D2D = SharpDX.Direct2D1;
using Jupiter = Windows.UI.Xaml;

namespace XamlRenderer.Rendering.Renderers
{
    public static class ImageRenderer
    {
        internal static async Task Render(CompositionEngine compositionEngine, SharpDX.Direct2D1.RenderTarget renderTarget, Jupiter.FrameworkElement rootElement, Jupiter.Controls.Image image)
        {
	        Rect boundingRect = image.GetBoundingRect(rootElement);

			RawRectangleF rect = boundingRect.ToSharpDX();
            if (rect.Width() == 0 ||
                rect.Height() == 0)
            {
                return;
            }

            D2D.Bitmap1 bitmap = await image.Source.ToSharpDX(renderTarget);

            if (bitmap == null)
            {
                return;
            }

            try
            {
                D2D.Layer layer = image.CreateAndPushLayerIfNecessary(renderTarget, rootElement);

                renderTarget.DrawBitmap(
                    bitmap,
                    rect,
                    (float)image.Opacity,
                    D2D.BitmapInterpolationMode.Linear,
					new RawRectangleF(0, 0, bitmap.PixelSize.Width, bitmap.PixelSize.Height));

                if (layer != null)
                {
                    renderTarget.PopLayer();
                    layer.Dispose();
                }
            }
            finally
            {
                bitmap.Dispose();
            }
        }
    }
}
