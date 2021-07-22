using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kebapi.Services
{
    /// <summary>
    /// Api request/response helper providing standard implementations for sending
    /// and receiving.
    /// </summary>
    public class RequestResponseHandler : IRequestResponseHandler
    {
        /// <summary>
        /// Provies the standard implementation for setting a response.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="statusCode"></param>
        /// <param name="responseBody"></param>
        /// <returns></returns>
        public async Task SetResponse(HttpContext context, HttpStatusCode statusCode, object responseBody)
        {
            context.Response.StatusCode = (int)statusCode;
            await JsonSerializer.SerializeAsync(context.Response.Body, responseBody, cancellationToken: context.RequestAborted);
        }

        /// <summary>
        /// Provides the standard implementation for getting a request.
        /// </summary>
        /// <typeparam name="TRequestType"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<TRequestType> GetRequest<TRequestType>(HttpContext context) 
        {
            context.Request.EnableBuffering();
            return await JsonSerializer.DeserializeAsync<TRequestType>(context.Request.Body, cancellationToken: context.RequestAborted);
        }
    }
}
