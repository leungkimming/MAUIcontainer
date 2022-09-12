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

namespace WebViewHostExample;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity {
    public delegate void uploadFile(Android.Net.Uri uri);
    public static uploadFile handler = null;
    public static MainActivity MainActivityInstance { get; private set; }
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
        MainActivityInstance = this;
    }
}
