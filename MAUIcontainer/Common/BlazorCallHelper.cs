using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Plugin.Fingerprint.Abstractions;
using MAUIcontainer;
using MAUIcontainer.Controls;
using Plugin.Firebase.CloudMessaging;

namespace MAUIcontainer.Common {
    public static partial class BlazorCallHelper {
        public class CallMAUIjson {
            public string method { get; set; }
            public string promiseId { get; set; }
            public string args { get; set; }
        }
        public delegate void Callback(string promiseId, string result);
        public static Callback callback = null;
        public static string promiseId { get; set; }
        public static void callMAUIhandler(Callback _callback, string args) {
            callback = _callback;
            CallMAUIjson callMAUIjson = JsonSerializer.Deserialize<CallMAUIjson>(args);
            promiseId = callMAUIjson.promiseId;
            typeof(BlazorCallHelper).GetMethod(callMAUIjson.method).Invoke(null, new object[] { callMAUIjson.args });
        }
        public static string getAADToken() {
            var authService = new AuthService(); // most likely you will inject it in constructor, but for simplicity let's initialize it here
            string accessToken = null;
            var status = Task.Run(async () => await authService.LoginStatus()).Result;
            if (status != null) {
                var result = authService.LoginAsync(CancellationToken.None).Result;
                accessToken = result?.AccessToken;
            }
            return accessToken;
        }
        public static void getToken(string args) {
            string accessToken = getAADToken();
            callback(promiseId, accessToken);
        }
        public static void getPMessage(string AppId) {
            string pMessage = "";
            int key = App.MessageQueue.Where(x => (x.Value != null) && (x.Value.Data["App"] == AppId))
                .FirstOrDefault().Key;
            App.errmessage += $"getPMessage key={key};";
            if (key != 0) {
                string sMessage = JsonSerializer.Serialize(App.MessageQueue[key]);
                pMessage = $"{key}|{Convert.ToBase64String(Encoding.Default.GetBytes(sMessage))}";
                App.MessageQueue[key] = null;
                App.errmessage += $"getPMessage JS={pMessage.Substring(0, 15)};";
            }
            callback(promiseId, pMessage);
        }
        public static void uploadFiles(string files) {
            MainThread.InvokeOnMainThreadAsync(async () => {
                ResponseDto response= new ResponseDto();
                try {
                    var filesRequest=JsonSerializer.Deserialize<FilesRequest>(files);
                    foreach (var file in filesRequest.Files) {
                        APIService.UploadFileRequest(file, filesRequest.Request);
                    }
                    response.Message = "Success";
                    response.StatusCode = System.Net.HttpStatusCode.OK;

                } catch (Exception) {
                    response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                    response.Message = "Fail";
                    throw;
                } finally {
                    callback(promiseId, JsonSerializer.Serialize(response));
                }


            });
        }
        public static void displayPhoto(string filePath) {
            Application.Current.MainPage.Navigation.PushModalAsync(new ImageViewer(filePath));
            callback(promiseId, null);
        }
        public static void deletePhoto(string filePath) {
            if (filePath == "*") {
                string CacheDir = FileSystem.CacheDirectory;
                foreach (string name in Directory.EnumerateFiles(CacheDir)) {
                    File.Delete(name);
                }
            } else {
                File.Delete(filePath);
            }
            callback(promiseId, null);
        }

        public static void photograph(string args) {
#if IOS
            bool rotate = true;
#else
            bool rotate = false;
#endif

            MainThread.InvokeOnMainThreadAsync(async () => {
                ResponseDto response= new ResponseDto();
                try {
                    if (MediaPicker.Default.IsCaptureSupported) {
                        MediaPickerOptions  mediaPickerOptions= new MediaPickerOptions();
                        mediaPickerOptions.Title = args;
                        FileResult photo = await MediaPicker.Default.CapturePhotoAsync(mediaPickerOptions);
                        FileDto fileDto=new FileDto();
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
                            using (Stream trimStream = await FileHelper.ResizeImage(sourceStream, rotate)) {
                                using (FileStream localFileStream = File.OpenWrite(localFilePath)) {
                                    await trimStream.CopyToAsync(localFileStream);
                                    trimStream.Position= 0;
                                    fileDto.Src = FileHelper.ThumbnailImage(trimStream);
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
                    throw;
                } finally {
                    callback(promiseId, JsonSerializer.Serialize(response));
                }

            });
        }

    }
}
