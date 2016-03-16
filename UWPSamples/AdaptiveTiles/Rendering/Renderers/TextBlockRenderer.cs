using System.Threading.Tasks;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using WinRTXamlToolkit.Controls.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SharpDX.Mathematics.Interop;

namespace WinRTXamlToolkit.Composition.Renderers
{
    public static class TextBlockRenderer
    {
        internal static async Task Render(CompositionEngine compositionEngine, RenderTarget renderTarget, FrameworkElement rootElement, TextBlock textBlock)
        {
            using (TextFormat textFormat = new TextFormat(
                compositionEngine.DWriteFactory,
                textBlock.FontFamily.Source,
                (float)textBlock.FontSize)
            {
                TextAlignment = textBlock.TextAlignment.ToSharpDX(),
                ParagraphAlignment = ParagraphAlignment.Near
            })
            {
                RawRectangleF rect = textBlock.GetBoundingRect(rootElement).ToSharpDX();
                // For some reason we need a bigger rect for the TextBlock rendering to fit in the same boundaries
                rect.Right++;
                rect.Bottom++;

                using (
                    Brush textBrush = await textBlock.Foreground.ToSharpDX(renderTarget, rect))
                {
                    if (textBrush == null)
                    {
                        return;
                    }

                    Layer layer = textBlock.CreateAndPushLayerIfNecessary(renderTarget, rootElement);

                    // You can render the bounding rectangle to debug composition
                    //renderTarget.DrawRectangle(
                    //    rect,
                    //    textBrush);
                    renderTarget.DrawText(
                        textBlock.Text,
                        textFormat,
                        rect,
                        textBrush);

                    if (layer != null)
                    {
                        renderTarget.PopLayer();
                        layer.Dispose();
                    }
                    //}
                }
            }
        }
    }
}
