using System.Net;


namespace MAUIcontainer {
    public class ResponseDto {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public string Content { get; set; }

    }
}
