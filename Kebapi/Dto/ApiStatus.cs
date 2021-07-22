using System.Collections.Generic;
using System.Net;

namespace Kebapi.Dto
{
    public class ApiStatus
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }
    }   
}
