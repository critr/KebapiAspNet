using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace Kebapi.Services.Authorisation
{
    /// <summary>
    /// Authorisation service. Or... who can do what.
    /// 
    /// Out of the box, .Net seems to want to follow the spray code pattern for 
    /// authorisation handling. That means code in in 3 - 5 different places per 
    /// rule: everything in this namespace, plus Startup config plus RoutingService 
    /// rule application. This service is therefore more accurately the sum of all 
    /// of those components.
    public class AuthorisationService 
    {
        /// <summary>
        /// Checks if a user has the Admin role.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>True if they have the role, false otherwise.</returns>
        public bool IsAdmin(ClaimsPrincipal user) 
        {
            return user.IsInRole("Admin");
        }

        /// <summary>
        /// Checks is a user has the User role.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool IsUser(ClaimsPrincipal user)
        {
            return user.IsInRole("User");
        }

        /// <summary>
        /// Checks if a user owns the resource in the current context.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="resource"></param>
        /// <returns>True if the resource is owned, dalse otherwise.</returns>
        public bool IsOwner(ClaimsPrincipal user, HttpContext context)
        {
            // The model is essentially: anything under "/users/{userId}" is owned
            // by that user.

            if (!context.Request.Path.StartsWithSegments("/users"))
                return false;

            // cast ensures we get id as string if id exists, or null if it doesn't
            var resourceId = (string)context.GetRouteValue("id");
            if (string.IsNullOrEmpty(resourceId))
                return false;

            var idClaim = user.FindFirst("id");
            if (idClaim == null)
                return false;
            var userId = idClaim.Value;
            if (string.IsNullOrEmpty(userId))
                return false;


            // All claims are strings by definition. So is our route id at this point.
            if (userId != resourceId)
                return false;

            return true;

        }

    }
}
