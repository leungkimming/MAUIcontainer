using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Webkit;
using Java.IO;
using Android.Content;
using Newtonsoft.Json;
using Microsoft.Identity.Client;

namespace WebViewHostExample.Platforms.Android {
    [Activity(Exported = true)]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
        DataHost = "auth",
        DataScheme = "msal48ce7928-5eea-415c-a209-35e39bef6b2e")]
    public class MsalActivity : BrowserTabActivity {
    }
}
