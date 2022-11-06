using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gaussians.ViewFunctions
{
    internal enum ExportFormat
    {
        PNG, JPEG, GIF
    }
    internal static class ImageExporter
    {
        public static RenderTargetBitmap GetRenderBitmap(FrameworkElement element, Size size, PixelFormat format)
        {
            element.Measure(size);
            element.Arrange(new(size));

            RenderTargetBitmap bitmap = new((int)size.Width + 1, (int)size.Height + 1,
                96, 96, format);
            bitmap.Render(element);
            return bitmap;
        }
        public static BitmapEncoder GetEncoder(ExportFormat export) => export switch
        {
            ExportFormat.PNG => new PngBitmapEncoder(),
            ExportFormat.JPEG => new JpegBitmapEncoder(),
            ExportFormat.GIF => new GifBitmapEncoder()
        };
        public static string ConvertFormatToString(ExportFormat format) => format switch
        {
            ExportFormat.PNG => ".png",
            ExportFormat.JPEG => ".jpeg",
            ExportFormat.GIF => ".gif"
        };
    }
    internal interface IBitmapConverter
    {

    }

}
