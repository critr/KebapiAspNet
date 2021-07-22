using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Kebapi.DataAccess;

namespace Kebapi.Test
{
    /// <summary>
    /// Fixture for testing our web server.
    /// </summary>
    public class WebServerFixture : WebApplicationFactory<Startup>
    {
        // Tests will all run against a database used only for testing that will
        // be created, populated, and dropped as needed.
        // Obviously the following values should never target a production
        // database. Unless you're super mad and it's your last day.
        private const string TestDatabaseName = "KebapiASPNetTests";
        private readonly string TestConnectionString = 
            $"Server=(localdb)\\mssqllocaldb;Database={TestDatabaseName};Trusted_Connection=True;MultipleActiveResultSets=true";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Attempt to replace the connection string with the one targeting
            // our test database.
            // There's another check on this later, because if this string substitution
            // fails (e.g. if an expected field name changes in config, or if we simply
            // typo the key here), we will end up targeting whichever database Dal infers
            // without this intervention, and that could be ANY database we weren't
            // intending to hit. Not good.
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(
                    new[]
                    {
                        new KeyValuePair<string, string>(
                            "Settings:Dal:ConnectionString",
                            TestConnectionString),
                    });
            });
            
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<MockAuthentication>();

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {  
                    var dal = scope.ServiceProvider.GetRequiredService<IDal>();
                    // Abort if we're not in our expected test database.
                    if (dal.DbName != TestDatabaseName 
                    || dal.ConnectionString != TestConnectionString)
                        throw new ArgumentException($"Expected to be connecting to database '{TestDatabaseName}' with connection string of '{TestConnectionString}', but instead are trying to connect to database '{dal.DbName}' with connection string of '{dal.ConnectionString}'.");
                    dal.ResetKebApiTestDatabase(CancellationToken.None).Wait(5000);

                    // Use our mock authentication service to "preload" into our
                    // test db some mock users covering all available Roles.
                    var ma = scope.ServiceProvider.GetRequiredService<MockAuthentication>(); 
                    ma.AddUser(MockAuthentication.StandardTestUserWithRoleAdmin); 
                    ma.AddUser(MockAuthentication.StandardTestUserWithRoleUser); 
                    ma.AddUser(MockAuthentication.StandardTestUserWithRoleEveryone); 
                }
            });
        }

        // Note: We have access to the client in this override if we ever need it.
        //protected override void ConfigureClient(HttpClient client)
        //{
        //}
    }
}
