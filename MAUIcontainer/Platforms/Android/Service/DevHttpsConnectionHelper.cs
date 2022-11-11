using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Security;
using RestSharp;

namespace MAUIcontainer {

    public class DevHttpsConnectionHelper {
        public DevHttpsConnectionHelper() {
            LazyHttpClient = new Lazy<HttpClient>(() => new HttpClient(GetPlatformMessageHandler()));
        }

        private Lazy<HttpClient> LazyHttpClient;
        public HttpClient HttpClient => LazyHttpClient.Value;


        public HttpMessageHandler GetPlatformMessageHandler() {
            var handler = new CustomAndroidMessageHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => {
                if (cert != null && cert.Issuer.Equals("CN=localhost"))
                    return true;
                return errors == SslPolicyErrors.None;
            };
            return handler;
        }
        internal sealed class CustomAndroidMessageHandler : Xamarin.Android.Net.AndroidMessageHandler {
            protected override Javax.Net.Ssl.IHostnameVerifier GetSSLHostnameVerifier(Javax.Net.Ssl.HttpsURLConnection connection)
                => new CustomHostnameVerifier();

            private sealed class CustomHostnameVerifier : Java.Lang.Object, Javax.Net.Ssl.IHostnameVerifier {
                public bool Verify(string hostname, Javax.Net.Ssl.ISSLSession session) {
                    return true;
                }
            }
        }
    }
}
