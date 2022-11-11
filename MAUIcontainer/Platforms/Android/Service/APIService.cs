using MAUIcontainer.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;


namespace MAUIcontainer {

    public static partial class APIService {
        private static void RefreshToken(HttpClient request, RequestDto requestDto) {
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

        public static void UploadFileRequest(FileDto file, RequestDto requestDto) {
            bool success = false;
            int retryCount = 3;
            int currentCount = 0;
            DevHttpsConnectionHelper devHttpsConnectionHelper=new DevHttpsConnectionHelper();
            RefreshToken(devHttpsConnectionHelper.HttpClient, requestDto);
            devHttpsConnectionHelper.HttpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
            var fileStreamContent = new StreamContent(File.OpenRead(file.FilePath));
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            using var multipartFormContent = new MultipartFormDataContent();
            multipartFormContent.Add(fileStreamContent, name: "files", fileName: file.Name);
            while (!success && currentCount < retryCount) {
                var response = devHttpsConnectionHelper.HttpClient.PostAsync(requestDto.Uri,multipartFormContent).Result;
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted) {
                    success = true;
                }
                currentCount = currentCount + 1;
            }
        }
    }
}
