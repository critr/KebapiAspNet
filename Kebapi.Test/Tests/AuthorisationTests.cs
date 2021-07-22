using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Kebapi.Services;
using Kebapi.Services.Authentication;
using Kebapi.Services.Token;


namespace Kebapi.Test
{
    // A series of lightweight tests to test permissions for executing API
    // actions. In most cases just the HTTP status code is checked for 
    // success or failure, since the heavy-lifting tests for those actions are
    // already done in different tests elsewhere. (See UserTests and
    // VenueTests.)
    // These tests are intended to be illustrative, and therefore are not as
    // exhaustive and nor do they cover all available API actions. 

    [Collection(WebServerCollection.Name)]
    public class AuthorisationTests : IClassFixture<WebServerFixture>
    {
        private static HttpClient _client;
        private readonly Bogus.Faker<Dto.ApiRegisterUser> _testUsers;
        private readonly IAuthenticationService _authenticationService;

        public AuthorisationTests(WebServerFixture fixture)
        {
            _client = fixture.CreateClient();
            _testUsers = new Bogus.Faker<Dto.ApiRegisterUser>()
                .StrictMode(true)
                .RuleFor(x => x.Name, y => y.Person.FirstName)
                .RuleFor(x => x.Surname, y => y.Person.LastName)
                .RuleFor(x => x.Username, (y, x) => y.Person.UserName)
                .RuleFor(x => x.Email, (y, x) => y.Person.Email)
                .RuleFor(x => x.Password, (y, x) => y.Internet.Password(10))
                ;
            _authenticationService = fixture.Services.GetService<IAuthenticationService>();
        }


        // Helpers

        // Users.Add helper.
        // Attempt a user registration with the user request it's given, returning the response.
        private static async Task<Dto.ApiAffectedIdResponse> RegisterGivenUser(Dto.ApiRegisterUser request, HttpStatusCode expectedStatusCode)
        {
            var json = JsonSerializer.Serialize(request);
            var httpResponse = await _client.PostAsync($"/users/register", new StringContent(json));
            Assert.Equal(expectedStatusCode, httpResponse.StatusCode);
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            var response = JsonSerializer.Deserialize<Dto.ApiAffectedIdResponse>(content);
            return response;
        }

        // Users.Get helper.
        private static async Task<HttpResponseMessage> AttemptUsersGet(object id, Dto.ApiSecurityToken apiSecurityToken)
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiSecurityToken.Token);
            
            return await AttemptUsersGet(id);
        }
        private static async Task<HttpResponseMessage> AttemptUsersGet(object id)
        {
            return await _client.GetAsync($"/users/{id}");
        }

        // Users.GetByUsername helper.
        private static async Task<Dto.ApiUserResponse> AttemptUsersGetByUsername(object username)
        {
            var httpResponse = await _client.GetAsync($"/users/find?username={username}");
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            var response = JsonSerializer.Deserialize<Dto.ApiUserResponse>(content);
            return response;
        }

        // Users.GetSome helper.
        private static async Task<HttpResponseMessage> AttemptUsersGetSome(Dto.ApiSecurityToken apiSecurityToken)
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiSecurityToken.Token);

            return await AttemptUsersGetSome();
        }
        private static async Task<HttpResponseMessage> AttemptUsersGetSome()
        {
            return await _client.GetAsync($"/users");
        }

        // Users.GetSomeFavourites helper.
        private static async Task<HttpResponseMessage> AttemptUsersGetSomeFavourites(object id, Dto.ApiSecurityToken apiSecurityToken)
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiSecurityToken.Token);

            return await AttemptUsersGetSomeFavourites(id);
        }
        private static async Task<HttpResponseMessage> AttemptUsersGetSomeFavourites(object id)
        {
            return await _client.GetAsync($"/users/{id}/favourites");
        }

        // Users.AddFavourite helper.
        private static async Task<HttpResponseMessage> AttemptUsersAddFavourite(object id, object venueId, Dto.ApiSecurityToken apiSecurityToken)
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiSecurityToken.Token);

            return await AttemptUsersAddFavourite(id, venueId);
        }
        private static async Task<HttpResponseMessage> AttemptUsersAddFavourite(object id, object venueId)
        {
            return await _client.PostAsync($"/users/{id}/favourites/{venueId}", null);
        }


        // Users.Get tests

        // Register a user; authenticate and try to get. Should result in OK.
        [Fact]
        public async Task WhenAuthenticatedUserGetsUser()
        {
            var request = _testUsers.Generate();
            var id = (await RegisterGivenUser(request, HttpStatusCode.Created)).ApiAffectedId.Value;
            var apiSecurityToken = await _authenticationService.AuthenticateUser(request.Username, request.Password);
            Assert.NotNull(apiSecurityToken);
            var response = await AttemptUsersGet(id, apiSecurityToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Register a user; do not authenticate and try to get. Should result in
        // Unauthorised.
        [Fact]
        public async Task WhenUnuthenticatedUserGetsUser()
        {
            var request = _testUsers.Generate();
            var id = (await RegisterGivenUser(request, HttpStatusCode.Created)).ApiAffectedId.Value;
            var response = await AttemptUsersGet(id);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // Register 2 users; authenticate the first and try to get the second
        // should result in Forbidden.
        [Fact]
        public async Task WhenUnauthorisedUserGetsUser()
        {
            var requestUser1 = _testUsers.Generate();
            await RegisterGivenUser(requestUser1, HttpStatusCode.Created);
            var apiSecurityTokenUser1 = await _authenticationService.AuthenticateUser(requestUser1.Username, requestUser1.Password);
            Assert.NotNull(apiSecurityTokenUser1);

            var requestUser2 = _testUsers.Generate();
            var idUser2 = (await RegisterGivenUser(requestUser2, HttpStatusCode.Created)).ApiAffectedId.Value;

            var response = await AttemptUsersGet(idUser2, apiSecurityTokenUser1);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Take a known user; try to get it with the mocked admin user. Should 
        // result in OK.
        // The API does not expose a method to register Admins, so this test
        // will use the fixture's mocked admin to test getting every known user.
        [Theory]
        [MemberData(nameof(TestData.KnownUsers), MemberType = typeof(TestData))]
        public async Task WhenRoleAdminGetsKnownUsers(Dto.ApiUser knownUser)
        {
            // Get the fixture's mocked admin user
            var requestUserAdmin = MockAuthentication.StandardTestUserWithRoleAdmin;
            var apiSecurityTokenUserAdmin = await _authenticationService.AuthenticateUser(requestUserAdmin.Username, requestUserAdmin.Password);
            Assert.NotNull(apiSecurityTokenUserAdmin);

            var response = await AttemptUsersGet(knownUser.Id, apiSecurityTokenUserAdmin);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Register a new user; try to get it with the mocked admin user.
        // Should result in OK.
        // The API does not expose a method to register Admins, so this test
        // will use the fixture's mocked admin to test getting a user.
        [Fact]
        public async Task WhenRoleAdminGetsUser()
        {
            // Get the fixture's mocked admin user
            var requestUserAdmin = MockAuthentication.StandardTestUserWithRoleAdmin;
            var apiSecurityTokenUserAdmin = await _authenticationService.AuthenticateUser(requestUserAdmin.Username, requestUserAdmin.Password);
            Assert.NotNull(apiSecurityTokenUserAdmin);

            var requestUser = _testUsers.Generate();
            var idUser = (await RegisterGivenUser(requestUser, HttpStatusCode.Created)).ApiAffectedId.Value;

            var response = await AttemptUsersGet(idUser, apiSecurityTokenUserAdmin);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }




        // Users.GetSome tests

        // This set should demonstrate that Users.GetSome can be called
        // regardless of permissions. (GetSome has no such requirment for
        // demonstration purposes more than by design.)
        [Fact]
        public async Task WhenRoleAdminGetsSomeUsers()
        {
            // Get the fixture's mocked user with role Admin.
            // Note: The fixture already registers this user at startup.
            var requestUser = MockAuthentication.StandardTestUserWithRoleAdmin;
            var apiSecurityToken = await _authenticationService.AuthenticateUser(requestUser.Username, requestUser.Password);
            Assert.NotNull(apiSecurityToken);

            var response = await AttemptUsersGetSome(apiSecurityToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task WhenRoleUserGetsSomeUsers()
        {
            // Get the fixture's mocked user with role User.
            // Note: The fixture already registers this user at startup.
            var requestUser = MockAuthentication.StandardTestUserWithRoleUser;
            var apiSecurityToken = await _authenticationService.AuthenticateUser(requestUser.Username, requestUser.Password);
            Assert.NotNull(apiSecurityToken);

            var response = await AttemptUsersGetSome(apiSecurityToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task WhenRoleEveryoneGetsSomeUsers()
        {
            // Get the fixture's mocked user with role Everyone.
            // Note: The fixture already registers this user at startup.
            var requestUser = MockAuthentication.StandardTestUserWithRoleEveryone;
            var apiSecurityToken = await _authenticationService.AuthenticateUser(requestUser.Username, requestUser.Password);
            Assert.NotNull(apiSecurityToken);

            var response = await AttemptUsersGetSome(apiSecurityToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task WhenRoleUndefinedGetsSomeUsers()
        {
            // Attempt with no authentication at all.
            var response = await AttemptUsersGetSome();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        // Users.GetSomeFavourites tests

        // Register a user; authenticate and try to get some favourites. The new
        // user should have no favourites. Should result in Not Found.
        [Fact]
        public async Task WhenAuthenticatedUserGetsSomeUserFavourites()
        {
            var request = _testUsers.Generate();
            var id = (await RegisterGivenUser(request, HttpStatusCode.Created)).ApiAffectedId.Value;
            var apiSecurityToken = await _authenticationService.AuthenticateUser(request.Username, request.Password);
            Assert.NotNull(apiSecurityToken);
            var response = await AttemptUsersGetSomeFavourites(id, apiSecurityToken);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Register a user; do not authenticate and try to get some favourites.
        // Should result in Unauthorised.
        [Fact]
        public async Task WhenUnuthenticatedUserGetsSomeUserFavourites()
        {
            var request = _testUsers.Generate();
            var id = (await RegisterGivenUser(request, HttpStatusCode.Created)).ApiAffectedId.Value;
            var response = await AttemptUsersGetSomeFavourites(id);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // Register 2 users; authenticate the first and try to get some
        // favourites from the second. Should result in Forbidden. (The second user
        // doesn't have favourites because we don't add any, nonetheless Forbidden
        // should trigger before we even get that far.)
        [Fact]
        public async Task WhenUnauthorisedUserGetsSomeUserFavourites()
        {
            var requestUser1 = _testUsers.Generate();
            await RegisterGivenUser(requestUser1, HttpStatusCode.Created);
            var apiSecurityTokenUser1 = await _authenticationService.AuthenticateUser(requestUser1.Username, requestUser1.Password);
            Assert.NotNull(apiSecurityTokenUser1);

            var requestUser2 = _testUsers.Generate();
            var idUser2 = (await RegisterGivenUser(requestUser2, HttpStatusCode.Created)).ApiAffectedId.Value;

            var response = await AttemptUsersGetSomeFavourites(idUser2, apiSecurityTokenUser1);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Take a known user; try to get some favourites from it using the
        // mocked admin user. Should result in OK or Not Found.
        // The API does not expose a method to register Admins, so this test
        // will use the fixture's mocked admin to test getting every known user.
        [Theory]
        [MemberData(nameof(TestData.KnownUsers), MemberType = typeof(TestData))]
        public async Task WhenRoleAdminGetsKnownUserFavourites(Dto.ApiUser knownUser)
        {
            // Get the fixture's mocked admin user
            var requestUserAdmin = MockAuthentication.StandardTestUserWithRoleAdmin;
            var apiSecurityTokenUserAdmin = await _authenticationService.AuthenticateUser(requestUserAdmin.Username, requestUserAdmin.Password);
            Assert.NotNull(apiSecurityTokenUserAdmin);

            var response = await AttemptUsersGetSomeFavourites(knownUser.Id, apiSecurityTokenUserAdmin);
            Assert.True(HttpStatusCode.OK == response.StatusCode || HttpStatusCode.NotFound == response.StatusCode);
        }

        // Register a new user; try to get some favourites from it using the
        // mocked admin user. The new user does not have any favourites. Should
        // result in Not Found.
        // The API does not expose a method to register Admins, so this test
        // will use the fixture's mocked admin to test getting a user.
        [Fact]
        public async Task WhenRoleAdminGetsUserFavourites()
        {
            // Get the fixture's mocked admin user
            var requestUserAdmin = MockAuthentication.StandardTestUserWithRoleAdmin;
            var apiSecurityTokenUserAdmin = await _authenticationService.AuthenticateUser(requestUserAdmin.Username, requestUserAdmin.Password);
            Assert.NotNull(apiSecurityTokenUserAdmin);

            var requestUser = _testUsers.Generate();
            var idUser = (await RegisterGivenUser(requestUser, HttpStatusCode.Created)).ApiAffectedId.Value;

            var response = await AttemptUsersGetSomeFavourites(idUser, apiSecurityTokenUserAdmin);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        // Users.AddFavourite tests

        // NOTE: The API purposely does not support adding and removing venues,
        // which complicates testing, which is the point. Here, we go with
        // using known venues when needed for the tests.

        // Register a user; authenticate it and attempt to add the known
        // favourite. Should result in OK.
        [Theory]
        [MemberData(nameof(TestData.KnownVenues), MemberType = typeof(TestData))]
        public async Task WhenRoleUserAddsKnownFavourites(Dto.ApiVenue venue) 
        {
            var request = _testUsers.Generate();
            var id = (await RegisterGivenUser(request, HttpStatusCode.Created)).ApiAffectedId.Value;
            var apiSecurityToken = await _authenticationService.AuthenticateUser(request.Username, request.Password);
            Assert.NotNull(apiSecurityToken);

            var response = await AttemptUsersAddFavourite(id, venue.Id, apiSecurityToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Get the fixture's registered admin user; authenticate it and attempt
        // to add the known favourite. Should result in OK.
        [Theory]
        [MemberData(nameof(TestData.KnownVenues), MemberType = typeof(TestData))]
        public async Task WhenRoleAdminAddsKnownFavourites(Dto.ApiVenue venue)
        {
            // Get the fixture's mocked admin user
            var request = MockAuthentication.StandardTestUserWithRoleAdmin;
            var id = (await AttemptUsersGetByUsername(request.Username)).ApiUser.Id;
            var apiSecurityToken = await _authenticationService.AuthenticateUser(request.Username, request.Password);
            Assert.NotNull(apiSecurityToken);

            var response = await AttemptUsersAddFavourite(id, venue.Id, apiSecurityToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Get the fixture's registered user with role Everyone; authenticate it
        // and attempt to add the known favourite. Should result in OK.
        [Theory]
        [MemberData(nameof(TestData.KnownVenues), MemberType = typeof(TestData))]
        public async Task WhenRoleEveryoneAddsKnownFavourites(Dto.ApiVenue venue)
        {
            // Get the fixture's mocked user with role Everyone
            var request = MockAuthentication.StandardTestUserWithRoleEveryone;
            var id = (await AttemptUsersGetByUsername(request.Username)).ApiUser.Id;
            var apiSecurityToken = await _authenticationService.AuthenticateUser(request.Username, request.Password);
            Assert.NotNull(apiSecurityToken);

            var response = await AttemptUsersAddFavourite(id, venue.Id, apiSecurityToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Register a user; do not authenticate it and attempt to add the known
        // favourite. Should result in Unauthorized.
        [Theory]
        [MemberData(nameof(TestData.KnownVenues), MemberType = typeof(TestData))]
        public async Task WhenUnauthorisedUserAddsKnownFavourites(Dto.ApiVenue venue)
        {
            var request = _testUsers.Generate();
            var id = (await RegisterGivenUser(request, HttpStatusCode.Created)).ApiAffectedId.Value;

            var response = await AttemptUsersAddFavourite(id, venue.Id);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // Register 2 users; authenticate the first and attempt to add favourite
        // to the second. Should result in Forbidden.
        [Theory]
        [MemberData(nameof(TestData.KnownVenues), MemberType = typeof(TestData))]
        public async Task WhenRoleUserAddsKnownFavouritesToOtherUser(Dto.ApiVenue venue)
        {
            var requestUser1 = _testUsers.Generate();
            await RegisterGivenUser(requestUser1, HttpStatusCode.Created);
            var apiSecurityTokenUser1 = await _authenticationService.AuthenticateUser(requestUser1.Username, requestUser1.Password);
            Assert.NotNull(apiSecurityTokenUser1);

            var requestUser2 = _testUsers.Generate();
            var idUser2 = (await RegisterGivenUser(requestUser2, HttpStatusCode.Created)).ApiAffectedId.Value;

            var response = await AttemptUsersAddFavourite(idUser2, venue.Id, apiSecurityTokenUser1);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Get the fixture's registered Admin user and register a second user;
        // authenticate the first and attempt to add favourite to the second.
        // Should result in OK.
        [Theory]
        [MemberData(nameof(TestData.KnownVenues), MemberType = typeof(TestData))]
        public async Task WhenRoleAdminAddsKnownFavouritesToOtherUser(Dto.ApiVenue venue)
        {
            var requestUser1 = MockAuthentication.StandardTestUserWithRoleAdmin;
            await AttemptUsersGetByUsername(requestUser1.Username);
            var apiSecurityTokenUser1 = await _authenticationService.AuthenticateUser(requestUser1.Username, requestUser1.Password);
            Assert.NotNull(apiSecurityTokenUser1);

            var requestUser2 = _testUsers.Generate();
            var idUser2 = (await RegisterGivenUser(requestUser2, HttpStatusCode.Created)).ApiAffectedId.Value;

            var response = await AttemptUsersAddFavourite(idUser2, venue.Id, apiSecurityTokenUser1);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

    }
}
