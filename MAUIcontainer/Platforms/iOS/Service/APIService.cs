using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using MAUIcontainer.Common;
using Newtonsoft.Json;

namespace MAUIcontainer {
    public static partial class APIService {
        private static void RefreshToken(RestRequest request, RequestDto requestDto) {
            RestClient restClient = new RestClient($"https://{requestDto.Domain}/Login?force=false");
#if DEBUG
            restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
#endif
            RestRequest restRequest= new RestRequest();
            restRequest.Method = Method.GET;
            restRequest.AddHeader("authorization", "Bearer " + BlazorCallHelper.getAADToken());
            IRestResponse restResponse=restClient.Execute(restRequest);

            var refreshToken=JsonConvert.DeserializeObject<RefreshTokenResponse>(restResponse.Content);

            request.AddHeader("authorization", "Bearer " + BlazorCallHelper.getAADToken());
            request.AddHeader("X-CSRF-TOKEN-HEADER", refreshToken!.CSRF_TOKEN);
            foreach (var header in restResponse.Headers) {
                if (header.Name == "X-UserRoles") {
                    request.AddHeader("X-UserRoles", header.Value.ToString());
                }
            }
        }

        public static void UploadFileRequest(FileDto file, RequestDto requestDto) {
            bool success = false;
            int retryCount = 3;
            int currentCount = 0;
            RestRequest request=new RestRequest();
            RefreshToken(request, requestDto);
            request.Method = Method.POST;
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "multipart/form-data");
            request.AddFile("files",file.FilePath,file.ContentType);
            while (!success && currentCount < retryCount) {
                RestClient restClient=new RestClient(requestDto.Uri);
#if DEBUG
                restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
#endif
                var response =restClient.Execute(request) ;
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted) {
                    success = true;
                }
                currentCount = currentCount + 1;
            }
        }
    }
}
