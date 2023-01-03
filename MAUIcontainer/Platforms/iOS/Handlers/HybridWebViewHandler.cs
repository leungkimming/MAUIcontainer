using CoreGraphics;
using Foundation;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using System;
using System.Runtime.Versioning;
using WebKit;
using MAUIcontainer.Controls;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using System.Text;

namespace MAUIcontainer.Platforms.iOS.Renderers {
    public class HybridWebViewHandler : ViewHandler<IHybridWebView, WKWebView> {
        public static PropertyMapper<IHybridWebView, HybridWebViewHandler> HybridWebViewMapper = new PropertyMapper<IHybridWebView, HybridWebViewHandler>(ViewHandler.ViewMapper) {
            [nameof(IHybridWebView.Source)] = MapSource
        };

        const string JavaScriptFunction = "function invokeMAUIAction(data){window.webkit.messageHandlers.invokeAction.postMessage(data);}";

        private WKUserContentController userController;
        private JSBridge jsBridgeHandler;
        static WKWebViewDelegate _delegate;

        public HybridWebViewHandler() : base(HybridWebViewMapper) {
        }
        static void MapSource(HybridWebViewHandler handler, IHybridWebView entry) {
            LoadSource(entry.Source, handler.PlatformView);
            if (entry.Source is UrlWebViewSource url) {
                _delegate.org_Url = url.Url;
            }
        }

        protected override WKWebView CreatePlatformView() {

            jsBridgeHandler = new JSBridge(this);
            userController = new WKUserContentController();

            var script = new WKUserScript(new NSString(JavaScriptFunction), WKUserScriptInjectionTime.AtDocumentEnd, false);

            userController.AddUserScript(script);
            userController.AddScriptMessageHandler(jsBridgeHandler, "invokeAction");

            var config = new WKWebViewConfiguration { UserContentController = userController };
            var webView = new WKWebView(CGRect.Empty, config);
            _delegate = new WKWebViewDelegate(webView);
            webView.NavigationDelegate = _delegate;

            return webView;
        }

        protected override void ConnectHandler(WKWebView platformView) {
            base.ConnectHandler(platformView);

            if (VirtualView.Source != null) {
                LoadSource(VirtualView.Source, PlatformView);
            }
        }

        protected override void DisconnectHandler(WKWebView platformView) {
            base.DisconnectHandler(platformView);

            userController.RemoveAllUserScripts();
            userController.RemoveScriptMessageHandler("invokeAction");

            jsBridgeHandler?.Dispose();
            jsBridgeHandler = null;
        }


        private static void LoadSource(WebViewSource source, WKWebView control) {
            if (source is HtmlWebViewSource html) {
                control.LoadHtmlString(html.Html, new NSUrl(html.BaseUrl ?? "http://localhost", true));
            } else if (source is UrlWebViewSource url) {
                control.LoadRequest(new NSUrlRequest(new NSUrl(url.Url)));
            }
        }
    }
    public class WKWebViewDelegate : WKNavigationDelegate {
        string user = "";
        string pw = "";
        int count = 0;
        public string org_Url { get; set; }
        public WKWebViewDelegate(WKWebView view) {
            WKJavascriptEvaluationResult callback = (NSObject result, NSError err) => {
                if (result != null) {
                    string[] parts = result.ToString().Split('|');
                    //System.Diagnostics.Debug.WriteLine("return:"+parts[1]);
                    if (parts.Length > 1) {
                        int.TryParse(parts[0], out int hashcode);
                        App.errmessage += $"callback hashcode={hashcode};";
                        if (App.MessageQueue.Any(x => x.Key == hashcode)) {
                            App.MessageQueue[hashcode] = null;
                            App.errmessage += $"MessageQ null hashcode={hashcode};";
                        }
                    }
                }
            };
            MessagingCenter.Subscribe<App, string>(this, "PushNotification", (sender, arg) => {
                App.errmessage += $"JS arg={arg.Substring(0, 15)};";
                //System.Diagnostics.Debug.WriteLine("send:"+s_arg);
                MainThread.BeginInvokeOnMainThread(() => view.EvaluateJavaScript(
                    $"DotNet.invokeMethod('{App.currentApp.BlazorNamespace}', 'setMessage', '{arg}' );", callback));
            });
        }
        
        [Foundation.Export("webView:didReceiveAuthenticationChallenge:completionHandler:")]
        public override void DidReceiveAuthenticationChallenge(WKWebView webView, NSUrlAuthenticationChallenge challenge,
            Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential> completionHandler) {
            if (challenge.ProtectionSpace.AuthenticationMethod == NSUrlProtectionSpace.AuthenticationMethodServerTrust) {
#if DEBUG
                completionHandler(NSUrlSessionAuthChallengeDisposition.UseCredential,
                    NSUrlCredential.FromTrust(trust: challenge.ProtectionSpace.ServerSecTrust));
#else
                completionHandler(NSUrlSessionAuthChallengeDisposition.PerformDefaultHandling, null);
#endif
            } else if (challenge.ProtectionSpace.AuthenticationMethod == NSUrlProtectionSpace.AuthenticationMethodNTLM) {
                if (user != "") {
                    NSUrlCredential credentials = new NSUrlCredential(user, pw, persistence: NSUrlCredentialPersistence.ForSession);
                    challenge.Sender?.UseCredential(credentials, challenge);
                    completionHandler(NSUrlSessionAuthChallengeDisposition.UseCredential, credentials);
                    count = 0;
                    user = null;
                    pw = null;
                    return;
                } else if (count == 0) {
                    MessagingCenter.Subscribe<Login, string[]>(this, "WindowsAuthen", (sender, arg) => {
                        MessagingCenter.Unsubscribe<Login, string[]>(this, "WindowsAuthen");
                        user = arg[0];
                        pw = arg[1];
                        webView.Reload();
                    });
                    Microsoft.Maui.Controls.Application.Current.MainPage.Navigation.PushModalAsync(new Login());
                    completionHandler(NSUrlSessionAuthChallengeDisposition.CancelAuthenticationChallenge, null);
                }
                count++;
            } else {
                completionHandler(NSUrlSessionAuthChallengeDisposition.CancelAuthenticationChallenge, null);
            }
        }
        [SupportedOSPlatform("ios14.5")]
        public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler) {
            var url = navigationAction.Request.Url.AbsoluteString;
            if ((navigationAction.NavigationType == WKNavigationType.LinkActivated) ||
                    navigationAction.ShouldPerformDownload ||
                    ((navigationAction.NavigationType == WKNavigationType.Other) && (!url.StartsWith(org_Url)))) {
                decisionHandler(WKNavigationActionPolicy.Cancel);
                if (url.ToUpper().StartsWith("HTTPS://")) { 
                    Browser.Default.OpenAsync(navigationAction.Request.Url, BrowserLaunchMode.SystemPreferred);
                }
            } else
                decisionHandler(WKNavigationActionPolicy.Allow);
        }
    }
    public class JSBridge : NSObject, IWKScriptMessageHandler {
        readonly WeakReference<HybridWebViewHandler> hybridWebViewRenderer;

        internal JSBridge(HybridWebViewHandler hybridRenderer) {
            hybridWebViewRenderer = new WeakReference<HybridWebViewHandler>(hybridRenderer);
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message) {
            HybridWebViewHandler hybridRenderer;

            if (hybridWebViewRenderer.TryGetTarget(out hybridRenderer)) {
                hybridRenderer.VirtualView?.InvokeAction(message.Body.ToString());
            }
        }
    }
}
