using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Net.Http;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Webkit;
using Android.Widget;
using Android.Net;
using Java.Interop;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;
using WebViewHostExample.Controls;
using static Android.Net.Http.SslCertificate;
using Microsoft.Extensions.DependencyInjection;
using Java.Net;
using Java.Lang;
using Android.Views;
using Microsoft.Maui.Controls;

namespace WebViewHostExample.Platforms.Droid.Renderers {
    public class HybridWebViewHandler : ViewHandler<IHybridWebView, Android.Webkit.WebView> {
        public static PropertyMapper<IHybridWebView, HybridWebViewHandler> HybridWebViewMapper = new PropertyMapper<IHybridWebView, HybridWebViewHandler>(ViewHandler.ViewMapper);

        const string JavascriptFunction = "function invokeMAUIAction(data){jsBridge.invokeAction(data);}";

        private JSBridge jsBridgeHandler;
        public JavascriptWebViewClient _JSclient;

        public HybridWebViewHandler() : base(HybridWebViewMapper) {
        }

        private void VirtualView_SourceChanged(object sender, SourceChangedEventArgs e) {
            LoadSource(e.Source, PlatformView);
            if (e.Source is UrlWebViewSource url) {
                _JSclient.org_Url = url.Url;
            }
        }

        protected override Android.Webkit.WebView CreatePlatformView() {
            var webView = new Android.Webkit.WebView(Context);
            jsBridgeHandler = new JSBridge(this);

            webView.Settings.JavaScriptEnabled = true;

            webView.Settings.SetSupportMultipleWindows(true);
            webView.Settings.SetEnableSmoothTransition(true);
            webView.Settings.DomStorageEnabled = true;
            webView.Settings.AllowFileAccessFromFileURLs = true;
            webView.Settings.DefaultTextEncodingName = "UTF-8";
            _JSclient = new JavascriptWebViewClient($"javascript: {JavascriptFunction}");
            webView.SetWebViewClient(_JSclient);
            webView.AddJavascriptInterface(jsBridgeHandler, "jsBridge");
            webView.SetWebChromeClient(new CustomWebChromeClient(this));
            return webView;
        }

        protected override void ConnectHandler(Android.Webkit.WebView platformView) {
            base.ConnectHandler(platformView);

            if (VirtualView.Source != null) {
                LoadSource(VirtualView.Source, PlatformView);
            }

            VirtualView.SourceChanged += VirtualView_SourceChanged;
        }

        protected override void DisconnectHandler(Android.Webkit.WebView platformView) {
            base.DisconnectHandler(platformView);

            VirtualView.SourceChanged -= VirtualView_SourceChanged;
            VirtualView.Cleanup();

            jsBridgeHandler?.Dispose();
            jsBridgeHandler = null;
        }

        private static void LoadSource(WebViewSource source, Android.Webkit.WebView control) {
            try {
                if (source is HtmlWebViewSource html) {
                    control.LoadDataWithBaseURL(html.BaseUrl, html.Html, null, "charset=UTF-8", null);
                } else if (source is UrlWebViewSource url) {
                    control.LoadUrl(url.Url);
                }
            } catch { }
        }
    }
    public class CustomWebChromeClient : WebChromeClient, IValueCallback {
        public HybridWebViewHandler _handler;
        public IValueCallback _callback;
        public CustomWebChromeClient(HybridWebViewHandler handler) {
            _handler = handler;
        }
        public override bool OnCreateWindow(Android.Webkit.WebView view, bool isDialog, bool isUserGesture, Message resultMsg) {
            var TestResult = view.GetHitTestResult();
            string data = TestResult.Extra;
            Context context = view.Context;
            var uri = Android.Net.Uri.Parse(data);
            Intent browserIntent = new Intent(Intent.ActionView, uri);
            context.StartActivity(browserIntent);
            return false;
        }
        public void doCallback(Android.Net.Uri uri) {
            Android.Net.Uri[] files = new Android.Net.Uri[1];
            if (uri != null) {
                files[0] = uri;
            } else {
                files = new Android.Net.Uri[] { };
            }
            _callback.OnReceiveValue(files);
        }
        public override bool OnShowFileChooser(Android.Webkit.WebView webView, IValueCallback filePathCallback,
            FileChooserParams fileChooserParams) {

            webView.EvaluateJavascript(@"DotNet.invokeMethod('Client', 'getEnvironment', 1);", this);
            _callback = filePathCallback;
            MainActivity.handler = doCallback;
            Intent intent = new Intent(Intent.ActionPick);
            intent.SetType("*/*");
            intent.SetAction(Intent.ActionGetContent);
            _handler.Services.GetService<Activity>().StartActivityForResult(Intent.CreateChooser(intent, "Select File to Upload"), 1);
            return true;
        }
        public void OnReceiveValue(Java.Lang.Object result) {
            Microsoft.Maui.Controls.Application.Current.MainPage
                .DisplayAlert("Warning", $"You are uploading file to the ({result.ToString().Trim('"')}) environment", "Ok");
        }
    }
    public class JavascriptWebViewClient : WebViewClient {
        string _javascript;
        string user = "";
        string pw = "";
        int count = 0;
        public string org_Url { get; set; }

        public JavascriptWebViewClient(string javascript) {
            _javascript = javascript;
        }
        public override void OnPageStarted(Android.Webkit.WebView view, string url, Bitmap favicon) {
            base.OnPageStarted(view, url, favicon);
            view.EvaluateJavascript(_javascript, null);
        }
#if DEBUG
        public override void OnReceivedSslError(global::Android.Webkit.WebView view, SslErrorHandler handler, SslError error) {
            handler.Proceed();//this line make ssl error handle so the webview show the page even with certificate errors
        }
#endif
        public override void OnReceivedHttpAuthRequest(global::Android.Webkit.WebView? view, HttpAuthHandler? handler, string? host, string? realm) {
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
        public override bool ShouldOverrideUrlLoading(Android.Webkit.WebView view, Android.Webkit.IWebResourceRequest request) {
            if (request.Url.ToString() != org_Url) {
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

        [JavascriptInterface]
        [Export("invokeAction")]
        public void InvokeAction(string data) {
            HybridWebViewHandler hybridRenderer;
            if (hybridWebViewRenderer != null && hybridWebViewRenderer.TryGetTarget(out hybridRenderer)) {
                hybridRenderer.VirtualView.InvokeAction(data);
            }
        }
    }
}
