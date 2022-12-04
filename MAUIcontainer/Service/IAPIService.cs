using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MAUIcontainer {
    public interface IAPIService {
        public void RefreshToken(HttpClient request, RequestDto requestDto);
        public void UploadFileRequest(FileDto file, RequestDto requestDto);
        public MyAppsResponse GetMyApps();
    }
}
