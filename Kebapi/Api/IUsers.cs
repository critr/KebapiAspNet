using Kebapi.Dto;
using Kebapi.Services.Token;
using System.Threading.Tasks;

namespace Kebapi.Api
{
    /// <summary>
    /// Interface for API functionality relating to regular users.
    /// </summary>
    public interface IUsers
    {
        Task<ApiAffectedRowsResponse> Activate();
        Task<ApiAffectedIdResponse> Add();
        Task<ApiAffectedIdResponse> AddFavourite();
        Task<ApiUserLoginResponse> Authenticate();
        Task<ApiAffectedRowsResponse> Deactivate();
        Task<ApiUserResponse> Get();
        Task<ApiUserAccountStatusResponse> GetAccountStatus();
        Task<ApiUserResponse> GetByUsername();
        Task<ApiAffectedRowsResponse> GetCount();
        Task<ApiUsersResponse> GetSome();
        Task<ApiVenuesResponse> GetSomeFavourites();
        Task<ApiAffectedIdResponse> RemoveFavourite();
    }
}