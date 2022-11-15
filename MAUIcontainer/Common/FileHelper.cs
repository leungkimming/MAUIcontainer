using Microsoft.Maui.Graphics.Platform;
using System.IO;
using System.Reflection;

namespace MAUIcontainer {
    public static class FileHelper {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public static string ThumbnailImage(Stream imagData) {
            Microsoft.Maui.Graphics.IImage image =PlatformImage.FromStream(imagData);
            if (image != null) {
                Microsoft.Maui.Graphics.IImage newImage = image.Downsize(100, true);
                return newImage.AsBase64();
            }
            return string.Empty;
        }
    }
}
