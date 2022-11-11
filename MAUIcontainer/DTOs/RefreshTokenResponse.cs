using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUIcontainer {
    public class RefreshTokenResponse {
        public DateTime? TokenExpiry { get; set; }
        public string sRefreshToken { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string CSRF_TOKEN { get; set; }
    }
}
