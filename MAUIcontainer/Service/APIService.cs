using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MAUIcontainer {
    public partial class APIService : IAPIService {
        public void RefreshToken(HttpClient request, RequestDto requestDto) {
            DevHttpsConnectionHelper devHttpsConnectionHelper=new DevHttpsConnectionHelper();
            devHttpsConnectionHelper.HttpClient.DefaultRequestHeaders.Add("authorization", "Bearer " + BlazorCallHelper.getAADToken());
            var response=devHttpsConnectionHelper.HttpClient.GetAsync($"https://{requestDto.Domain}/Login?force=false").Result;
            var refreshToken=response.Content.ReadFromJsonAsync<RefreshTokenResponse>().Result;

            request.DefaultRequestHeaders.Add("authorization", "Bearer " + BlazorCallHelper.getAADToken());
            request.DefaultRequestHeaders.Add("X-CSRF-TOKEN-HEADER", refreshToken!.CSRF_TOKEN);
            foreach (var header in response.Headers) {
                if (header.Key == "X-UserRoles") {
                    request.DefaultRequestHeaders.Add("X-UserRoles", header.Value.FirstOrDefault());
                }
            }
        }

        public void UploadFileRequest(FileDto file, RequestDto requestDto) {
            bool success = false;
            int retryCount = 3;
            int currentCount = 0;
            DevHttpsConnectionHelper devHttpsConnectionHelper=new DevHttpsConnectionHelper();
            RefreshToken(devHttpsConnectionHelper.HttpClient, requestDto);
            devHttpsConnectionHelper.HttpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
            var fileStreamContent = new StreamContent(File.OpenRead(file.FilePath));
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue($"{file.ContentType}");
            using var multipartFormContent = new MultipartFormDataContent();
            multipartFormContent.Add(fileStreamContent, name: "files", fileName: file.Name);
            while (!success && currentCount < retryCount) {
                var response = devHttpsConnectionHelper.HttpClient.PostAsync(requestDto.Uri,multipartFormContent).Result;
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted) {
                    success = true;
                }
                currentCount = currentCount + 1;
            }
            File.Delete(file.FilePath);
        }
        public MyAppsResponse GetMyApps() {
            //Should call a Apps Management API to retrieve based on authentication token.
            DevHttpsConnectionHelper devHttpsConnectionHelper = new DevHttpsConnectionHelper();
            var response = devHttpsConnectionHelper.HttpClient.GetAsync($"https://mauiclient.z23.web.core.windows.net/myapps.json?dt={DateTime.Now.ToLongTimeString()}").Result;
            return response.Content.ReadFromJsonAsync<MyAppsResponse>().Result;
        }
    }
}
