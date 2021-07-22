
using Kebapi.Services.Token;

namespace Kebapi.Dto
{
    public class ApiUserLoginResponse
    {
        public ApiStatus ApiStatus { get; set; }
        public ApiSecurityToken ApiSecurityToken { get; set; }
    }
}
