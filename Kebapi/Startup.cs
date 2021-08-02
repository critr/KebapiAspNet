using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Kebapi.Api;
using Kebapi.DataAccess;
using Kebapi.Services;
using Kebapi.Services.Authentication;
using Kebapi.Services.Authorisation;
using Kebapi.Services.Hashing;
using Kebapi.Services.Token;
using Kebapi.Services.Routing;

namespace Kebapi
{
    public class Startup
    {
        internal static Settings AppSettings { get; private set; }

        public Startup(IConfiguration configuration)
        {
            // Grab strongly-typed settings sourced from injected IConfiguration.
            AppSettings = GetSettings(configuration);
        }

        // Builds a strongly-typed Settings object containing everything the app
        // needs from IConfiguration. Any additional constraints we want to
        // impose on config go here, e.g. enforcing a known Environment variable
        // for token key signing.
        public static Settings GetSettings(IConfiguration config) 
        {
            var section = config.GetSection("Settings");
            if (!section.Exists())
                throw new System.ArgumentException(
                    "Cannot configure the app. Missing expected 'Settings' section in configuration.");
            var settings = new Settings();
            section.Bind(settings);
            // Bind won't tell you when it fails to bind.
            // This will quite happily "work":
            //      int test = 2;
            //      section.Bind(test);
            // So we'll go with a couple of basic checks, but this would be the
            // place to make sure the configuration we have is the configuration
            // we expect.
            if (settings.Api == null)
                throw new System.ArgumentException(
                    $"Cannot configure the app. Some Settings are missing. Binding failed for '{typeof(Settings).Name}.{nameof(settings.Api)}'.");
            if (settings.Dal == null)
                throw new System.ArgumentException(
                    $"Cannot configure the app. Some Settings are missing. Binding failed for '{typeof(Settings).Name}.{nameof(settings.Dal)}'.");

            // Here we can add any config rules we want to enforce, like a token
            // signing key coming from the real* Environment. (*Not the hodgepodge
            // cobbled together in an IConfiguration which includes all manner of
            // sources each taking precedence over the other in arcane ways, so 
            // we're explicitly avoiding a call like this here:
            //      signingKey = config["KEBAPI_AUTH_SECRET"];)
            var signingKey = System.Environment.GetEnvironmentVariable("KEBAPI_AUTH_SECRET");
            if (string.IsNullOrEmpty(signingKey))
                throw new System.ArgumentException(
                    "Cannot configure the app. Environment is missing required signing key.");
            settings.Api.Auth.TokenValidation.SigningKey = signingKey;

            return settings;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Slice settings so each service can access only what it needs.
            var tvSettings = AppSettings.Api.Auth.TokenValidation;
            services.AddSingleton(tvSettings);
            services.AddSingleton(AppSettings.Api.UserRegistration);
            services.AddSingleton(AppSettings.Api.Paging);
            services.AddSingleton(AppSettings.Dal);

            // Make HttpContext available to services.
            services.AddHttpContextAccessor();

            // Configure Authentication with JWT (Jason Web Tokens).
            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Issuer, Audience are used to ensure generated tokens are unique.
                    ValidateIssuer = true,
                    ValidIssuer = tvSettings.Issuer, 
                    ValidateAudience = true,
                    ValidAudience = tvSettings.Audience,
                    // SigningKey should be a long randomly-generated value.
                    ValidateIssuerSigningKey = true,
                    // Being explicit about the crypto algorithms we will accept, 
                    // locks down what can be thrown at us maliciously.
                    ValidAlgorithms = new[]
                    {
                      SecurityAlgorithms.HmacSha256
                    },
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(tvSettings.SigningKey))
                };
            });

            // Register our Authorisation service and configure the policies
            // that will restrict access to API actions.
            // Unfortunately, the way the platform handles this necessitates
            // spray coding, with 3 - 4 components per policy plus glue often
            // being required.
            services.AddSingleton<Services.Authorisation.AuthorisationService>();
            services.AddSingleton<IAuthorizationHandler, Services.Authorisation.CanReadUserHandler>();
            services.AddSingleton<IAuthorizationHandler, Services.Authorisation.CanUpdateUserHandler>();
            services.AddSingleton<IAuthorizationHandler, Services.Authorisation.CanReadUsersHomeHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanReadUserPolicy", pb =>
                    pb.Requirements.Add(new Services.Authorisation.CanReadUserRequirement()));
                options.AddPolicy("CanUpdateUserPolicy", pb =>
                    pb.Requirements.Add(new Services.Authorisation.CanUpdateUserRequirement()));
                options.AddPolicy("CanReadUsersHomePolicy", pb =>
                    pb.Requirements.Add(new Services.Authorisation.CanReadUsersHomeRequirement()));
                options.AddPolicy("IsInRoleAdminPolicy", pb => pb.RequireRole("Admin"));
                // We don't currently use the IsInRoleUserOrHigherPolicy policy.
                // Keeping it here for illustration.
                options.AddPolicy("IsInRoleUserOrHigherPolicy", pb => pb.RequireAssertion(context =>
                {
                    // Our model currently consists of 3 roles in this hierarchy:
                    // - Admin
                    // - User
                    // - Everyone
                    // If more roles are added in future, this assert should
                    // evaluate against all higher roles in the hierarchy. For 
                    // now, a simple OR does it for role User.
                    return context.User.IsInRole("Admin") || context.User.IsInRole("User");
                }));

            });
            // For reference, here are some other ways to specify policies,
            // including those referencing claims in the security token:
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("SomePolicy", policy => policy.RequireClaim("Permission", "CanViewPage", "CanViewAnything"));
            //    options.AddPolicy("EmployeeOnlyPolicy", policy => policy.RequireClaim("EmployeeNumber"));
            //    options.AddPolicy("EmployeeMustHaveOneOfTheseIdsPolicy", policy => policy.RequireClaim("EmployeeNumber", "1", "2", "3"));
            //    options.AddPolicy("RequiresEvalOfThisAssertionPolicy", policy => policy.RequireAssertion(context => context.Succeed(SomeAuthorizationRequirement),true));
            //    options.AddPolicy("RequiresEvalOfThisAssertionPolicy", policy => policy.RequireAssertion(new System.Func<Microsoft.AspNetCore.Authorization.AuthorizationHandlerContext, bool>(true), true));            
            //});


            // Register our other services
            services.AddSingleton<DataAccess.IDal, DataAccess.Dal>();
            services.AddSingleton<Services.Hashing.IHashingService,
                Services.Hashing.HashingService>();
            services.AddSingleton<Services.Token.TokenService>();
            services.AddSingleton<Services.Authentication.IAuthenticationService, 
                Services.Authentication.AuthenticationService>();
            services.AddSingleton<Services.IRequestResponseHandler, 
                Services.RequestResponseHandler>();
            services.AddSingleton<Services.InputParser>();
            services.AddSingleton<Services.DataMapper>();
            services.AddTransient<Api.IUsers, Api.Users>();
            services.AddTransient<Api.IVenues, Api.Venues>();
            services.AddTransient<Api.IAdmins, Api.Admins>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            // For non-dev environments, ASP.Net default behaviour when an exception
            // is thrown, is to return a blank 500 response to clients.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // Set the app to perform Authentication and Authorisation, in this
            // case based on the JWT we told it to expect in ConfigureServices.
            // Note: Order of Authentication and Authorisation matters. Also
            // must be injected after any routing and before any output-generating
            // stuff. (UseRouting must come before; UseEndpoints must come after.)
            app.UseAuthentication();
            app.UseAuthorization();

            // The Add method here is an extension method we've written for
            // IEndpointRouteBuilder. It takes the route builder (endpoints),
            // adds our route mappings to it, and returns it.
            app.UseEndpoints(
                endpoints => endpoints
                .Add(env));
        }

    }

}
