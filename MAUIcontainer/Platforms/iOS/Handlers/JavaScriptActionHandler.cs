using Foundation;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Handlers;
using WebKit;
using MAUIcontainer.Platforms.iOS.Renderers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Platform;
using MAUIcontainer.Common;
using System.Reflection.Metadata;

namespace MAUIcontainer.Controls {
    public partial class JavaScriptActionHandler {
        public void ProcessMessage(string message, IViewHandler _handler) {
            handler = _handler;
            var webview = ((HybridWebViewHandler)handler).PlatformView;

            if (message.StartsWith("Download:")) {
                //"Download:abc.txt,mimeType:application\pdf,base64:xxxxxxxxxxxxxxxx
                int minePos = message.IndexOf(",mimeType:");
                int base64Pos = message.IndexOf(",base64:");
                string fileName = message.Substring(9, minePos - 9);
                string minetype = message.Substring(minePos + 10, base64Pos - minePos - 10);
                string base64 = message.Substring(base64Pos + 8);
                string dwldsPath = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments), fileName);
                byte[] pdfAsBytes = Convert.FromBase64String(base64);
                File.WriteAllBytes(dwldsPath, pdfAsBytes);
                Application.Current.MainPage.Navigation.PushModalAsync(new FileViewer(dwldsPath));
            } else if (message.StartsWith("callMAUI:")) {
                BlazorCallHelper.callMAUIhandler(ResolvePromise, message.Substring(9, message.Length - 9)!);
            } else {
                Application.Current.MainPage.DisplayAlert("Message", message, "Ok");
            }
        }
        public void ResolvePromise(string promiseId, string result) {
            var webview = ((HybridWebViewHandler)handler).PlatformView;
            webview.EvaluateJavaScript($"resolvePromise('{promiseId}', '{result}', null)", null);
        }
    }
}
