using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebViewHostExample {
    public class AuthService {
        private readonly IPublicClientApplication authenticationClient;
        public AuthService() {
            authenticationClient = PublicClientApplicationBuilder.Create(WebViewHostExample.Common.Constants.ApplicationId)
                //.WithB2CAuthority(Constants.AuthoritySignIn) // uncomment to support B2C
#if WINDOWS
            .WithRedirectUri("http://localhost");
#else
                .WithRedirectUri($"msal{WebViewHostExample.Common.Constants.ApplicationId}://auth")
#endif
#if IOS
                .WithIosKeychainSecurityGroup("microsoft.adalcache")
#endif
            .Build();
        }
        public async Task<string> GetAccessToken(CancellationToken cancellationToken) {
            try {
                var result = authenticationClient
                    .UserTokenCache;
                return "";
            } catch (MsalClientException) {
                return null;
            }
        }
        public async Task<AuthenticationResult> LoginAsync(CancellationToken cancellationToken) {
            AuthenticationResult result;
            try {
                var accounts=await authenticationClient.GetAccountsAsync();
                bool tryInteractiveLogin = false;
                try {
                    result = await authenticationClient
                        .AcquireTokenSilent(WebViewHostExample.Common.Constants.Scopes, accounts.FirstOrDefault())
                        .ExecuteAsync(cancellationToken);
                    return result;
                } catch (MsalUiRequiredException) {
                    tryInteractiveLogin = true;
                } catch (Exception) {
                    tryInteractiveLogin = true;
                }
                if (tryInteractiveLogin) {
                    result = await authenticationClient

                    .AcquireTokenInteractive(WebViewHostExample.Common.Constants.Scopes)

                    .WithPrompt(Prompt.ForceLogin)
#if ANDROID
                    .WithParentActivityOrWindow(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity)
#endif
#if WINDOWS
		.WithUseEmbeddedWebView(false)				
#endif
                    .ExecuteAsync(cancellationToken);
                    return result;
                }
                return null;
            } catch (MsalClientException e) {
                Console.WriteLine(e.Message + e.StackTrace);
                return null;
            }
        }
        public async Task<AuthenticationResult> LoginStatus() {
            AuthenticationResult result;
            try {
                var accounts = await authenticationClient.GetAccountsAsync();
                result = await authenticationClient
                    .AcquireTokenSilent(WebViewHostExample.Common.Constants.Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync(CancellationToken.None);
                return result;
            } catch (Exception e) { }
            return null;
        }
        public async void LogoutAsync() {
            var accounts = await authenticationClient.GetAccountsAsync();
            foreach (var acct in accounts) {
                await authenticationClient.RemoveAsync(acct).ConfigureAwait(false);
            }
        }
    }
}
