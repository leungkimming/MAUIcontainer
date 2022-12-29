using Foundation;
using Microsoft.Identity.Client;
using UIKit;
using Plugin.Firebase.CloudMessaging;

namespace MAUIcontainer
{

    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {

	    protected override MauiApp CreateMauiApp() 
        {
            return MauiProgram.CreateMauiApp();
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            //AppCenter.Start("1518793c-b3c7-47e9-a6e8-3c8447489246", typeof(Analytics), typeof(Crashes));
            return base.FinishedLaunching(application, launchOptions);
        }
        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options) {
            if (url.AbsoluteString.StartsWith("heart://")) {
                return OpenDeepLink(url.AbsoluteString);
            } else {
                AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);
                return base.OpenUrl(app, url, options);
            }
        }
        private bool OpenDeepLink(string url) {
            App.errmessage += url;
            var x = url.IndexOf('/', 8);
            if (x > 0) {
                var dlinkdata = new Dictionary<string, string> {
                    { "App", url.Substring(8, x - 8) },
                    { "Message", url }
                };
                FCMNotification dlinkFCM = new FCMNotification(null, "DeepLink", null, dlinkdata);
                //var dlinkarg = new Plugin.Firebase.CloudMessaging.EventArgs.FCMNotificationTappedEventArgs(dlinkMess);
                CrossFirebaseCloudMessaging.Current.OnNotificationReceived(dlinkFCM);
            }
            return true;
        }

    }

}

