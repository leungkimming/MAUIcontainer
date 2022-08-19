using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Webkit;
using Java.IO;
using Android.Content;
using Newtonsoft.Json;

namespace WebViewHostExample;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity {
    public delegate void uploadFile(Android.Net.Uri uri);
    public static uploadFile handler = null;
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
        if ((requestCode == 1) && (resultCode == Result.Ok) && (data != null) && handler != null) {
            Android.Net.Uri uri = data.Data;
            handler(uri);
            handler = null;
        }
    }
}
