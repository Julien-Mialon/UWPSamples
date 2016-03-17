using System.Threading.Tasks;
using Windows.UI.Xaml;
using SharpDX.Direct2D1;

namespace XamlRenderer.Rendering.Renderers
{
    public static class FrameworkElementRenderer
    {
        internal static async Task Render(CompositionEngine compositionEngine, RenderTarget renderTarget, FrameworkElement rootElement, FrameworkElement fe)
        {
            await compositionEngine.RenderChildren(renderTarget, rootElement, fe);
        }
    }
}
