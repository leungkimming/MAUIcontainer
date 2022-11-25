using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace MAUIcontainer {
    public class DevHttpsConnectionHelper {
        public DevHttpsConnectionHelper() {
            LazyHttpClient = new Lazy<HttpClient>(() => new HttpClient(GetPlatformMessageHandler()));
        }

        private Lazy<HttpClient> LazyHttpClient;
        public HttpClient HttpClient => LazyHttpClient.Value;


        public HttpMessageHandler GetPlatformMessageHandler() {
            var handler = new NSUrlSessionHandler {
#if DEBUG
                TrustOverrideForUrl = IsHttpsLocalhost
#endif
            };
            return handler;
        }
        public bool IsHttpsLocalhost(NSUrlSessionHandler sender, string url, Security.SecTrust trust) {
            return true;
        }
    }
}
