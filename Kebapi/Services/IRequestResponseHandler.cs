using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace Kebapi.Services
{
    public interface IRequestResponseHandler
    {
        public Task SetResponse(HttpContext context, HttpStatusCode statusCode, object responseBody);
        public Task<TRequestType> GetRequest<TRequestType>(HttpContext context);
    }
}
