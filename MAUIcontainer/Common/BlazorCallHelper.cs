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

namespace MAUIcontainer {
    public static partial class BlazorCallHelper {
        private static IFileHelper _fileHelper;
        private static IAuthService _authService;
        public static void Configure(IFileHelper fileHelper, IAuthService authService) {
            _fileHelper = fileHelper;
            _authService = authService;
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
            string accessToken = null;
            var status = Task.Run(async () => await _authService.LoginStatus()).Result;
            if (status != null) {
                var result = _authService.LoginAsync(CancellationToken.None).Result;
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
        public static async void uploadFiles(string files) {
            ResponseDto response = await _fileHelper.uploadFiles(files);
            callback(promiseId, JsonSerializer.Serialize(response));
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

        public static async void photograph(string args) {
            ResponseDto response = await _fileHelper.CapturePhoto(args);
            callback(promiseId, JsonSerializer.Serialize(response));
        }
    }
}
