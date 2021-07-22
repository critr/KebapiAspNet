using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Kebapi.Services.Authorisation
{
    public class CanReadUserHandler : AuthorizationHandler<CanReadUserRequirement>
    {
        private readonly AuthorisationService _authorisationService;

        public CanReadUserHandler(AuthorisationService authorisationService) 
        {
            _authorisationService = authorisationService;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanReadUserRequirement requirement)
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
