using CoreGraphics;
using Foundation;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using System;
using System.Management;
using System.Runtime.Versioning;
using WebKit;
using WebViewHostExample.Controls;

namespace WebViewHostExample.Platforms.iOS.Renderers
{
    public class HybridWebViewHandler : ViewHandler<IHybridWebView, WKWebView>
    {
        public static PropertyMapper<IHybridWebView, HybridWebViewHandler> HybridWebViewMapper = new PropertyMapper<IHybridWebView, HybridWebViewHandler>(ViewHandler.ViewMapper);
        
        const string JavaScriptFunction = "function invokeCSharpAction(data){window.webkit.messageHandlers.invokeAction.postMessage(data);}";

        private WKUserContentController userController;
        private JSBridge jsBridgeHandler;
        public string Org_url;
        WKWebViewDelegate _delegate;

        public HybridWebViewHandler() : base(HybridWebViewMapper) {
        }

        private void VirtualView_SourceChanged(object sender, SourceChangedEventArgs e) {
            LoadSource(e.Source, PlatformView);
            if (e.Source is UrlWebViewSource url) {
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
            _delegate = new WKWebViewDelegate();
            webView.NavigationDelegate = _delegate;

            return webView;            
        }

        protected override void ConnectHandler(WKWebView platformView) {
            base.ConnectHandler(platformView);

            if (VirtualView.Source != null)
            {
                LoadSource(VirtualView.Source, PlatformView);
            }

            VirtualView.SourceChanged += VirtualView_SourceChanged;
        }

        protected override void DisconnectHandler(WKWebView platformView) {
            base.DisconnectHandler(platformView);

            VirtualView.SourceChanged -= VirtualView_SourceChanged;

            userController.RemoveAllUserScripts();
            userController.RemoveScriptMessageHandler("invokeAction");
        
            jsBridgeHandler?.Dispose();
            jsBridgeHandler = null;
        }


        private static void LoadSource(WebViewSource source, WKWebView control) {
            if (source is HtmlWebViewSource html)
            {
                control.LoadHtmlString(html.Html, new NSUrl(html.BaseUrl ?? "http://localhost", true));
            }
            else if (source is UrlWebViewSource url)
            {
                control.LoadRequest(new NSUrlRequest(new NSUrl(url.Url)));
            }
        }
    }
    public class WKWebViewDelegate : WKNavigationDelegate {
        string user = "";
        string pw = "";
        int count = 0;
        public string org_Url { get; set; }

        [Foundation.Export("webView:didReceiveAuthenticationChallenge:completionHandler:")]
        public override void DidReceiveAuthenticationChallenge(WKWebView webView, NSUrlAuthenticationChallenge challenge,
            Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential> completionHandler) {
            bool isDebug = false;
#if DEBUG
            isDebug = true;
#endif
            if (isDebug && (challenge.ProtectionSpace.AuthenticationMethod == NSUrlProtectionSpace.AuthenticationMethodServerTrust)) {
                completionHandler(NSUrlSessionAuthChallengeDisposition.UseCredential,
                    NSUrlCredential.FromTrust(trust: challenge.ProtectionSpace.ServerSecTrust));
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
                    ((navigationAction.NavigationType == WKNavigationType.Other) && (url != org_Url))) {
                decisionHandler(WKNavigationActionPolicy.Cancel);
                Browser.Default.OpenAsync(navigationAction.Request.Url, BrowserLaunchMode.SystemPreferred);
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
