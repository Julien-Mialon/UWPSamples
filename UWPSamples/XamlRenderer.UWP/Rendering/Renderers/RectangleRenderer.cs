using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using WinRTXamlToolkit.Controls.Extensions;

namespace XamlRenderer.Rendering.Renderers
{
    public static class RectangleRenderer
    {
        internal static async Task Render(CompositionEngine compositionEngine, SharpDX.Direct2D1.RenderTarget renderTarget, FrameworkElement rootElement, Rectangle rectangle)
        {
            RawRectangleF rect = rectangle.GetBoundingRect(rootElement).ToSharpDX();
            Brush fill = await rectangle.Fill.ToSharpDX(renderTarget, rect);
            Brush stroke = await rectangle.Stroke.ToSharpDX(renderTarget, rect);

            try
            {
                Layer layer = rectangle.CreateAndPushLayerIfNecessary(renderTarget, rootElement);

                if (rectangle.RadiusX > 0 &&
                    rectangle.RadiusY > 0)
                {
                    RoundedRectangle roundedRect = new SharpDX.Direct2D1.RoundedRectangle();
                    roundedRect.Rect = rect;
                    roundedRect.RadiusX = (float)rectangle.RadiusX;
                    roundedRect.RadiusY = (float)rectangle.RadiusY;

                    if (rectangle.StrokeThickness > 0 &&
                        stroke != null)
                    {
                        float halfThickness = (float)(rectangle.StrokeThickness * 0.5);
                        roundedRect.Rect = rect.Eroded(halfThickness);

                        if (fill != null)
                        {
                            renderTarget.FillRoundedRectangle(roundedRect, fill);
                        }

                        renderTarget.DrawRoundedRectangle(
                            roundedRect,
                            stroke,
                            (float)rectangle.StrokeThickness,
                            rectangle.GetStrokeStyle(compositionEngine.D2DFactory));
                    }
                    else
                    {
                        renderTarget.FillRoundedRectangle(roundedRect, fill);
                    }
                }
                else
                {
                    if (rectangle.StrokeThickness > 0 &&
                        stroke != null)
                    {
                        float halfThickness = (float)(rectangle.StrokeThickness * 0.5);

                        if (fill != null)
                        {
                            renderTarget.FillRectangle(rect.Eroded(halfThickness), fill);
                        }

                        RawRectangleF strokeRect = rect.Eroded(halfThickness);
                        renderTarget.DrawRectangle(
                            strokeRect,
                            stroke,
                            (float)rectangle.StrokeThickness,
                            rectangle.GetStrokeStyle(compositionEngine.D2DFactory));
                    }
                    else
                    {
                        renderTarget.FillRectangle(rect, fill);
                    }
                }

                if (layer != null)
                {
                    renderTarget.PopLayer();
                    layer.Dispose();
                }
            }
            finally
            {
                if (fill != null)
                    fill.Dispose();
                if (stroke != null)
                    stroke.Dispose();
            }
        }
    }
}
