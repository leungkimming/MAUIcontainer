using Foundation;
using Microsoft.Identity.Client;
using UIKit;

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
            AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);
            return base.OpenUrl(app, url, options);
        }

    }

}

