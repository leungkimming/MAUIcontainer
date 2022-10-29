using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Webkit;
using Java.IO;
using Android.Content;
using Newtonsoft.Json;
using Android.Runtime;
using Microsoft.Identity.Client;
using Plugin.Fingerprint;
using Android.Views;
using Plugin.Fingerprint.Abstractions;
using Plugin.Firebase.CloudMessaging;

namespace MAUIcontainer;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity {
    public delegate void uploadFile(Android.Net.Uri uri);
    public static uploadFile handler = null;

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
        if ((requestCode == 1) && handler != null) {
            if ((resultCode == Result.Ok) && (data != null)) {
                Android.Net.Uri uri = data.Data;
                handler(uri);
            } else {
                handler(null);
            }
            handler = null;
        }
        AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode, resultCode, data);
    }
    protected override void OnCreate(Bundle savedInstanceState) {
        base.OnCreate(savedInstanceState);
        CrossFingerprint.SetCurrentActivityResolver(() => this);
        HandleIntent(Intent);
        CreateNotificationChannelIfNeeded();
    }
    protected override void OnNewIntent(Intent intent) {
        base.OnNewIntent(intent);
        HandleIntent(intent);
    }

    private static void HandleIntent(Intent intent) {
        FirebaseCloudMessagingImplementation.OnNewIntent(intent);
    }

    private void CreateNotificationChannelIfNeeded() {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O) {
            CreateNotificationChannel();
        }
    }

    private void CreateNotificationChannel() {
        var channelId = $"{PackageName}.general";
        var notificationManager = (NotificationManager)GetSystemService(NotificationService);
        var channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);
        notificationManager.CreateNotificationChannel(channel);
        FirebaseCloudMessagingImplementation.ChannelId = channelId;
        //FirebaseCloudMessagingImplementation.SmallIconRef = Resource.Drawable.ic_push_small;
    }
}
