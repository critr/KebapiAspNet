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
        Task<ApiVenueDistanceResponse> GetDistance();
        Task<ApiVenuesNearbyResponse> GetNearby();
        Task<ApiVenuesResponse> GetSome();
    }
}