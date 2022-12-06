using Microsoft.Maui.Graphics.Platform;
using NativeMedia;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace MAUIcontainer {
    public class PhotoHelper : IPhotoHelper {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public IAPIService _APIService { get; set; }
        public int delayMilliseconds = 5000000;
        public PhotoHelper(IAPIService service) {
            _APIService = service;        
        }
        public string ThumbnailImage(Stream imagData) {
            using (Microsoft.Maui.Graphics.IImage image = PlatformImage.FromStream(imagData)) {
                if (image != null) {
                    using (Microsoft.Maui.Graphics.IImage newImage = image.Downsize(100, true)) {
                        return newImage.AsBase64();
                    }
                }
                return string.Empty;
            }
        }
        public async Task<ResponseDto> CapturePhoto(string args) {
            ResponseDto response = new ResponseDto();
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(delayMilliseconds));
            IMediaFile file = null;
            try {
                if (!MediaGallery.CheckCapturePhotoSupport()) {
                    response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                    response.Message = "No Camera feature!";
                }
                if (await Permissions.RequestAsync<Permissions.Camera>() != PermissionStatus.Granted) {
                    response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                    response.Message = "Camera access denied!";
                }
                if (await Permissions.RequestAsync<SaveMediaPermission>() != PermissionStatus.Granted) {
                    response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                    response.Message = "Media write access denied!";
                }
                file = await MediaGallery.CapturePhotoAsync(cts.Token);
                FileDto fileDto = new FileDto();
                fileDto.ContentType = file.ContentType;
                fileDto.Name = $"{file.NameWithoutExtension}.{file.Extension}";
                // save the file into local storage
                string localFilePath = Path.Combine(FileSystem.CacheDirectory, fileDto.Name);
                fileDto.FilePath = localFilePath;
                using var stream = await file.OpenReadAsync();
                using FileStream localFileStream = File.OpenWrite(localFilePath);
                await Task.Delay(100);
                await stream.CopyToAsync(localFileStream);
//                await MediaGallery.SaveAsync(MediaFileType.Image, stream, App.currentApp.Name);
                stream.Position = 0;
                fileDto.Src = ThumbnailImage(stream);
                response.Message = "Success";
                response.StatusCode = System.Net.HttpStatusCode.OK;
                response.Content = Convert.ToBase64String(Encoding.Default.GetBytes(JsonSerializer.Serialize(fileDto)));
            } catch (Exception e) {
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                response.Message = "Fail: " + e.Message;
            }
            return response;
        }
        public async Task<ResponseDto> uploadFiles(string files) {
            ResponseDto response = new ResponseDto();
            await MainThread.InvokeOnMainThreadAsync(async () => {
                try {
                    var filesRequest = JsonSerializer.Deserialize<FilesRequest>(files);
                    foreach (var file in filesRequest.Files) {
                        _APIService.UploadFileRequest(file, filesRequest.Request);
                    }
                    response.Message = "Success";
                    response.StatusCode = System.Net.HttpStatusCode.OK;

                } catch (Exception) {
                    response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                    response.Message = "Fail";
                }
            });
            return response;
        }
    }
}
