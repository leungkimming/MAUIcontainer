using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebViewHostExample.Common {
    public static class Constants {
        /// <summary>
        /// The base URI for the Datasync service.
        /// </summary>
        public static string ServiceUri = "https://demo-datasync-quickstart.azurewebsites.net";

        /// <summary>
        /// The application (client) ID for the native app within Azure Active Directory
        /// </summary>
        public static string ApplicationId = "48ce7928-5eea-415c-a209-35e39bef6b2e";

        /// <summary>
        /// The list of scopes to request
        /// </summary>
        public static string[] Scopes = new[] {
          "api://b496211d-46e0-4fce-9859-d8c22b4b3507/APIAccess"
      };
    }
}
