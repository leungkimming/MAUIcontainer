using Android.App;
using Android.OS;
using Android.Runtime;
using Firebase;
using Plugin.FirebasePushNotification;

namespace MAUIcontainer;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override void OnCreate() {
        base.OnCreate();

        //Set the default notification channel for your app when running Android Oreo
        if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O) {
            //Change for your default notification channel id here
            FirebasePushNotificationManager.DefaultNotificationChannelId = "FirebasePushNotificationChannel";

            //Change for your default notification channel name here
            FirebasePushNotificationManager.DefaultNotificationChannelName = "General";
        }
        FirebaseApp.InitializeApp(this.ApplicationContext);
        //If debug you should set to 'true' to reset token once and write down the token
#if DEBUG
        FirebasePushNotificationManager.Initialize(this, false);
#else
               FirebasePushNotificationManager.Initialize(this,false);
#endif

        //Handle notification when app is closed here
        CrossFirebasePushNotification.Current.OnNotificationReceived += (s, p) => {
        };
    }
}
