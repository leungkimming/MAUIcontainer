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

namespace WebViewHostExample.Platforms.Droid.Renderers {
    public class HybridWebViewHandler : ViewHandler<IHybridWebView, Android.Webkit.WebView> {
        public static PropertyMapper<IHybridWebView, HybridWebViewHandler> HybridWebViewMapper = new PropertyMapper<IHybridWebView, HybridWebViewHandler>(ViewHandler.ViewMapper);

        const string JavascriptFunction = "function invokeCSharpAction(data){jsBridge.invokeAction(data);}";

        private JSBridge jsBridgeHandler;

        public HybridWebViewHandler() : base(HybridWebViewMapper) {
        }

        private void VirtualView_SourceChanged(object sender, SourceChangedEventArgs e) {
            LoadSource(e.Source, PlatformView);
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
            webView.SetWebViewClient(new JavascriptWebViewClient($"javascript: {JavascriptFunction}"));
            webView.AddJavascriptInterface(jsBridgeHandler, "jsBridge");
            webView.SetDownloadListener(new CustomDownloadListener(webView));
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
    public class CustomWebChromeClient : WebChromeClient {
        public HybridWebViewHandler _handler;
        public IValueCallback _callback;
        public CustomWebChromeClient(HybridWebViewHandler handler) {
            _handler = handler;
        }
        public void doCallback(Android.Net.Uri uri) {
            var files = new Android.Net.Uri[] { uri };
            _callback.OnReceiveValue(files);
        }
        public override bool OnShowFileChooser(Android.Webkit.WebView webView, IValueCallback filePathCallback,
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

    public class JavascriptWebViewClient : WebViewClient {
        string _javascript;
        string user = "";
        string pw = "";
        int count = 0;

        public JavascriptWebViewClient(string javascript) {
            _javascript = javascript;
        }

        public override void OnPageStarted(Android.Webkit.WebView view, string url, Bitmap favicon) {
            base.OnPageStarted(view, url, favicon);
            view.EvaluateJavascript(_javascript, null);
        }
        public override void OnReceivedSslError(global::Android.Webkit.WebView view, SslErrorHandler handler, SslError error) {
            handler.Proceed();//this line make ssl error handle so the webview show the page even with certificate errors
        }

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

        [JavascriptInterface]
        [Export("saveBase64toDownload")]
        [Obsolete]
        public async void saveBase64toDownload(string base64data) {
            string currentDateTime = DateTime.Now.ToString("yyyyMMddHHmmssff");
            string minetype = base64data.Substring(5, (base64data.IndexOf(";base64,") - 5));
            string base64 = base64data.Substring(base64data.IndexOf(";base64,") + 8);
            MimeTypeMap mimemap = MimeTypeMap.Singleton;
            string ext = mimemap.GetExtensionFromMimeType(minetype);
            if (minetype == "application/x-zip-compressed") ext = "zip";
            string dwldsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(
                Android.OS.Environment.DirectoryDownloads) + "/" + currentDateTime + "." + ext;
            byte[] pdfAsBytes = Base64.Decode(base64, 0);
            File.WriteAllBytes(dwldsPath, pdfAsBytes);
            //Toast.MakeText(Android.App.Application.Context, "File downloaded!", ToastLength.Short).Show();
            bool answer = await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
                "File downloaded", "Would you like to browse the Download Folder?", "Yes", "No");
            if (answer) {
                Intent intent = new Intent(Intent.ActionPick);
                intent.SetType("*/*");
                intent.SetAction(Intent.ActionGetContent);
                HybridWebViewHandler hybridRenderer;
                if (hybridWebViewRenderer != null && hybridWebViewRenderer.TryGetTarget(out hybridRenderer)) {
                    hybridRenderer.Services.GetService<Activity>().StartActivityForResult(Intent.CreateChooser(intent, "Open Downloaded File"), 1);
                }
            }
        }
    }

    public class CustomDownloadListener : Java.Lang.Object, IDownloadListener {
        public Android.Webkit.WebView _webview;
        public CustomDownloadListener(Android.Webkit.WebView view) {
            _webview = view;
        }
        public void OnDownloadStart(string url, string userAgent, string contentDisposition, string mimetype, long contentLength) {
            try {
                if (url.StartsWith("blob")) {
                    string _javascript = @"javascript: var xhr = new XMLHttpRequest();" +
                        "xhr.open('GET', '" + url + "', true);" +
                        "xhr.setRequestHeader('Content-type','" + mimetype + "');" +
                        "xhr.responseType = 'blob';" +
                        "xhr.onload = function(e) {" +
                        "    if (this.status == 200) {" +
                        "        var blobPdf = this.response;" +
                        "        var reader = new FileReader();" +
                        "        reader.readAsDataURL(blobPdf);" +
                        "        reader.onloadend = function() {" +
                        "            base64data = reader.result;" +
                        "            jsBridge.saveBase64toDownload(base64data); " +
                        "        }" +
                        "    }" +
                        "};" +
                        "xhr.send();";
                    _webview.LoadUrl(_javascript);
                } else {
                    DownloadManager.Request request = new DownloadManager.Request(Android.Net.Uri.Parse(url));
                    //request.AllowScanningByMediaScanner();
                    request.SetNotificationVisibility(DownloadVisibility.VisibleNotifyCompleted);
                    // if this path is not create, we can create it.
                    string thmblibrary = FileSystem.AppDataDirectory + "/download";
                    if (!Directory.Exists(thmblibrary))
                        Directory.CreateDirectory(thmblibrary);
                    request.SetDestinationInExternalFilesDir(Android.App.Application.Context, FileSystem.AppDataDirectory, "download");
                    DownloadManager dm = (DownloadManager)Android.App.Application.Context.GetSystemService(Android.App.Application.DownloadService);
                    dm.Enqueue(request);
                }
            } catch (Exception) {
                throw;
            }
        }
    }
}
