using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace Kebapi.Services.Routing
{
    /// <summary>
    /// Specifies API routing with relevant authorisation restictions, exposing
    /// it through an IEndpointRouteBuilder extension method. This means that in
    /// Startup.Configure we can add our routing like this, where Add is our
    /// extension method:
    /// <para>
    /// app.UseEndpoints(endpoints => endpoints.Add(env));
    /// </para>
    /// </summary>
    public static class RoutingService
    {

        /// <summary>
        /// Helper for mapping to functions exposed in our Api classes 
        /// (controllers).
        /// </summary>
        /// <typeparam name="TApiController">An Api class.</typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        private static RequestDelegate MapApiMethod<TApiController>(Func<TApiController, Task> handler)
        {
            return context =>
            {
                var controller = context.RequestServices.GetRequiredService<TApiController>();
                return handler(controller);
            };
        }

        /// <summary>
        /// Defines the endpoint-to-API function mappings, and makes them 
        /// available through an Add (extension) method on IEndpointRouteBuilder.
        /// <para>
        /// We can use it like this in Startup.Configure:
        /// </para>
        /// <para>
        /// app.UseEndpoints(endpoints => endpoints.Add(env));
        /// </para>
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        // Extension Method.
        // Gives route builder an Add method that squeezes in our own
        // endpoint-to-action mappings.
        public static IEndpointRouteBuilder Add(
            this IEndpointRouteBuilder endpoints,
            IWebHostEnvironment env)
        {

            // Users
            // Outline of routing plan and authorisation policies employed for Users,
            // with emphasis on being demonstrative over practical:
            // . Resources belonging to a user are structured as
            //   /users/{userId}/{resource}.
            // . Anything under /users/{userId}/ can be read with
            //   CanReadUserPolicy.
            // . Anything under /users/{userId}/ can be updated with
            //   CanUpdateUserPolicy.
            // . /users deliberately has no authorisation requirement to match
            //   the Node.js version, but we do set it to AllowAnonymous to be
            //   explicit about it.
            // . /users/register is also set to AllowAnonymous so that anyone
            //   can register.
            // . /users/home is a contrived and not fleshed-out area only accessible
            //   with CanReadUsersHomePolicy.

            // Authentication - We explicitly AllowAnonymous access so it can happen
            // behind the scenes whenever it is needed.
            endpoints.MapPost("users/auth",
                MapApiMethod<Api.IUsers>(x => x.Authenticate()))
                .AllowAnonymous();

            endpoints.MapGet("/users",
                MapApiMethod<Api.IUsers>(x => x.GetSome()))
                .AllowAnonymous();
            // An example if we wanted to apply the currently unused
            // IsInRoleUserOrHigherPolicy:
            //  .RequireAuthorization("IsInRoleUserOrHigherPolicy");
            endpoints.MapGet("/users/find/",
                MapApiMethod<Api.IUsers>(x => x.GetByUsername()))
                .AllowAnonymous();
            endpoints.MapGet("/users/{id}",
                MapApiMethod<Api.IUsers>(x => x.Get()))
                .RequireAuthorization("CanReadUserPolicy");
            endpoints.MapGet("/users/{id}/favourites",
                MapApiMethod<Api.IUsers>(x => x.GetSomeFavourites()))
                .RequireAuthorization("CanReadUserPolicy");
            endpoints.MapPost("/users/{id}/favourites/{venueId}",
                MapApiMethod<Api.IUsers>(x => x.AddFavourite()))
                .RequireAuthorization("CanUpdateUserPolicy");
            endpoints.MapDelete("/users/{id}/favourites/{venueId}",
                MapApiMethod<Api.IUsers>(x => x.RemoveFavourite()))
                .RequireAuthorization("CanUpdateUserPolicy");
            endpoints.MapGet("/users/{id}/status",
                MapApiMethod<Api.IUsers>(x => x.GetAccountStatus()))
                .RequireAuthorization("CanReadUserPolicy");
            // Seems to be no MapPatch method, so map to it explicitly.
            endpoints.MapMethods("/users/{id}/activate", new[] { HttpMethods.Patch },
                MapApiMethod<Api.IUsers>(x => x.Activate()))
                .RequireAuthorization("CanUpdateUserPolicy");
            endpoints.MapMethods("/users/{id}/deactivate", new[] { HttpMethods.Patch },
                MapApiMethod<Api.IUsers>(x => x.Deactivate()))
                .RequireAuthorization("CanUpdateUserPolicy");
            endpoints.MapPost("/users/register",
                MapApiMethod<Api.IUsers>(x => x.Add()))
                .AllowAnonymous();
            // Deliberately not fleshed out, just demonstrating another policy.
            // TODO: Decide if we make more of this or at least bring in line with the other actions.
            endpoints.MapGet("/users/home",
                context => context
                .Response.WriteAsync("A contrived shared area only for registered users."))
                .RequireAuthorization("CanReadUsersHomePolicy");

            // Venues
            // Nothing special here in terms of routing and authorisation, keeping
            // it in line with Node.js version.
            endpoints.MapGet("/venues/{id}",
                MapApiMethod<Api.IVenues>(x => x.Get()))
                .AllowAnonymous();
            endpoints.MapGet("/venues",
                MapApiMethod<Api.IVenues>(x => x.GetSome()))
                .AllowAnonymous();
            endpoints.MapGet("/venues/{id}/distance",
                MapApiMethod<Api.IVenues>(x => x.GetDistance()))
                .AllowAnonymous();
            endpoints.MapGet("/venues/nearby",
                MapApiMethod<Api.IVenues>(x => x.GetNearby()))
                .AllowAnonymous();

            // Admin/Maintenance
            // These are Admin functions that shouldn't be mapped anywhere other
            // than a Dev or Admin environment. Arguably some of these functions
            // should never be accessible from a front end. We'll keep them just
            // because it's convenient for this project at this stage, leaving
            // this comment as a reminder. 
            if (env.IsDevelopment())
            {
                // Choosing to segregate admin functions to an /admin route.
                // A different world view might keep admins as just a special
                // case of a user. Mappings would then stay as the usual
                // /users/{id}/{someaction}.

                // This set of critical database operations don't require authorisation
                // because authorisation requires database access and we may not even
                // have a db at this point. Hence above comment and only enabling in dev
                // environment.
                endpoints.MapGet("/admin/dev/createdb",
                    MapApiMethod<Api.IAdmins>(x => x.CreateDb()));
                endpoints.MapGet("/admin/dev/dropdb",
                    MapApiMethod<Api.IAdmins>(x => x.DropDb()));
                endpoints.MapGet("/admin/dev/resetdb",
                    MapApiMethod<Api.IAdmins>(x => x.ResetDb()));
                endpoints.MapGet("/admin/dev/resettestdb",
                    MapApiMethod<Api.IAdmins>(x => x.ResetTestDb()));

                // Some stats that could be used in an Admin dashboard.
                endpoints.MapGet("/users/count",
                    MapApiMethod<Api.IUsers>(x => x.GetCount()))
                    .RequireAuthorization("IsInRoleAdminPolicy");
                endpoints.MapGet("/venues/count",
                    MapApiMethod<Api.IVenues>(x => x.GetCount()))
                    .RequireAuthorization("IsInRoleAdminPolicy");

            }

            return endpoints;

        }

    }
}
