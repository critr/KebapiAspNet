using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Kebapi.Services.Authorisation
{
    public class CanReadUsersHomeHandler : AuthorizationHandler<CanReadUsersHomeRequirement>
    {
        private readonly AuthorisationService _authorisationService;

        public CanReadUsersHomeHandler(AuthorisationService authorisationService) 
        {
            _authorisationService = authorisationService;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanReadUsersHomeRequirement requirement)
        {
            // Is Admin or is User.
            if (_authorisationService.IsAdmin(context.User))
                context.Succeed(requirement);
            else if (_authorisationService.IsUser(context.User))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
