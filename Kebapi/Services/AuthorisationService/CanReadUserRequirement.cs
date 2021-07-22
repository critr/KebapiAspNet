using Microsoft.AspNetCore.Authorization;

namespace Kebapi.Services.Authorisation
{
    // Has permission to Read
    public class CanReadUserRequirement : IAuthorizationRequirement { }
}
