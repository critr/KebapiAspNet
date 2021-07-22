using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Kebapi.Services.Authorisation
{
    public class CanUpdateUserHandler : AuthorizationHandler<CanUpdateUserRequirement>
    {
        private readonly AuthorisationService _authorisationService;

        public CanUpdateUserHandler(AuthorisationService authorisationService) 
        {
            _authorisationService = authorisationService;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanUpdateUserRequirement requirement)
        {
            // Is Admin or is resource owner.
            if (_authorisationService.IsAdmin(context.User))
                context.Succeed(requirement);
            else if (_authorisationService.IsOwner(context.User, (HttpContext)context.Resource))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
