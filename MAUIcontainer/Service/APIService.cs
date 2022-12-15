using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MAUIcontainer.Common;

namespace MAUIcontainer{
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
        public static MyAppsResponse GetMyApps() {
            //Should call a Apps Management API to retrieve based on authentication token.
            //DevHttpsConnectionHelper devHttpsConnectionHelper = new DevHttpsConnectionHelper();
            //var response = devHttpsConnectionHelper.HttpClient.GetAsync($"https://mauiclient.z23.web.core.windows.net/myapps.json?dt={DateTime.Now.ToLongTimeString()}").Result;
            //return response.Content.ReadFromJsonAsync<MyAppsResponse>().Result;
            MyAppsResponse myAppsResponse=new MyAppsResponse();
            MyAppDto appDto= new MyAppDto();
            appDto.Name = "Demo1";
            appDto.BlazorNamespace = "Client";
            appDto.BlazorUrl = "https://10.120.131.80:7196/";
            appDto.Description = "Test AAD, Fingerprint, Push Notification, Telerik doc/report/UI";
            appDto.ImageBase64 = "iVBORw0KGgoAAAANSUhEUgAAADwAAAA8CAIAAAC1nk4lAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAABMxSURBVGhDxVp5kBzVeVcuEyFphRDF6dg5qlIVp4o/UonLKQfY3Znpe6Z7rt2ZPWZmtRIS4g7mMAGKhCM2xWHLB7hwwICBlMFxBBJICFMIKYSATUxI2QIstLtzH3vvzHT3dE/n9/VrLbtradmVkspXr1pve96893vf9/uO90ZrnNVL2xXbldZxQZ99pOt6s9lsuIL+/HurbRsts2Vbumm4X7XdadrulKuT0wJtWRZDbBgGWx5PYGWg8YQAtzucQAMucJsW7ZCBZhOuVlYKmi0MYesBLkNsmgBizCODMMQQthMMRocNgzT0JpTNRnpTr15OBfS8giHAAcQQvGcDgA9/Avr8G4wh0C3Qwpoz9dL0BHvvTb16OTFozAdrthzXpk7bbDtgr902WrYJnC2riWZahm61WGuCqCZwtwAFWBeK6eBrZguwLWNG1+978cfK1+48kj1mYqA7/ymgPzFoLAYj0rPVAsDRShF0hLYarWbdbJDOWnrLsdBoDNZ2HPIwXcd4V4kkeNMk+phNu4U202iu69c6tvafe3my7likFBsz0Ca9VVcsJ6FH24EpMS+WPDpe6X/g7h8ePvTmh0c+zGULkxOVicmJmWnTaVlt0jGp2TJsB7psMia4xMFuTbZVbNLSjb3vvnN2qm/TUOLvn38aaiafwBOa9pZchZxU02Q7cNe26g39Z6O/7uiRNg9Ez9s68IfXDP3R9Vu/dMcN1z72yMP79xWrtXqzAW6AMMBNWJsedIAGKxou7pbjdN9z25nJaPD+u6pzs5gcgid1vDVXIScAzaYDYugJCxtQU7O++923//LW687K9G5KJdb3x8/Zltq8tf+MRPhPrxlOfuO+77+6f2JuhsztuiZQMtCmbsAfEZy/d2j/7/eH/+z6bR9XSmQipmY83LW8hVcsi0DT9l2Bomne40ENaLASOFCcqm7O9Gwe7jlvy+D524c+tzOzKdWzYTC2rj9y3vaBz1059Fc3Xz0xOd2wzDmb+A3qI6XsemnPbyeCG4d7XE/RrbYBd6DA7a4Iv3DXtJj68S2sjg6DROpz2U+fHX/5CWj2lojhImba8jgKIzuO0dbB0odf3fvHVwyvT8f/YOfWC3YMb9yS6EgnzxpIXLgzfd4V/edvS//5Ddt/dOj1yekpclarjQ1ceu+tGwaS8fvvBSDEH8QT+J+7WMtw7AbI6P5JWN344/VdwTdYhwmDugi0C5gEQ+dBQyzo2q6T4q32TDn32ncfHgqHL9g6eOHOoYsuT19w+cCF2wehZjD+rHRy3UAc1kg8cM+sYcBH5xzjvO19Zw33H/rgl7pjwVxwcTiiG/LAD4MUT7hJ03gJJADAIM4LYXIdgEEl0N4nsJArpI8W0Q5v8Cmgw7fgUvrEZHXXtyci8ZLS+2suXOT58VB0V6JXGer7k51bz0knOlJhYnw6DsIA/Rl9kfBDd1ers5+/ftsb73+09dFHzk7Hf6cvsqY3BDptHk74773txmd+8MO33ixPz07VdR0JAHHSFXIPBHjbgikowgD3p4Kmfyi5mHjCGxEfjGMfT/7dbeVQpBxJ5qPRnKx97BM/DkhVTilIwbdDoR1R7Yv90YsGo+uGEmszvWcMxTdkEmuTGpzvnKHkxnTP7/ap69O9G1PJjsFeMGptqvczA+Ez+rS1/eoXbtjJ33v7tse/9cTBV198552Xf/Hzl9979/l33ty1d3dxdhp7IK8lyywLmm3XcIw5szHVnJpr1Itbd46JylE5VIz05GPhWjhZlKJZSRnhlTFOGuPlnE/+QJZelYNfjUaujUb+Nhbrj8c/m06sHYhhD58ZGtiU6cM2NqSjG1LxjlQcBulIJzZmkh1DyQ0DPetSPXizMZU4e7jvnG0D5w8PbBoMX3zTldMGcboBXp0QNG3ouFCqA9tMu9KozcxNz2RHxoTOoz75KK9NK4MVra+khkdVbfqK/rwczPmVAhc6JspHu4UCr8zx4qgovBDSPp/q2TwQXzccT0VVcUD8vYHYhuG+jUP96wd7AHdDOrkh1Qvdn5npRR+uDAts2pI8e0caO/nCdZc/duDl2dk6giYQswx6YtAumb2IAzefM+qNpjG9+wVotxTuq6jRshY9xgl5OVpQe6xDidaB3lFOQivxwYciwS8ltXPTPWszCeDoGHKZkIl09Md/JUhZSc2Lwf8WlZcU5cGw5k8k/6K/dzMUn0msA9ahnouuzHzx9pv/6fVX3j16pNacJTu3QU6D1T/UFiT7E4PG07HbSHJOOZcLJyvx3mw8lo8lS+EekGQ0IB4LReu7tfJwb94vloPaLwX+jIE01NaRicHEHakoSLw+FTk7Ffmt4VgtoGZlCY6b54WioIzzcoMTq5z0n1LwjZC6Xwu+o8Vzbx9omrOIqrbRQHRBBoVyHYuKW9TfcEcWql3MvwEasQL0AGIQGhst3XEnmFCO9VTCsUq4hwgtR0cCYtYvjIIYAehY+RdFvjgZ6Uj3EFnTiTOHEsCNti6ThOJvjMYKglgISBVOBXmqXDDLS1n4gIC+UgvgSYaaGtjRKo3aCAFugUnxA6htSi6sEcJ5ejC4TFxFkwA3wrM+O14QY0cFaYSXjnFyVglD0/lQNCsoHwnaGB+c8IkXICqn+tZtiXRsixNNMwmwc00q+tlM9L8kpcaHxjggDjafko2fhAohuUBwRaBEKwrU0AFz8rJY6FJhZgZreVnD4Lru50VoEsOcMuZm3vtZzq+BsgA9JoYKaqwc6c0rak4MQtNFTvmPYPDMDLRLjrW+P7FhoHdzOq4lY98KR45wWlEI5PjQOB/NSZL9XLx1QAWvSrxKcDmF4Wb9aihQ9sslIWKOHPNwLSsEeilikEQ3Zu36+GNPIKKNchTUSmqsqMUr0cSYKEN5MPEvFOniwbgbCtCSyaR6dyT8fEiBLsniglTkQkVOJl365bnHQ8brSrELqlWoLQCd9yuNZ+VqJlgKSI29+z1cy8qaJXCZoDwzjXph+zVwu2MwoqIBLrgBWo8GENeAWxqOqR2ZvnPTsWQi9mxIKAc4ZJwaF4HDYXnsKs9JVZ4MVegWp25U7IPQMbAqJT/TtIe70C1bh4O1+8RKZ3D6nns8XMvKGngeUDKSQLAHsBkvp956PXtpIOuTEYnLoVhJjeREFWomxJI46Vc6kz0vqQKcCfjgVXgPpeIJApQDi3TJWp6TC1IIvlgUYQQ8hRIXLnMyNG2+ps3eJxS6wsi49uQEkjnckTmfB3OxEGhgpc+Pi6tps/bjfx79m24kDgACifMBdaxbAWgYvRzQEDreE9RSkJgNc5dEMIEAwcmyUiAvKmUhtAR0ISCXfWH4HLZUFjQwB+Ohb8xQvS5U6pewAYwxjo20HRxKEZ9P6pRrAJFhhY5ZBwJSj3/7kbFLuvIB0tCIwI9FA9VBFZkPsBDpRoIcNpPlhZKglQIC1gagrMxlY1xJErFJ1+cWgSZ8vIq9UbTmsCWVOqJEZukU8j7ac9kvNd//FSol0nCL6j6Gcol4jshuVfA3/oSawenad783domv4g+RWXnF2BPU9war/VpeUFFp4E1WlMucCD3lZVAiWO7SZr6utg4Hj/n8eVGCzhYjRlyjeFzoFfV/VUo9IqYF6WETpnLwKh/gK1xQz45BgXTgXVDWLRECDdWCx+hAoPhGo4HRU08+CU1XAggd4igvtl6NGPu0Yoo4DT4QDbhgxSUlKE6BolOa+WbQfiNU7YzA+uXAUnrgzbgYySZ81nPq7HYNnkC699yAAjZtIBRtTU4irQA0w+3BXCweaCaADsR08+K0m/sP5zovzQnIBTIWMF8JNXYr1VgQfy6EMh+/xgRuZmfE2hNCcMCYqk9bOAwNw0Cnoio1n1SnUmFww9W065Qc+UCO40uDGYbEQ3cS+YTTEA8xFaW28cGHWZ+vLBACKMl4ND75dTLoEhWSF7pOBkB5ha+kpRoXwyaRtBcOozHgQEBGEJy6QwH7iQ8iwo6IkRTOZexWrt3yVYbEQ3cSWRo92FsKIK16+arr4GRI2rAdIkbZH3EVv8jD5vWEJ3AAGSnPjYALh6FhhpxEtSv6Vd4bzwxFjRfzkmYcfGMhjJMJ1R50CjweOvAKT4DWjbnaI4+g0CG2BYJU9ACuH+pZGoDRsCo9aW+hUvcJBqCRd2InIsUNBhQboI6rlAqFSzfeHYexjBBohA4Igw7Fo2PQ5VCz/sZrmAjmo6dfg8NlRX4JFK8hkHOkYLSKqOJZ9UWWjMFLWGlMEEp+GuCagtCT/6EjKIhFVnNupaBJr64ALUATNyyDLoeOjuBA5epjoa2Ra3igJHPLnJsdkHFkwro4C7J0Awd1vW2RJ1QD0TGJB276CIhFiaq8TglFqYdrWSHQEIYYcCHQN8pwHcHSNgs+f1ZCaPtkPWjL1YpciAiV68T5jL0EMVohLJvfCeuPa1SEBBd9ShxD2UisC8DX8abcGaju38euED5VPNCLECPdmC26nWjZOJXgpATCLVyy0k3WnL5Zc34eASBKhwE2YNGw2pWKtV/TD0uIZUtyDTIr+EbfAq/8UoETKtuvapkN5AkP17JCoBmVGWIG2nRaSP1ok//wtWw3D4jz6xEZYE1Rmbg5qL8lsFhx/NNFoCe+ErT2h6yD6KsIDgs/grkKUbEchrIRHMNlMTy7fx8OSkgrHq5lxeM0E8CFkOYdE4kfsxhOq3L1DSUfD6DkOkIE1RlQTnTFyyGlhRzpxoF5NPjI9TDqg+UTd8nNp9UxKbAkulczsr0nOHsPV/SFirIye/AwdGzZTSjQw7WsLKqnGWK8RXFI96Zu+Jt7ZR8dh9zgUPJRFUqAuCByXmGYXwIajb1BIz/DtxTaLdS5cEx5SG4eCJaTKL+UQlegPjeFpR0b51mTwVpevHp6nhgMtImkD864d7JmKQfCoW5211NQcoAhiNnVQDjnW5poGHcZaIQXkAdnLex2lA8sHFkOSeO3oDBEAFHKicE23RXaOl3lGQzW8kJpnMGdRwxhJ2HoG37RbsxOPPZEWaKKmUKHe4iCy8/TYL6RKURK6eizHInBqPHBaWTvhSOpzLgsVgjyMEXjtZ/qtgG4DSqjV6bpecSQedDuDROCnqM7BnVNI39ZFyiB8AcXZNCp3qA4/QkUbKwa9qK1+3R54macJTvEgZcq6d5E/YMjdOFLdxowrnmysm6JLKryvHe/IZgrp2luwa5l/WK2SxnH+QWnD0RZHMAQCjitcpk6/ZSmP6WNKhRSoHLsjSAKQpmPUH3Hi5SnEHlQWsEggji950Wj3VxhQlkoKwKNeSeeebbopxw+sUWe3RWqJsJAgHBLimT50i/Xn1PqBwkZjOAqmPwVFR/i2jh80U9VB/pQcw0HogDv0FV1ExWlt8yKhUKe1z25UFXenJt95nk6jV4pO/virb1xnA8YaIQzaDHrC9Z3hUyKysjwxAe3ogjiDIGjg/NMrPlcFOPpaoHHhuXpvXus9ixcz7LpxLQqWRFoSlOmZRaKleHtlajSeFoxfopDqJRVAm4YUQGi0KVWtsu1m3HkJsLQTpCGqK4Qal9RrX8LNV+gIhu1FxgydeOt1vQk8gCcb4UJZaHQXd6nC1yxbbdtfXbvSxVeqm5RcbIiGsiUdJCQAaXqVygqo2pzUyaqPICuukVL/S7V+Hep+agGViAwwyyNI0cQT1vu76srTN0LZUWg7bYBE7Zs3bSalS4BTB0VuWJAQ9GDfkUMF/wqFZzu0RXNYzmihCRA5bWbBOuQisM8vBalab5LQOajiOrJ/42m6RYVCZJ+1LEnv/HgmKgQoXE+IHAqcHuR4Xj9yS49sAcgpqpIgDVQpmKwUo70Tr/1pjfvqcrKNE2X8JbhGHbbaR55vxJPsvtP986F7hJGZJ4FOArMx7M6vDArigWFLiHAH9pV96WT33kYQcOb91RlZaABGRmA/fBhmVNP/iB3iR+Y3BQjATQKKQoXn9CDcJMF3HMAKRgqF0IjO64wpyZ0a9UkXiIrAg3eIUcyAsJv8KyPfZQVQOgAQt5oQKh/P1RO8TnUmX66kC6mlGKM1UkqzsV0oJLCJTmCmpFt3pv3VGVFoBcKFjRbDZQlRUWAguGLY5d1Tt2pVW4KlH1qVhamesPOPk0/KKHqQJapSipRvLu79sA36b/f4FQEDZyerBo0qELXg0Zr8oH7890olBWwthjlK9cGi10qsl01ojR/EtRf0ygvIvy5V34Tt97eqFQajgPc/w+aRi4y2qaJHFmbLN9yG/IIzrmoLqqKSGUQp4Ah4zuVyatCdI50f6wYv+2O1lStZbTp11eLfl3z5jpVOQVNg9wW1I2Qgngy9Y8PUpnBU14ESvI594mCCWrOhiITDz2AOg55xC102/TEae705FToAdwudHJKc3Jcf+FH5aEhukCTEJI1nLKgYGLzjqvrr79iGzpqXNjHTSjed725TlVWDXqJ0PnGbjfnJqtqX/4y3+iXLyn89ZeLnfxIetjSx2EKuj9BWX66jFgkpw2a0iSZ3bZ0p0WNfgXEIRUHfMuguE4cthv/q6hPX9PEk6Zj63aziZQJAlg6qk12yqQQ1waFUTM3vS+cvjjO/wAPr859tOuKAQAAAABJRU5ErkJggg==";
            myAppsResponse.MyAppsDto = new MyAppDto[] { appDto };
            return myAppsResponse;
        }
    }
}
