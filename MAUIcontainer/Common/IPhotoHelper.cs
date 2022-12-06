using Microsoft.Maui.Graphics.Platform;
using System.IO;
using System.Reflection;

namespace MAUIcontainer {
    public interface IPhotoHelper {
        public string ThumbnailImage(Stream imagData);
        public Task<ResponseDto> CapturePhoto(string args);
        public Task<ResponseDto> uploadFiles(string files);
    }
}
