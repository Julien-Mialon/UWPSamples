using System;
using System.Threading.Tasks;
using SharpDX;
using WinRTXamlToolkit.Controls.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using SharpDX.Mathematics.Interop;
using Jupiter = Windows.UI.Xaml;
using D2D = SharpDX.Direct2D1;

namespace WinRTXamlToolkit.Composition.Renderers
{
    public static class PathRenderer
    {
        internal static async Task Render(CompositionEngine compositionEngine, SharpDX.Direct2D1.RenderTarget renderTarget, FrameworkElement rootElement, Jupiter.Shapes.Path path)
        {
            RawRectangleF rect = path.GetBoundingRect(rootElement).ToSharpDX();
            D2D.Brush fill = await path.Fill.ToSharpDX(renderTarget, rect);
            D2D.Brush stroke = await path.Stroke.ToSharpDX(renderTarget, rect);

            D2D.Layer layer = path.CreateAndPushLayerIfNecessary(renderTarget, rootElement);
            RawMatrix3x2 oldTransform = renderTarget.Transform;
            renderTarget.Transform = new RawMatrix3x2(
                1, 0, 0, 1, rect.Left, rect.Top);
            //renderTarget.PushLayer(ref layerParameters, layer);

            D2D.Geometry d2dGeometry = path.Data.ToSharpDX(compositionEngine.D2DFactory, rect);

            if (fill != null)
            {
                renderTarget.FillGeometry(d2dGeometry, fill, null);
            }

            if (stroke != null &&
                path.StrokeThickness > 0)
            {
                renderTarget.DrawGeometry(
                d2dGeometry,
                stroke,
                (float)path.StrokeThickness,
                path.GetStrokeStyle(compositionEngine.D2DFactory));}

            //if (path.StrokeThickness > 0 &&
            //    stroke != null)
            //{
            //    var halfThickness = (float)(path.StrokeThickness * 0.5);
            //    roundedRect.Rect = rect.Eroded(halfThickness);

            //    if (fill != null)
            //    {
            //        renderTarget.FillRoundedRectangle(roundedRect, fill);
            //    }

            //    renderTarget.DrawRoundedRectangle(
            //        roundedRect,
            //        stroke,
            //        (float)path.StrokeThickness,
            //        path.GetStrokeStyle(compositionEngine.D2DFactory));
            //}
            //else
            //{
            //    renderTarget.FillRoundedRectangle(roundedRect, fill);
            //}

            if (layer != null)
            {
                renderTarget.PopLayer();
                layer.Dispose();
            }

            renderTarget.Transform = oldTransform;
        }

        public static D2D.Geometry ToSharpDX(this Geometry geometry, D2D.Factory factory, RawRectangleF rect)
        {
            D2D.PathGeometry d2dGeometry = new D2D.PathGeometry(factory);

            D2D.GeometrySink sink = d2dGeometry.Open();
            sink.AddGeometry(geometry, rect);
            sink.Close();

            return d2dGeometry;
        }

        public static void AddGeometry(this D2D.GeometrySink sink, Geometry geometry, RawRectangleF rect)
        {
            GeometryGroup geometryGroup = geometry as Jupiter.Media.GeometryGroup;

            if (geometryGroup != null)
            {
                sink.AddGeometryGroup(geometryGroup, rect);
            }

            LineGeometry lineGeometry = geometry as Jupiter.Media.LineGeometry;

            if (lineGeometry != null)
            {
                sink.AddLineGeometry(lineGeometry);
            }

            RectangleGeometry rectangleGeometry = geometry as Jupiter.Media.RectangleGeometry;

            if (rectangleGeometry != null)
            {
                sink.AddRectangleGeometry(rectangleGeometry);
            }

            EllipseGeometry ellipseGeometry = geometry as Jupiter.Media.EllipseGeometry;

            if (ellipseGeometry != null)
            {
                sink.AddEllipseGeometry(ellipseGeometry);
            }

            PathGeometry pathGeometry = geometry as Jupiter.Media.PathGeometry;

            if (pathGeometry != null)
            {
                sink.AddPathGeometry(pathGeometry);
            }
        }

        public static void AddGeometryGroup(this D2D.GeometrySink sink, GeometryGroup geometryGroup, RawRectangleF rect)
        {
            sink.SetFillMode(geometryGroup.FillRule.ToSharpDX());

            foreach (Geometry childGeometry in geometryGroup.Children)
            {
                sink.AddGeometry(childGeometry, rect);
            }
        }

        public static void AddPathGeometry(
            this D2D.GeometrySink sink, Jupiter.Media.PathGeometry pathGeometry)
        {
            sink.SetFillMode(pathGeometry.FillRule.ToSharpDX());

            foreach (PathFigure childFigure in pathGeometry.Figures)
            {
                sink.AddPathFigure(childFigure);
            }
        }

        public static void AddEllipseGeometry(
            this D2D.GeometrySink sink, Jupiter.Media.EllipseGeometry ellipseGeometry)
        {
            // Start the ellipse at 9 o'clock.
            sink.BeginFigure(
                new RawVector2(
                    (float)(ellipseGeometry.Center.X - ellipseGeometry.RadiusX),
                    (float)(ellipseGeometry.Center.Y)),
                    D2D.FigureBegin.Filled);


            // Do almost full ellipse in one arc (there is .00001 pixel size missing)
            sink.AddArc(
                new D2D.ArcSegment
                {
                    Point = new RawVector2(
                        (float)(ellipseGeometry.Center.X - ellipseGeometry.RadiusX),
                        (float)(ellipseGeometry.Center.Y + 0.00001)),
                    Size = new Size2F(
                        (float)(ellipseGeometry.RadiusX * 2),
                        (float)(ellipseGeometry.RadiusY * 2)),
                    RotationAngle = 0,
                    SweepDirection = D2D.SweepDirection.Clockwise,
                    ArcSize = D2D.ArcSize.Large
                });

            // Close the ellipse
            sink.EndFigure(D2D.FigureEnd.Closed);
        }

        public static void AddLineGeometry(
            this D2D.GeometrySink sink, Jupiter.Media.LineGeometry lineGeometry)
        {
            sink.BeginFigure(
                lineGeometry.StartPoint.ToSharpDX(),
                D2D.FigureBegin.Hollow);
            sink.AddLine(
                lineGeometry.EndPoint.ToSharpDX());
            sink.EndFigure(D2D.FigureEnd.Open);
        }

        public static void AddRectangleGeometry(
            this D2D.GeometrySink sink, Jupiter.Media.RectangleGeometry rectangleGeometry)
        {
            sink.BeginFigure(
                new RawVector2(
                    (float)(rectangleGeometry.Rect.Left),
                    (float)(rectangleGeometry.Rect.Top)),
                    D2D.FigureBegin.Filled);
            sink.AddLines(
                new []
                {
                    new RawVector2(
                        (float)(rectangleGeometry.Rect.Right),
                        (float)(rectangleGeometry.Rect.Top)),
                    new RawVector2(
                        (float)(rectangleGeometry.Rect.Right),
                        (float)(rectangleGeometry.Rect.Bottom)),
                    new RawVector2(
                        (float)(rectangleGeometry.Rect.Left),
                        (float)(rectangleGeometry.Rect.Bottom)),
                });
            sink.EndFigure(D2D.FigureEnd.Closed);
        }

        public static void AddPathFigure(
            this D2D.GeometrySink sink, Jupiter.Media.PathFigure pathFigure)
        {
            sink.BeginFigure(
                pathFigure.StartPoint.ToSharpDX(),
                pathFigure.IsFilled ? D2D.FigureBegin.Filled : D2D.FigureBegin.Hollow);

            foreach (PathSegment segment in pathFigure.Segments)
            {
                sink.AddPathFigureSegment(segment);
            }

            sink.EndFigure(pathFigure.IsClosed ? D2D.FigureEnd.Closed : D2D.FigureEnd.Open);
        }

        public static void AddPathFigureSegment(
            this D2D.GeometrySink sink, Jupiter.Media.PathSegment segment)
        {
            BezierSegment bezierSegment = segment as BezierSegment;

            if (bezierSegment != null)
            {
                sink.AddBezier(
                    new D2D.BezierSegment
                    {
                        Point1 = bezierSegment.Point1.ToSharpDX(),
                        Point2 = bezierSegment.Point2.ToSharpDX(),
                        Point3 = bezierSegment.Point3.ToSharpDX()
                    });
                return;
            }

            LineSegment lineSegment = segment as LineSegment;

            if (lineSegment != null)
            {
                sink.AddLine(
                    lineSegment.Point.ToSharpDX());
                return;
            }

            PolyBezierSegment polyBezierSegment = segment as PolyBezierSegment;

            if (polyBezierSegment != null)
            {
                D2D.BezierSegment[] beziers = new D2D.BezierSegment[polyBezierSegment.Points.Count / 3];

                for (int i = 0; i < beziers.Length; i++)
                {
                    beziers[i].Point1 = polyBezierSegment.Points[i * 3].ToSharpDX();
                    beziers[i].Point2 = polyBezierSegment.Points[i * 3 + 1].ToSharpDX();
                    beziers[i].Point3 = polyBezierSegment.Points[i * 3 + 2].ToSharpDX();
                }

                sink.AddBeziers(beziers);
                return;
            }

            PolyLineSegment polyLineSegment = segment as PolyLineSegment;

            if (polyLineSegment != null)
            {
                RawVector2[] lines = new RawVector2[polyLineSegment.Points.Count];

                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = polyLineSegment.Points[i].ToSharpDX();
                }

                sink.AddLines(lines);
                return;
            }

            QuadraticBezierSegment quadraticBezierSegment = segment as QuadraticBezierSegment;

            if (quadraticBezierSegment != null)
            {
                sink.AddQuadraticBezier(
                    new D2D.QuadraticBezierSegment
                    {
                        Point1 = quadraticBezierSegment.Point1.ToSharpDX(),
                        Point2 = quadraticBezierSegment.Point2.ToSharpDX()
                    });
                return;
            }

            PolyQuadraticBezierSegment polyQuadraticBezierSegment = segment as PolyQuadraticBezierSegment;

            if (polyQuadraticBezierSegment != null)
            {
                D2D.QuadraticBezierSegment[] quadraticBeziers = new D2D.QuadraticBezierSegment[polyBezierSegment.Points.Count / 2];

                for (int i = 0; i < quadraticBeziers.Length; i++)
                {
                    quadraticBeziers[i].Point1 = polyBezierSegment.Points[i * 2].ToSharpDX();
                    quadraticBeziers[i].Point2 = polyBezierSegment.Points[i * 2 + 1].ToSharpDX();
                }

                sink.AddQuadraticBeziers(quadraticBeziers);
                return;
            }

            ArcSegment arcSegment = segment as ArcSegment;

            if (arcSegment != null)
            {
                sink.AddArc(
                    new D2D.ArcSegment
                    {
                        Point = arcSegment.Point.ToSharpDX(),
                        Size = arcSegment.Size.ToSharpDX(),
                        RotationAngle = (float)arcSegment.RotationAngle,
                        SweepDirection = arcSegment.SweepDirection.ToSharpDX(),
                        ArcSize = arcSegment.IsLargeArc ? D2D.ArcSize.Large : D2D.ArcSize.Small
                    });
                return;
            }
        }

        public static D2D.FillMode ToSharpDX(this Jupiter.Media.FillRule fillRule)
        {
            switch (fillRule)
            {
                case FillRule.EvenOdd:
                    return D2D.FillMode.Alternate;
                case FillRule.Nonzero:
                    return D2D.FillMode.Winding;
                default:
                    throw new NotSupportedException("Unexpected FillRule value - not available in Windows 8 RTM.");
            }
        }
    }
}
