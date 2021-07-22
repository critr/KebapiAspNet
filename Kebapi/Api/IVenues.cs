using Kebapi.Dto;
using System.Threading.Tasks;

namespace Kebapi.Api
{
    /// <summary>
    /// Interface for API functionality relating to venues. 
    /// </summary>
    public interface IVenues
    {
        Task<ApiVenueResponse> Get();
        Task<ApiAffectedRowsResponse> GetCount();
        Task<ApiVenuesResponse> GetSome();
    }
}