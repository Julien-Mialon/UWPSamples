using System.Threading.Tasks;
using SharpDX;
using WinRTXamlToolkit.Controls.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;
using SharpDX.Mathematics.Interop;
using Jupiter = Windows.UI.Xaml;
using D2D = SharpDX.Direct2D1;

namespace WinRTXamlToolkit.Composition.Renderers
{
    public static class EllipseRenderer
    {
        internal static async Task Render(CompositionEngine compositionEngine, SharpDX.Direct2D1.RenderTarget renderTarget, FrameworkElement rootElement, Ellipse ellipse)
        {
            RawRectangleF rect = ellipse.GetBoundingRect(rootElement).ToSharpDX();

            D2D.Ellipse d2dEllipse = new D2D.Ellipse(
                new RawVector2(
                    (float)((rect.Left + rect.Right) * 0.5),
                    (float)((rect.Top + rect.Bottom) * 0.5)),
                (float)(0.5 * rect.Width()),
                (float)(0.5 * rect.Height()));
            D2D.Brush fill = await ellipse.Fill.ToSharpDX(renderTarget, rect);

            D2D.Layer layer = ellipse.CreateAndPushLayerIfNecessary(renderTarget, rootElement);

            D2D.Brush stroke = await ellipse.Stroke.ToSharpDX(renderTarget, rect);

            if (ellipse.StrokeThickness > 0 &&
                stroke != null)
            {
                float halfStrokeThickness = (float)(ellipse.StrokeThickness * 0.5);
                d2dEllipse.RadiusX -= halfStrokeThickness;
                d2dEllipse.RadiusY -= halfStrokeThickness;

                if (fill != null)
                {
                    renderTarget.FillEllipse(d2dEllipse, fill);
                }

                renderTarget.DrawEllipse(
                    d2dEllipse,
                    stroke,
                    (float)ellipse.StrokeThickness,
                    ellipse.GetStrokeStyle(compositionEngine.D2DFactory));
            }
            else if (fill != null)
            {
                renderTarget.FillEllipse(d2dEllipse, fill);
            }

            if (layer != null)
            {
                renderTarget.PopLayer();
                layer.Dispose();
            }
        }
    }
}
