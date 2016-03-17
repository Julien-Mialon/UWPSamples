using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using WinRTXamlToolkit.Controls.Extensions;

namespace XamlRenderer.Rendering.Renderers
{
    public static class LineRenderer
    {
        internal static async Task Render(CompositionEngine compositionEngine, SharpDX.Direct2D1.RenderTarget renderTarget, FrameworkElement rootElement, Line line)
        {
            RawRectangleF rect = line.GetBoundingRect(rootElement).ToSharpDX();
            Brush stroke = await line.Stroke.ToSharpDX(renderTarget, rect);

            if (stroke == null ||
                line.StrokeThickness <= 0)
            {
                return;
            }

            Layer layer = line.CreateAndPushLayerIfNecessary(renderTarget, rootElement);

            renderTarget.DrawLine(
                new RawVector2(
                    rect.Left + (float)line.X1,
                    rect.Top + (float)line.Y1),
                new RawVector2(
                    rect.Left + (float)line.X2,
                    rect.Top + (float)line.Y2),
                    stroke,
                    (float)line.StrokeThickness,
                    line.GetStrokeStyle(compositionEngine.D2DFactory));

            if (layer != null)
            {
                renderTarget.PopLayer();
                layer.Dispose();
            }
        }
    }
}
