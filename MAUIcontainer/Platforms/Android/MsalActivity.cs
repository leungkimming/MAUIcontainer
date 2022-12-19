using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Webkit;
using Java.IO;
using Android.Content;
using Newtonsoft.Json;
using Microsoft.Identity.Client;

namespace MAUIcontainer.Platforms.Android {
    [Activity(Exported = true)]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
        DataHost = "auth",
        DataSchemes = new[] { 
            "msal48ce7928-5eea-415c-a209-35e39bef6b2e",
            "msalfadc4ccc-c090-4bab-9e8e-eb00f5f6a3a4" }
    )]
    public class MsalActivity : BrowserTabActivity {
    }
}
