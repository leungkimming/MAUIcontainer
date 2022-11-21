using Microsoft.Maui.Graphics.Platform;
using System.IO;
using System.Reflection;
using SkiaSharp;
using SkiaSharp.Views;

namespace MAUIcontainer {
    public static class FileHelper {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        static SKBitmap m_bm;
        static SKBitmap m_resizedBm;
        static SKBitmap m_rotatedBm;
        public static string ThumbnailImage(Stream imagData) {
            Microsoft.Maui.Graphics.IImage image =PlatformImage.FromStream(imagData);
            if (image != null) {
                Microsoft.Maui.Graphics.IImage newImage = image.Downsize(100, true);
                return newImage.AsBase64();
            }
            return string.Empty;
        }
        public static async Task<Stream> ResizeImage(Stream stream, bool rotate) {
            m_bm = SKBitmap.Decode(stream);
            Int32 reference = m_bm.Width > m_bm.Height ? m_bm.Width : m_bm.Height;
            double factor = 1;
            if (reference > 1152) {
                factor = (double)reference / 1152.00;
            }
            Int32 width = Convert.ToInt32(m_bm.Width / factor);
            Int32 height = Convert.ToInt32(m_bm.Height / factor);
            SKSizeI sksize = new SKSizeI(width, height);

            try {
                m_resizedBm = m_bm.Resize(sksize, SKFilterQuality.High);

                SKImage image = null;
                if (rotate) {
                    m_rotatedBm = Rotate();
                    image = SKImage.FromPixels(m_rotatedBm.PeekPixels());
                } else {
                    image = SKImage.FromPixels(m_resizedBm.PeekPixels());
                }

                SKData encoded = image.Encode();
                Stream streamRotat = encoded.AsStream();
                return streamRotat;
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public static SKBitmap Rotate() {
            using (var bitmap = m_resizedBm) {
                var rotated = new SKBitmap(bitmap.Height, bitmap.Width);
                using (var surface = new SKCanvas(rotated)) {
                    surface.Translate(bitmap.Height, 0);
                    surface.RotateDegrees(90);
                    surface.DrawBitmap(bitmap, 0, 0);
                }
                return rotated;
            }
        }
    }
}
