using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Net.Http;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using AndroidWeb= Android.Webkit;
using Android.Widget;
using AndroidNet=Android.Net;
using Java.Interop;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;
using MAUIcontainer.Controls;
using static Android.Net.Http.SslCertificate;
using Microsoft.Extensions.DependencyInjection;
using Java.Net;
using Java.Lang;
using Android.Views;
using Microsoft.Maui.Controls;
using System.Text.Json;
using Plugin.Firebase.CloudMessaging;

namespace MAUIcontainer.Platforms.Droid.Renderers {
    public class HybridWebViewHandler : ViewHandler<IHybridWebView, AndroidWeb.WebView> {
        public static PropertyMapper<IHybridWebView, HybridWebViewHandler> HybridWebViewMapper = new PropertyMapper<IHybridWebView, HybridWebViewHandler>(ViewHandler.ViewMapper) {
            [nameof(IHybridWebView.Source)] = MapSource
        };

        const string JavascriptFunction = "function invokeMAUIAction(data){jsBridge.invokeAction(data);}";

        private JSBridge jsBridgeHandler;
        public HybridWebViewHandler() : base(HybridWebViewMapper) {            
        }

        static void MapSource(HybridWebViewHandler handler, IHybridWebView entry) {
            if (entry.Source != null) {
                LoadSource(entry.Source, handler.PlatformView);
            }
        }

        protected override AndroidWeb.WebView CreatePlatformView() {
            var webView = new AndroidWeb.WebView(Context);
            jsBridgeHandler = new JSBridge(this);

            webView.Settings.JavaScriptEnabled = true;

            webView.Settings.SetSupportMultipleWindows(true);
            webView.Settings.SetEnableSmoothTransition(true);
            webView.Settings.DomStorageEnabled = true;
            webView.Settings.AllowFileAccessFromFileURLs = true;
            webView.Settings.DefaultTextEncodingName = "UTF-8";
            webView.SetWebViewClient(new JavascriptWebViewClient($"javascript: {JavascriptFunction}"));
            webView.AddJavascriptInterface(jsBridgeHandler, "jsBridge");
            webView.SetWebChromeClient(new CustomWebChromeClient(this,webView));
            webView.ClearFormData();
            webView.ClearHistory();
            webView.ClearMatches();
            webView.ClearCache(true);
            //webView.DebugDump();
            return webView;
        }

        protected override void ConnectHandler(AndroidWeb.WebView platformView) {
            base.ConnectHandler(platformView);

            if (VirtualView.Source != null) {
                LoadSource(VirtualView.Source, PlatformView);
            }
        }

        protected override void DisconnectHandler(AndroidWeb.WebView platformView) {
            base.DisconnectHandler(platformView);

            VirtualView.Cleanup();

            jsBridgeHandler?.Dispose();
            jsBridgeHandler = null;
        }

        private static void LoadSource(WebViewSource source, AndroidWeb.WebView control) {
//            try {
                if (source is HtmlWebViewSource html) {
                    control.LoadDataWithBaseURL(html.BaseUrl, html.Html, null, "charset=UTF-8", null);
                } else if (source is UrlWebViewSource url) {
                    control.LoadUrl(url.Url);
                }
//            } catch { }
        }
    }
    public class CustomWebChromeClient : AndroidWeb.WebChromeClient {
        public HybridWebViewHandler _handler;
        public AndroidWeb.IValueCallback _callback;
        public CustomWebChromeClient(HybridWebViewHandler handler, AndroidWeb.WebView webview) {
            _handler = handler;
            AndroidWeb.WebView.SetWebContentsDebuggingEnabled(true);
            AndroidWeb.CookieManager cookieManager = AndroidWeb.CookieManager.Instance;
            cookieManager.SetAcceptThirdPartyCookies(webview, true);
        }
        public override bool OnCreateWindow(AndroidWeb.WebView view, bool isDialog, bool isUserGesture, Message resultMsg) {
            var TestResult = view.GetHitTestResult();
            string data = TestResult.Extra;
            Context context = view.Context;
            var uri = AndroidNet.Uri.Parse(data);
            Intent browserIntent = new Intent(Intent.ActionView, uri);
            context.StartActivity(browserIntent);
            return false;
        }
        public void doCallback(AndroidNet.Uri uri) {
            AndroidNet.Uri[] files = new AndroidNet.Uri[1];
            if (uri != null) {
                files[0] = uri;
            } else {
                files = new AndroidNet.Uri[] { };
            }
            _callback.OnReceiveValue(files);
        }
        public override bool OnShowFileChooser(AndroidWeb.WebView webView, AndroidWeb.IValueCallback filePathCallback,
            FileChooserParams fileChooserParams) {
            _callback = filePathCallback;
            MainActivity.handler = doCallback;
            Intent intent = new Intent(Intent.ActionPick);
            intent.SetType("*/*");
            intent.SetAction(Intent.ActionGetContent);
            _handler.Services.GetService<Activity>().StartActivityForResult(Intent.CreateChooser(intent, "Select File to Upload"), 1);
            return true;
        }
    }
    public class JavascriptWebViewClient : AndroidWeb.WebViewClient, AndroidWeb.IValueCallback {
        string _javascript;
        string user = "";
        string pw = "";
        int count = 0;

        public JavascriptWebViewClient(string javascript) {
            _javascript = javascript;
        }
        public void OnReceiveValue(Java.Lang.Object result) {
            if (result != null) {
                string[] parts = result.ToString().Trim('"').Split('|');
                if (parts.Length > 1) {
                    int.TryParse(parts[0], out int hashcode);
                    App.errmessage += $"callback hashcode={hashcode};";
                    if (App.MessageQueue.Any(x => x.Key == hashcode)) {
                        App.MessageQueue[hashcode] = null;
                        App.errmessage += $"MessageQ null hashcode={hashcode};";
                    }
                }
            }
        }
        public override void OnPageStarted(AndroidWeb.WebView view, string url, Bitmap favicon) {
            base.OnPageStarted(view, url, favicon);
            view.EvaluateJavascript(_javascript, null);
            MessagingCenter.Subscribe<App, string>(this, "PushNotification", (sender, arg) => {
                App.errmessage += $"JS arg={arg};";
                MainThread.BeginInvokeOnMainThread(() => view.EvaluateJavascript(
                    $"DotNet.invokeMethod('{App.currentApp.BlazorNamespace}', 'setMessage', '{arg}' );", this));
            });
        }
#if DEBUG
        public override void OnReceivedSslError(global::Android.Webkit.WebView view, AndroidWeb.SslErrorHandler handler, SslError error) {
            handler.Proceed();//this line make ssl error handle so the webview show the page even with certificate errors
        }
#endif
        public override void OnReceivedHttpAuthRequest(global::Android.Webkit.WebView? view, AndroidWeb.HttpAuthHandler? handler, string? host, string? realm) {
            if (user != "") {
                handler.Proceed(user, pw);
                count = 0;
                user = null;
                pw = null;
                return;
            } else if (count == 0) {
                MessagingCenter.Subscribe<Login, string[]>(this, "WindowsAuthen", (sender, arg) => {
                    MessagingCenter.Unsubscribe<Login, string[]>(this, "WindowsAuthen");
                    user = arg[0];
                    pw = arg[1];
                    view.Reload();
                });
                Microsoft.Maui.Controls.Application.Current.MainPage.Navigation.PushModalAsync(new Login());
            }
            count++;
        }
        public override bool ShouldOverrideUrlLoading(AndroidWeb.WebView view, AndroidWeb.IWebResourceRequest request) {
            if (!string.IsNullOrWhiteSpace(request.Url.Query) && request.Url.Query.Contains("attachment")) { 
                string _javascript = @"javascript: var xhr = new XMLHttpRequest();" +
                    "xhr.open('GET', '" + request.Url.ToString() + "', true);" +
                    "xhr.responseType = 'blob';" +
                    "xhr.onload = function(e) {" +
                    "    if (this.status == 200) {" +
                    "        var blobPdf = this.response;" +
                    "        var fileName = this.getResponseHeader('content-disposition').split('filename=')[1].split(';')[0];" +
                    "        var mimetype = this.getResponseHeader('content-disposition');" +
                    "        var reader = new FileReader();" +
                    "        reader.readAsDataURL(blobPdf);" +
                    "        reader.onloadend = function() {" +
                    "            base64data = reader.result.split(';base64,')[1];" +
                    "            savetoMAUI(base64data, mimetype, fileName)" +
                    "        }" +
                    "    }" +
                    "};" +
                    "xhr.send();";
                view.LoadUrl(_javascript);
                return true;
            } else {
                return false;
            }
        }
    }

    public class JSBridge : Java.Lang.Object {
        readonly WeakReference<HybridWebViewHandler> hybridWebViewRenderer;

        internal JSBridge(HybridWebViewHandler hybridRenderer) {
            hybridWebViewRenderer = new WeakReference<HybridWebViewHandler>(hybridRenderer);
        }

        [AndroidWeb.JavascriptInterface]
        [Export("invokeAction")]
        public void InvokeAction(string data) {
            HybridWebViewHandler hybridRenderer;
            if (hybridWebViewRenderer != null && hybridWebViewRenderer.TryGetTarget(out hybridRenderer)) {
                hybridRenderer.VirtualView.InvokeAction(data);
            }
        }
    }
}
