using System.Threading.Tasks;
using SharpDX;
using WinRTXamlToolkit.Controls.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SharpDX.Mathematics.Interop;
using Jupiter = Windows.UI.Xaml;
using D2D = SharpDX.Direct2D1;

namespace WinRTXamlToolkit.Composition.Renderers
{
    public static class BorderRenderer
    {
        internal static async Task Render(CompositionEngine compositionEngine, SharpDX.Direct2D1.RenderTarget renderTarget, FrameworkElement rootElement, Border border)
        {
            RawRectangleF rect = border.GetBoundingRect(rootElement).ToSharpDX();
            D2D.Brush brush = await border.Background.ToSharpDX(renderTarget, rect);

            if (brush != null)
            {
                D2D.PathGeometry geometry = GetBorderFillGeometry(compositionEngine, border, rect);

                D2D.Layer layer = border.CreateAndPushLayerIfNecessary(renderTarget, rootElement);

                renderTarget.FillGeometry(geometry, brush);

                if (layer != null)
                {
                    renderTarget.PopLayer();
                    layer.Dispose();
                }
            }

            await compositionEngine.RenderChildren(renderTarget, rootElement, border);
        }

        private static D2D.PathGeometry GetBorderFillGeometry(
            CompositionEngine compositionEngine, Border border, RawRectangleF rect)
        {
            Size2F topLeftCornerSize = new Size2F(
                (float)border.CornerRadius.TopLeft,
                (float)border.CornerRadius.TopLeft);
            Size2F topRightCornerSize = new Size2F(
                (float)border.CornerRadius.TopRight,
                (float)border.CornerRadius.TopRight);
            Size2F bottomLeftCornerSize = new Size2F(
                (float)border.CornerRadius.BottomLeft,
                (float)border.CornerRadius.BottomLeft);
            Size2F bottomRightCornerSize = new Size2F(
                (float)border.CornerRadius.BottomRight,
                (float)border.CornerRadius.BottomRight);

            float topCornersWidth = topLeftCornerSize.Width + topRightCornerSize.Width;

            if (topCornersWidth > rect.Width())
            {
                float scale = rect.Width() / topCornersWidth;
                topLeftCornerSize.Width *= scale;
                topRightCornerSize.Width *= scale;
            }

            float bottomCornersWidth = bottomLeftCornerSize.Width + bottomRightCornerSize.Width;

            if (bottomCornersWidth > rect.Width())
            {
                float scale = rect.Width() / bottomCornersWidth;
                bottomLeftCornerSize.Width *= scale;
                bottomRightCornerSize.Width *= scale;
            }

            float leftCornersHeight = topLeftCornerSize.Height + bottomLeftCornerSize.Height;

            if (leftCornersHeight > rect.Height())
            {
                float scale = rect.Height() / leftCornersHeight;
                topLeftCornerSize.Height *= scale;
                bottomLeftCornerSize.Height *= scale;
            }

            float rightCornersHeight = topRightCornerSize.Height + bottomRightCornerSize.Height;

            if (rightCornersHeight > rect.Height())
            {
                float scale = rect.Height() / rightCornersHeight;
                topRightCornerSize.Height *= scale;
                bottomRightCornerSize.Height *= scale;
            }

            D2D.PathGeometry geometry = new D2D.PathGeometry(compositionEngine.D2DFactory);

            // Create the geometry of the irregular rounded rectangle.
            D2D.GeometrySink geometrySink = geometry.Open();

            // Start to the right of the topleft corner.
            geometrySink.BeginFigure(
                new RawVector2(
                    rect.Left + topLeftCornerSize.Width,
                    rect.Top + 0), 
                D2D.FigureBegin.Filled);

            //if (topCornersWidth < rect.Width)
            {
                // Top edge
                geometrySink.AddLine(
                    new RawVector2(
                        rect.Left + rect.Width() - topRightCornerSize.Width,
                        rect.Top + 0));
            }

            //if (topRightCornerSize.Width > 0)

            // Top-right corner
            geometrySink.AddArc(
                new D2D.ArcSegment
                {
                    Point = new RawVector2(
                        rect.Left + rect.Width(),
                        rect.Top + topRightCornerSize.Height),
                    Size = topRightCornerSize,
                    RotationAngle = 0,
                    SweepDirection = D2D.SweepDirection.Clockwise,
                    ArcSize = D2D.ArcSize.Small
                });

            // Right edge
            geometrySink.AddLine(
                new RawVector2(
                    rect.Left + rect.Width(),
                    rect.Top + rect.Height() - bottomRightCornerSize.Height));

            // Bottom-right corner
            geometrySink.AddArc(
                new D2D.ArcSegment
                {
                    Point = new RawVector2(
                        rect.Left + rect.Width() - bottomRightCornerSize.Width,
                        rect.Top + rect.Height()),
                    Size = bottomRightCornerSize,
                    RotationAngle = 0,
                    SweepDirection = D2D.SweepDirection.Clockwise,
                    ArcSize = D2D.ArcSize.Small
                });

            // Bottom edge
            geometrySink.AddLine(
                new RawVector2(
                    rect.Left + bottomLeftCornerSize.Width,
                    rect.Top + rect.Height()));

            // Bottom-left corner
            geometrySink.AddArc(
                new D2D.ArcSegment
                {
                    Point = new RawVector2(
                        rect.Left + 0,
                        rect.Top + rect.Height() - bottomLeftCornerSize.Height),
                    Size = bottomLeftCornerSize,
                    RotationAngle = 0,
                    SweepDirection = D2D.SweepDirection.Clockwise,
                    ArcSize = D2D.ArcSize.Small
                });

            // Left edge
            geometrySink.AddLine(
                new RawVector2(
                    rect.Left + 0,
                    rect.Top + topLeftCornerSize.Height));

            // Top-left corner
            geometrySink.AddArc(
                new D2D.ArcSegment
                {
                    Point = new RawVector2(
                        rect.Left + topLeftCornerSize.Width,
                        rect.Top + 0),
                    Size = topLeftCornerSize,
                    RotationAngle = 0,
                    SweepDirection = D2D.SweepDirection.Clockwise,
                    ArcSize = D2D.ArcSize.Small
                });

            geometrySink.EndFigure(D2D.FigureEnd.Closed);
            geometrySink.Close();

            return geometry;
        }
    }
}
