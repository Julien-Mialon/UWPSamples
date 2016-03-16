using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using SharpDX;
using WinRTXamlToolkit.Controls.Extensions;
using WinRTXamlToolkit.Imaging;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using AdaptiveTiles.Rendering;
using SharpDX.Mathematics.Interop;
using Jupiter = Windows.UI.Xaml;
using D2D = SharpDX.Direct2D1;

namespace WinRTXamlToolkit.Composition
{
    public static class Conversion
    {
        /// <summary>
        /// Creates and pushes a D2D layer if necessary. Returns the layer or null if not required.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="renderTarget">The render target.</param>
        /// <param name="rootElement"></param>
        /// <returns></returns>
        public static D2D.Layer CreateAndPushLayerIfNecessary(this Jupiter.FrameworkElement element, D2D.RenderTarget renderTarget, Jupiter.FrameworkElement rootElement)
        {
            if (element.Opacity >= 1)
                //element.Clip == null &&
                //element.RenderTransform == null)
            {
                return null;
            }

            D2D.Layer layer = new D2D.Layer(renderTarget);
            D2D.LayerParameters layerParameters = new D2D.LayerParameters();
            layerParameters.Opacity = (float)element.Opacity;
            layerParameters.ContentBounds = element.GetBoundingRect(rootElement).ToSharpDX();
            renderTarget.PushLayer(ref layerParameters, layer);

            return layer;
        }

        public static SharpDX.DirectWrite.TextAlignment ToSharpDX(
            this Jupiter.TextAlignment alignment)
        {
            switch (alignment)
            {
                case Jupiter.TextAlignment.Center:
                    return SharpDX.DirectWrite.TextAlignment.Center;
                case Jupiter.TextAlignment.Right:
                    return SharpDX.DirectWrite.TextAlignment.Trailing;
                case Jupiter.TextAlignment.Justify:
                    return SharpDX.DirectWrite.TextAlignment.Justified;
                case Jupiter.TextAlignment.Left:
                    return SharpDX.DirectWrite.TextAlignment.Leading;
                default:
                    throw new NotSupportedException("Unexpected TextAlignment value - not available in Windows 8 RTM.");
            }
        }

        public static async Task<D2D.Brush> ToSharpDX(
            this Brush brush,
            D2D.RenderTarget renderTarget,
            RawRectangleF rect)
        {
            if (brush == null)
                return null;

            SolidColorBrush solidColorBrush = brush as SolidColorBrush;

            if (solidColorBrush != null)
            {
                RawColor4 color = solidColorBrush.Color.ToSharpDX();
				
				return new D2D.SolidColorBrush(
                    renderTarget,
                    color,
                    new D2D.BrushProperties
                    {
                        Opacity = (float)solidColorBrush.Opacity
                    });
            }

            LinearGradientBrush linearGradientBrush = brush as LinearGradientBrush;

            if (linearGradientBrush != null)
            {
                D2D.LinearGradientBrushProperties properties = new D2D.LinearGradientBrushProperties();
                //properties.StartPoint =
                //    new Vector2(
                //        (float)(linearGradientBrush.StartPoint.X * renderTarget.Size.Width),
                //        (float)(linearGradientBrush.StartPoint.Y * renderTarget.Size.Height));
                //properties.EndPoint =
                //    new Vector2(
                //        (float)(linearGradientBrush.EndPoint.X * renderTarget.Size.Width),
                //        (float)(linearGradientBrush.EndPoint.Y * renderTarget.Size.Height));
                properties.StartPoint =
                    new RawVector2(
                        rect.Left + (float)(linearGradientBrush.StartPoint.X * rect.Width()),
                        rect.Top + (float)(linearGradientBrush.StartPoint.Y * rect.Height()));
                properties.EndPoint =
                    new RawVector2(
                        rect.Left + (float)(linearGradientBrush.EndPoint.X * rect.Width()),
                        rect.Top + (float)(linearGradientBrush.EndPoint.Y * rect.Height()));

                D2D.BrushProperties brushProperties = new D2D.BrushProperties();

                brushProperties.Opacity = (float)linearGradientBrush.Opacity;

                if (linearGradientBrush.Transform != null)
                {
                    brushProperties.Transform = linearGradientBrush.Transform.ToSharpDX();
                }

                D2D.GradientStopCollection gradientStopCollection = linearGradientBrush.GradientStops.ToSharpDX(renderTarget);

                return new D2D.LinearGradientBrush(
                    renderTarget,
                    properties,
                    brushProperties,
                    gradientStopCollection);
            }

            ImageBrush imageBrush = brush as ImageBrush;

            if (imageBrush != null)
            {
                D2D.Bitmap1 bitmap = await imageBrush.ImageSource.ToSharpDX(renderTarget);

                int w = bitmap.PixelSize.Width;
                int h = bitmap.PixelSize.Height;
                RawMatrix3x2 transform = MatrixExtensions.Identity();

                switch (imageBrush.Stretch)
                {
                    case Stretch.None:
                        transform.M31 += rect.Left + rect.Width() * 0.5f - w / 2;
                        transform.M32 += rect.Top + rect.Height() * 0.5f - h / 2;
                        break;
                    case Stretch.Fill:
                        transform = MatrixExtensions.Scaling(
                            rect.Width() / w,
                            rect.Height() / h);
                        transform.M31 += rect.Left;
                        transform.M32 += rect.Top;
                        break;
                    case Stretch.Uniform:
                        float bitmapAspectRatio = (float)w / h;
                        float elementAspectRatio = rect.Width() / rect.Height();

                        if (bitmapAspectRatio > elementAspectRatio)
                        {
                            float scale = rect.Width() / w;
                            transform = MatrixExtensions.Scaling(scale);
                            transform.M31 += rect.Left;
                            transform.M32 += rect.Top + rect.Height() * 0.5f - scale * h / 2;
                        }
                        else // (elementAspectRatio >= bitmapAspectRatio)
                        {
                            float scale = rect.Height() / h;
                            transform = MatrixExtensions.Scaling(scale);
                            transform.M31 += rect.Left + rect.Width() * 0.5f - scale * w / 2;
                            transform.M32 += rect.Top;
                        }

                        break;
                    case Stretch.UniformToFill:
                        float bitmapAspectRatio2 = (float)w / h;
                        float elementAspectRatio2 = rect.Width() / rect.Height();

                        if (bitmapAspectRatio2 > elementAspectRatio2)
                        {
                            float scale = rect.Height() / h;
                            transform = MatrixExtensions.Scaling(scale);
                            transform.M31 += rect.Left + rect.Width() * 0.5f - scale * w / 2;
                            transform.M32 += rect.Top;
                        }
                        else // (elementAspectRatio >= bitmapAspectRatio)
                        {
                            float scale = rect.Width() / w;
                            transform = MatrixExtensions.Scaling(scale);
                            transform.M31 += rect.Left;
                            transform.M32 += rect.Top + rect.Height() * 0.5f - scale * h / 2;
                        }

                        break;
                }
                

                return new D2D.BitmapBrush1(
                    (D2D.DeviceContext)renderTarget,
                    bitmap,
                    new D2D.BitmapBrushProperties1
                    {
                        ExtendModeX = D2D.ExtendMode.Clamp,
                        ExtendModeY = D2D.ExtendMode.Clamp,
                        InterpolationMode = D2D.InterpolationMode.HighQualityCubic
                    })
                    {
                        Opacity = (float)imageBrush.Opacity,
                        Transform = transform
                    };
                //    var writeableBitmap = imageBrush.ImageSource as WriteableBitmap;
                //    var bitmapImage = imageBrush.ImageSource as BitmapImage;

                //    if (bitmapImage != null)
                //    {
                //        writeableBitmap =
                //            await WriteableBitmapFromBitmapImageExtension.FromBitmapImage(bitmapImage);
                //    }
                //    CompositionEngine c;

                //    return new D2D.BitmapBrush(
                //        renderTarget,
                //        writeableBitmap.ToSharpDX(),
                //}
            }

#if DEBUG
            throw new NotSupportedException("Only SolidColorBrush supported for now");
#else
            return new D2D.SolidColorBrush(renderTarget, Color.Transparent);
#endif
        }

        public static RawMatrix3x2 ToSharpDX(
            this Transform transform)
        {
            MatrixTransform matrixTransform = transform as MatrixTransform;

            if (matrixTransform != null)
            {
                return matrixTransform.Matrix.ToSharpDX();
            }

            throw new NotImplementedException();
        }

        public static RawMatrix3x2 ToSharpDX(
            this Matrix matrix)
        {
                return new RawMatrix3x2(
                    (float)matrix.M11,
                    (float)matrix.M12,
                    (float)matrix.M21,
                    (float)matrix.M22,
                    (float)matrix.OffsetX,
                    (float)matrix.OffsetY);
        }

        public static D2D.GradientStopCollection ToSharpDX(
            this GradientStopCollection gradientStopCollection,
            D2D.RenderTarget renderTarget)
        {
            D2D.GradientStop[] gradientStops = new D2D.GradientStop[gradientStopCollection.Count];

            for (int i = 0; i < gradientStopCollection.Count; i++)
            {
                gradientStops[i] = gradientStopCollection[i].ToSharpDX();
            }

            return new D2D.GradientStopCollection(renderTarget, gradientStops);
        }

        public static D2D.GradientStop ToSharpDX(
            this GradientStop gradientStop)
        {
            return new D2D.GradientStop
                   {
                       Color = gradientStop.Color.ToSharpDX(),
                       Position = (float)gradientStop.Offset
                   };
        }

        public static RawColor4 ToSharpDX(
            this Windows.UI.Color color)
        {
            return new RawColor4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

        public static RawRectangleF ToSharpDX(this Rect rect)
        {
            return new RawRectangleF((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
        }

        public static D2D.CapStyle ToSharpDX(
            this PenLineCap lineCap)
        {
            switch (lineCap)
            {
                case PenLineCap.Flat:
                    return D2D.CapStyle.Flat;
                case PenLineCap.Round:
                    return D2D.CapStyle.Round;
                case PenLineCap.Square:
                    return D2D.CapStyle.Square;
                case PenLineCap.Triangle:
                    return D2D.CapStyle.Triangle;
                default:
                    throw new NotSupportedException("Unexpected PenLineCap value - not available in Windows 8 RTM.");
            }
        }

        public static D2D.LineJoin ToSharpDX(
            this PenLineJoin lineJoin)
        {
            switch (lineJoin)
            {
                case PenLineJoin.Miter:
                    return D2D.LineJoin.Miter;
                case PenLineJoin.Bevel:
                    return D2D.LineJoin.Bevel;
                case PenLineJoin.Round:
                    return D2D.LineJoin.Round;
                default:
                    throw new NotSupportedException("Unexpected PenLineJoin value - not available in Windows 8 RTM.");
            }
        }

        public static RawVector2 ToSharpDX(
            this Point point)
        {
            return new RawVector2(
                (float)point.X,
                (float)point.Y);
        }

        public static Size2F ToSharpDX(
            this Size size)
        {
            return new Size2F(
                (float)size.Width,
                (float)size.Height);
        }

        public static D2D.SweepDirection ToSharpDX(
            this SweepDirection sweepDirection)
        {
            return
                sweepDirection == SweepDirection.Clockwise
                    ? D2D.SweepDirection.Clockwise
                    : D2D.SweepDirection.CounterClockwise;
        }

        public static async Task<D2D.Bitmap1> ToSharpDX(this ImageSource imageSource, D2D.RenderTarget renderTarget)
        {
            WriteableBitmap wb = imageSource as WriteableBitmap;

            if (wb == null)
            {
                BitmapImage bi = imageSource as BitmapImage;

                if (bi == null)
                {
                    return null;
                }

                wb = await RenderingExtension.FromBitmapImage(bi);

                if (wb == null)
                {
                    return null;
                }
            }

            int width = wb.PixelWidth;
            int height = wb.PixelHeight;
            //var cpuReadBitmap = CompositionEngine.CreateCpuReadBitmap(width, height);

            D2D.Bitmap1 cpuReadBitmap = CompositionEngine.CreateRenderTargetBitmap(width, height);
            //var mappedRect = cpuReadBitmap.Map(D2D.MapOptions.Write | D2D.MapOptions.Read | D2D.MapOptions.Discard);

            using (Stream readStream = wb.PixelBuffer.AsStream())
            {
                int pitch = width * 4;
                //using (var writeStream =
                //    new DataStream(
                //        userBuffer: mappedRect.DataPointer,
                //        sizeInBytes: mappedRect.Pitch * height,
                //        canRead: false,
                //        canWrite: true))
                {
                    byte[] buffer = new byte[pitch * height];
                    readStream.Read(buffer, 0, buffer.Length);
                    cpuReadBitmap.CopyFromMemory(buffer, pitch);

                    //for (int i = 0; i < height; i++)
                    //{
                    //    readStream.Read(buffer, 0, mappedRect.Pitch);
                    //    writeStream.Write(buffer, 0, buffer.Length);
                    //}
                    
                }
            }
            //cpuReadBitmap.CopyFromMemory();

            return cpuReadBitmap;
        }
    }
}
