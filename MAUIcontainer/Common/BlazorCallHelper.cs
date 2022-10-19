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
        public static void getPMessage(string dummy) {
            string pMessage = "";
            int key = App.MessageQueue.Where(x => x.Value != null)
                .FirstOrDefault().Key;
            App.errmessage += $"getPMessage key={key};";
            if (key != 0) {
                pMessage = $"{key}|{JsonSerializer.Serialize(App.MessageQueue[key])}";
                pMessage = pMessage.Replace(@"\n", "").Replace(@"\u0022", "");
                App.MessageQueue[key] = null;
                App.errmessage += $"getPMessage JS={pMessage.Substring(0, 15)};";
            }
            callback(promiseId, pMessage);
        }
    }
}
