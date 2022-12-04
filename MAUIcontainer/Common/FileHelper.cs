using Microsoft.Maui.Graphics.Platform;
using System.IO;
using System.Reflection;
using SkiaSharp;
using SkiaSharp.Views;
using System.Text;
using System.Text.Json;

namespace MAUIcontainer {
    public class FileHelper : IFileHelper {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        SKBitmap m_bm;
        SKBitmap m_resizedBm;
        SKBitmap m_rotatedBm;
        public IAPIService _APIService { get; set; }
        public FileHelper(IAPIService service) {
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
        public async Task<Stream> ResizeImage(Stream stream, bool rotate) {
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
                image.Dispose();
                Stream streamRotat = encoded.AsStream();
                return streamRotat;
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public SKBitmap Rotate() {
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
        public async Task<ResponseDto> CapturePhoto(string args) {
#if IOS
            bool rotate = true;
#else
            bool rotate = false;
#endif
            ResponseDto response = new ResponseDto();

            await MainThread.InvokeOnMainThreadAsync(async () => {
                try {
                    if (MediaPicker.Default.IsCaptureSupported) {
                        MediaPickerOptions mediaPickerOptions = new MediaPickerOptions();
                        mediaPickerOptions.Title = args;
                        FileResult photo = await MediaPicker.Default.CapturePhotoAsync(mediaPickerOptions);
                        FileDto fileDto = new FileDto();
                        if (photo.ContentType.StartsWith("image")) {
                            fileDto.ContentType = photo.ContentType;
                        } else {
                            fileDto.ContentType = $"image/{photo.ContentType}";
                        }
                        fileDto.Name = photo.FileName;
                        // save the file into local storage
                        string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
                        fileDto.FilePath = localFilePath;
                        using (Stream sourceStream = await photo.OpenReadAsync()) {
                            using (Stream trimStream = await ResizeImage(sourceStream, rotate)) {
                                using (FileStream localFileStream = File.OpenWrite(localFilePath)) {
                                    await trimStream.CopyToAsync(localFileStream);
                                    trimStream.Position = 0;
                                    fileDto.Src = ThumbnailImage(trimStream);
                                }
                            }
                        }
                        response.Message = "Success";
                        response.StatusCode = System.Net.HttpStatusCode.OK;
                        response.Content = Convert.ToBase64String(Encoding.Default.GetBytes(JsonSerializer.Serialize(fileDto)));
                    }
                } catch (Exception) {
                    response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                    response.Message = "Fail";
                }
            });

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
