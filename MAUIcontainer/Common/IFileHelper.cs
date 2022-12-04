using Microsoft.Maui.Graphics.Platform;
using System.IO;
using System.Reflection;
using SkiaSharp;
using SkiaSharp.Views;

namespace MAUIcontainer {
    public interface IFileHelper {
        public string ThumbnailImage(Stream imagData);
        public Task<Stream> ResizeImage(Stream stream, bool rotate);
        public SKBitmap Rotate();
        public Task<ResponseDto> CapturePhoto(string args);
        public Task<ResponseDto> uploadFiles(string files);
    }
}
