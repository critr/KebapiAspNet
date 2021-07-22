using Kebapi.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Kebapi.Services.Authentication
{
    /// <summary>
    /// Interface for our authentication service.
    /// </summary>
    public interface IAuthenticationService
    {
        Task<ApiSecurityToken> AuthenticateUser(string usernameOrEmail, string password);
        Task<ApiSecurityToken> AuthenticateUser(string usernameOrEmail, string password, CancellationToken ct);
    }
}