using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Plugin.Fingerprint.Abstractions;
using WebViewHostExample;
using WebViewHostExample.Controls;

namespace WebViewHostExample.Common {
    public static partial class BlazorCallHelper {
        [SupportedOSPlatform("android31.0")]
        public static void getFingerPrint(string reason) {
            var message = "";
            MainActivity.MainActivityInstance.RunOnUiThread(async () => {
                CancellationTokenSource _cancel = new CancellationTokenSource(); //new CancellationTokenSource(TimeSpan.FromSeconds(10))
                var dialogConfig = new AuthenticationRequestConfiguration("MAUIContainer", reason) {
                    CancelTitle = "Cancel",
                    FallbackTitle = "Use Passcode",
                    AllowAlternativeAuthentication = true,
                    ConfirmationRequired = true
                };
                dialogConfig.HelpTexts.MovedTooFast = "Finger moving too fast";
                FingerprintAuthenticationResult result =
                    await Plugin.Fingerprint.CrossFingerprint.Current.AuthenticateAsync(
                        dialogConfig, _cancel.Token);

                message = "Authenticated";
                if (!result.Authenticated) {
                    message = $"{result.Status}: {result.ErrorMessage}";
                }
                callback(promiseId, message);
            });
        }
    }
}
