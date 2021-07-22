using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Kebapi.Domain;
using System;

namespace Kebapi.Test
{
    // Tests on Users.
    // Note that Authorisation tests are handled elsewhere (AuthorisationTests.cs),
    // so these tests all run with mock Admin-level access rights.

    // Note: When performing Gets for Users and Venues, the API has the following
    // paging traits:
    // - Non-numeric values for start row and row count are converted to zero.
    // - For row count, zero means 'all' or no limit other than page size for
    //   row count.
    // - For start row, zero means start at first.
    // - Negative numerics are interpreted as non-negative.

    [Collection(WebServerCollection.Name)]
    public class UserTests : IClassFixture<WebServerFixture>
    {
        private static HttpClient _client;
        private readonly Bogus.Faker<Dto.ApiRegisterUser> _testUsers;
        private readonly TokenValidationSettings _tokenValidationSettings;
        public UserTests(WebServerFixture fixture)
        {
            _client = fixture.CreateClient();

            // Faker for generating test users.
            _testUsers = new Bogus.Faker<Dto.ApiRegisterUser>()
                .StrictMode(true)
                .RuleFor(x => x.Name, y => y.Person.FirstName)
                .RuleFor(x => x.Surname, y => y.Person.LastName)
                .RuleFor(x => x.Username, y => y.Person.UserName)
                .RuleFor(x => x.Email, (y, x) => y.Internet.Email(x.Name, x.Surname))
                .RuleFor(x => x.Password, y => y.Internet.Password(10))
                ;

            // Any application settings required.
            /* 
             * TODO: Review if we really want to be taking settings from config in
             * tests in preference to setting our own. For now we'll just grab the 
             * token settings from config.
             */
            _tokenValidationSettings = fixture.Services
                .GetService<TokenValidationSettings>();

            // Authorisation tests are done in AuthorisationTests.cs not here.
            // For these tests, mock an Admin-level bearer token that is used
            // for all relevant requests in these tests.
            var ma = fixture.Services.GetService<MockAuthentication>();
            var mockUser = MockAuthentication.StandardTestUserWithRoleAdmin;
            // Note: we can substitute for a real user like this:
            //var realUser = new MockAuthentication.User 
            //{
            //    Username = "Babs",
            //    Name = "Lucy",
            //    Surname = "Matthews",
            //    Email = "babs@matthews.co.uk",
            //    Password = "lucy1",
            //    Role = User.Role.User,
            //    AccountStatus = User.AccountStatus.Active,
            //};
            var apiSecurityToken = ma.AuthenticateUser(mockUser);
            // Add the token as a bearer token on the request header.
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers
                .AuthenticationHeaderValue("Bearer", apiSecurityToken.Token);
        }


        // Users.Add helper.
        // Attempt a user registration with the user request it's given,
        // returning the response.
        private static async Task<Dto.ApiAffectedIdResponse> RegisterGivenUser(
            Dto.ApiRegisterUser request, HttpStatusCode expectedStatusCode)
        {
            var json = JsonSerializer.Serialize(request);
            var httpResponse = await _client.PostAsync($"/users/register", new StringContent(json));
            Assert.Equal(expectedStatusCode, httpResponse.StatusCode);
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            return JsonSerializer.Deserialize<Dto.ApiAffectedIdResponse>(content);
        }

        // Users.Add helper.
        // Attempt a user registration with a newly generated fake user.
        private async Task<Dto.ApiAffectedIdResponse> RegisterNewFakeUser()
        {
            var request = _testUsers.Generate();
            var response = await RegisterGivenUser(request, HttpStatusCode.Created);
            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("User registered.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.Created, response.ApiStatus.StatusCode);
            return response;
        }


        // - - - - Authenticate User Tests - - - - -

        // Users.Authenticare helper.
        private static async Task<Dto.ApiUserLoginResponse> AuthenticateGivenUser(
            Dto.ApiUserLogin request, HttpStatusCode expectedStatusCode)
        {
            var json = JsonSerializer.Serialize(request);
            var httpResponse = await _client.PostAsync("users/auth", new StringContent(json));
            Assert.Equal(expectedStatusCode, httpResponse.StatusCode);
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            return JsonSerializer.Deserialize<Dto.ApiUserLoginResponse>(content);
        }
        // Users.Authenticare helper. Helper for testing against unexpected requests.
        private static async Task<Dto.ApiUserLoginResponse> AuthenticateGivenUser(
            object request)
        {
            var json = JsonSerializer.Serialize(request);
            var httpResponse = await _client.PostAsync("users/auth", new StringContent(json));
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            return JsonSerializer.Deserialize<Dto.ApiUserLoginResponse>(content);
        }
        // Users.Authenticare helper. Helper for testing against empty requests.
        private static async Task<Dto.ApiUserLoginResponse> AuthenticateGivenEmptyRequest()
        {
            var httpResponse = await _client.PostAsync("users/auth", new StringContent(""));
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            return JsonSerializer.Deserialize<Dto.ApiUserLoginResponse>(content);
        }

        // Register a user; try to authenticate them using either their email or
        // username, plus their password. Repeat n times.
        // Should result in OK.
        [Theory]
        [InlineData(5, true)]
        [InlineData(5, false)]
        public async Task WhenAuthenticatingARegisteredUser(int iterations,
            bool useEmail)
        {
            for (int i = 0; i < iterations; i++)
            {
                var request = _testUsers.Generate();
                await RegisterGivenUser(request, HttpStatusCode.Created);
                Dto.ApiUserLogin loginRequest = new()
                {
                    UsernameOrEmail = useEmail
                        ? request.Email : request.Username,
                    Password = request.Password
                };
                var response = await AuthenticateGivenUser(loginRequest,
                    HttpStatusCode.OK);
                Assert.Empty(response.ApiStatus.Errors);
                Assert.Equal("Authentication succeeded.",
                    response.ApiStatus.Message);

                Assert.NotNull(response.ApiSecurityToken);
                Assert.NotNull(response.ApiSecurityToken.Token);

                var tokenExpireMinutes = _tokenValidationSettings.ExpireMinutes;
                Assert.True(tokenExpireMinutes >= 0,
                    $"Expected TokenValidationSettings.ExpireMinutes >= 0, got {tokenExpireMinutes}");

                // Could be discrepancies here if tests ever run on a different
                // box with different time, or if test runs very slow, or if
                // interval ever changes to something smaller than minutes.
                // All unlikely for this project, but noting it anyway.                 
                var now = DateTime.UtcNow;
                // Strip out unwanted portions of DateTime like Ticks and
                // Milliseconds by creating an explicitly formatted version of
                // now. We're only interested in time comparisons down to the
                // second for these Asserts.
                var nowFormatted = new DateTime(now.Year,
                                                now.Month,
                                                now.Day,
                                                now.Hour,
                                                now.Minute,
                                                now.Second);
                // Test minutes-to-expiry from settings matches the token expiry
                // date that came back with the token.
                Assert.Equal(tokenExpireMinutes,
                    (response.ApiSecurityToken.Expires - nowFormatted)
                    .TotalMinutes);
                // Test token expires when expected.
                var expires = nowFormatted.AddMinutes(tokenExpireMinutes);
                Assert.Equal(expires, response.ApiSecurityToken.Expires);
                // Alternative to using nowFormatted:
                //  Assert.Equal(expires.ToString("yyyy-MM-dd hh:mm:ss"),
                //      response.ApiSecurityToken.Expires.ToString("yyyy-MM-dd hh:mm:ss"));

            }
        }

        // Create a user without registering them; try to authenticate them using
        // either their email or username, plus their password. Repeat n times.
        // Should result in Unauthorized.
        [Theory]
        [InlineData(5, true)]
        [InlineData(5, false)]
        public async Task WhenAuthenticatingAnUnregisteredUser(int iterations,
            bool useEmail)
        {
            for (int i = 0; i < iterations; i++)
            {
                var request = _testUsers.Generate();
                Dto.ApiUserLogin loginRequest = new()
                {
                    UsernameOrEmail = useEmail
                        ? request.Email : request.Username,
                    Password = request.Password
                };
                var response = await AuthenticateGivenUser(loginRequest,
                    HttpStatusCode.Unauthorized);
                Assert.Empty(response.ApiStatus.Errors);
                Assert.Equal("Authentication failed.",
                    response.ApiStatus.Message);

                Assert.Null(response.ApiSecurityToken);
            }
        }

        // Take an unexpected request; try to use it as the user to authenticate.
        // Should result in Bad Request.
        [Theory]
        [MemberData(nameof(TestData.UnexpectedRequests), MemberType = typeof(TestData))]
        public async Task WhenAuthenticatingWithUnexpectedRequest(string json)
        {
            object unexpectedRequest = JsonSerializer.Deserialize<object>(json);
            var response = await AuthenticateGivenUser(unexpectedRequest);
            Assert.Equal(HttpStatusCode.BadRequest,
                response.ApiStatus.StatusCode);
            Assert.NotEmpty(response.ApiStatus.Errors);
            Assert.NotNull(response.ApiStatus.Errors[0]);
            Assert.Contains("Missing needed info:",
                response.ApiStatus.Errors[0]);

            Assert.True(
                response.ApiStatus.Errors[0].Contains("UsernameOrEmail")
                ||
                response.ApiStatus.Errors[0].Contains("Password")
                );
            Assert.Equal("Wonky info received.",
                response.ApiStatus.Message);

            Assert.Null(response.ApiSecurityToken);
        }

        // Take an empty request; try to use it as the user to authenticate.
        // Should result in Bad Request.
        [Fact]
        public async Task WhenAuthenticatingWithEmptyRequest()
        {
            var response = await AuthenticateGivenEmptyRequest();
            Assert.Equal(HttpStatusCode.BadRequest,
                response.ApiStatus.StatusCode);
            Assert.Null(response.ApiStatus.Errors);
            Assert.Equal("Wonky login request received. Check the request method and body.",
                response.ApiStatus.Message);

            Assert.Null(response.ApiSecurityToken);
        }

        // - - - - /Authenticate User Tests - - - - 


        // - - - - Add User Favourite Tests - - - -

        // Users.AddFavourite helper.
        private static async Task<Dto.ApiAffectedIdResponse> AddUserFavourite(
            object id, object venueId, HttpStatusCode expectedStatusCode)
        {
            var httpResponse = await _client.PostAsync($"/users/{id}/favourites/{venueId}", null);
            Assert.Equal(expectedStatusCode, httpResponse.StatusCode);
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            return JsonSerializer.Deserialize<Dto.ApiAffectedIdResponse>(content);
        }

        // Register a user; add a known favourite.
        // Should result in OK.
        [Theory]
        [MemberData(nameof(TestData.KnownVenues), MemberType = typeof(TestData))]
        public async Task WhenAddingAKnownVenueToAUsersFavourites(Dto.ApiVenue knownVenue)
        {
            var id = (await RegisterNewFakeUser()).ApiAffectedId.Value;
            var response = await AddUserFavourite(id, knownVenue.Id, HttpStatusCode.OK);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Favourite added.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, response.ApiStatus.StatusCode);
            Assert.True(response.ApiAffectedId.Value > 0, $"Expected Id > 0, got {response.ApiAffectedId.Value}");
        }

        // Register a user; try to add a favourite using an invalid id.
        // Should result in Bad Request.
        [Theory]
        [MemberData(nameof(TestData.InvalidIds), MemberType = typeof(TestData))]
        public async Task WhenAddingAnInvalidVenueToAUsersFavourites(object invalidVenueId)
        {
            var id = (await RegisterNewFakeUser()).ApiAffectedId.Value;
            var response = await AddUserFavourite(id, invalidVenueId, HttpStatusCode.BadRequest);

            Assert.NotEmpty(response.ApiStatus.Errors);
            Assert.Single(response.ApiStatus.Errors);
            Assert.Contains("Missing an expected integer (greater than 0) argument: venueId. The value supplied was '", response.ApiStatus.Errors[0]);
            Assert.Equal("Cannot invoke Add favourite.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.BadRequest, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiAffectedId);
        }

        // Take a known venue; try to add it as a favourite to a user that has an
        // invalid id.
        // Should result in Bad Request.
        [Theory]
        [MemberData(nameof(TestData.InvalidIds), MemberType = typeof(TestData))]
        public async Task WhenAddingAKnownVenueToAnInvalidUsersFavourites(object invalidUserId)
        {
            var knownVenueId = ((Dto.ApiVenue)TestData.KnownVenues.First().First()).Id;
            var response = await AddUserFavourite(invalidUserId, knownVenueId, HttpStatusCode.BadRequest);

            Assert.NotEmpty(response.ApiStatus.Errors);
            Assert.Single(response.ApiStatus.Errors);
            Assert.Contains("Missing an expected integer (greater than 0) argument: id. The value supplied was '", response.ApiStatus.Errors[0]);
            Assert.Equal("Cannot invoke Add favourite.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.BadRequest, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiAffectedId);
        }

        // Register a user and add a known favourite; try to add the same favourite
        // again.
        // Should result in OK.
        [Theory]
        [MemberData(nameof(TestData.KnownVenues), MemberType = typeof(TestData))]
        public async Task WhenAddingAnAlreadyAddedKnownVenueToAUsersFavourites(Dto.ApiVenue knownVenue)
        {
            var id = (await RegisterNewFakeUser()).ApiAffectedId.Value;
            var addResponse = await AddUserFavourite(id, knownVenue.Id, HttpStatusCode.OK);

            Assert.Empty(addResponse.ApiStatus.Errors);
            Assert.Equal("Favourite added.", addResponse.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, addResponse.ApiStatus.StatusCode);
            Assert.True(addResponse.ApiAffectedId.Value > 0, $"Expected Id > 0, got {addResponse.ApiAffectedId.Value}");

            var response = await AddUserFavourite(id, knownVenue.Id, HttpStatusCode.OK);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Favourite already exists.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiAffectedId.Value);
        }

        // Take a known venue; try to add it as a favourite to a user that has an
        // inexistent id.
        // Should result in Unprocessable Entity.
        [Theory]
        [MemberData(nameof(TestData.InexistentIds), MemberType = typeof(TestData))]
        public async Task WhenAddingAKnownVenueToAnInexistentUsersFavourites(object inexistentUserId)
        {
            var knownVenueId = ((Dto.ApiVenue)TestData.KnownVenues.First().First()).Id;
            var response = await AddUserFavourite(inexistentUserId, knownVenueId, HttpStatusCode.UnprocessableEntity);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Cannot add favourite. User and/or venue does not exist.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiAffectedId.Value);
        }

        // Register a user and take an inexistent id as a venue id; try to add it
        // as a favourite to the user.
        // Should result in Unprocessable Entity.
        [Theory]
        [MemberData(nameof(TestData.InexistentIds), MemberType = typeof(TestData))]
        public async Task WhenAddingAnInexistentVenueToAUsersFavourites(object inexistentVenueId)
        {
            var id = (await RegisterNewFakeUser()).ApiAffectedId.Value;
            var response = await AddUserFavourite(id, inexistentVenueId, HttpStatusCode.UnprocessableEntity);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Cannot add favourite. User and/or venue does not exist.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiAffectedId.Value);
        }


        // - - - - /Add User Favourite Tests - - - -


        // - - - - Get User Favourite Tests - - - -

        // Venues.GetSome helper.
        private static async Task<Dto.ApiVenuesResponse> GetSomeVenues(HttpStatusCode expectedStatusCode)
        {
            var httpResponse = await _client.GetAsync($"/venues");
            Assert.Equal(expectedStatusCode, httpResponse.StatusCode);
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            return JsonSerializer.Deserialize<Dto.ApiVenuesResponse>(content);
        }

        // Users.GetSomeFavourites helper.
        private static async Task<Dto.ApiVenuesResponse> GetSomeUserFavourites(object id, HttpStatusCode expectedStatusCode)
        {
            var httpResponse = await _client.GetAsync($"/users/{id}/favourites");
            Assert.Equal(expectedStatusCode, httpResponse.StatusCode);
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            return JsonSerializer.Deserialize<Dto.ApiVenuesResponse>(content);
        }

        // Register a user; get some venues and add them as favourites; attempt
        // to get that user's favourites.
        // Should result in OK, with matching venues.
        [Fact]
        public async Task WhenGettingSomeKnownFavouritesFromAUser()
        {
            var id = (await RegisterNewFakeUser()).ApiAffectedId.Value;
            var availableVenuesResponse = await GetSomeVenues(HttpStatusCode.OK);

            Assert.Empty(availableVenuesResponse.ApiStatus.Errors);
            Assert.Equal("Get venues returned a result.", availableVenuesResponse.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, availableVenuesResponse.ApiStatus.StatusCode);

            Assert.True(availableVenuesResponse.ApiVenue.Count >= 3, $"Test requires minimum of 3 Venues in test data, got {availableVenuesResponse.ApiVenue.Count}");

            foreach (var venue in availableVenuesResponse.ApiVenue)
            {
                var addResponse = await AddUserFavourite(id, venue.Id, HttpStatusCode.OK);
                Assert.Empty(addResponse.ApiStatus.Errors);
                Assert.Equal("Favourite added.", addResponse.ApiStatus.Message);
                Assert.Equal(HttpStatusCode.OK, addResponse.ApiStatus.StatusCode);
                Assert.True(addResponse.ApiAffectedId.Value > 0, $"Expected Id > 0, got {addResponse.ApiAffectedId.Value}");
            }

            var response = await GetSomeUserFavourites(id, HttpStatusCode.OK);
            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Get user favourites returned a result.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, response.ApiStatus.StatusCode);

            foreach (var venue in response.ApiVenue)
            {
                //Using Assert.Contains(entity, entityCollection) would require implementing
                //IEquatable or overriding Equals in a Dto, so going with a bloaty LINQ comparison.
                Assert.True(availableVenuesResponse.ApiVenue.Any(
                    o =>
                    o.Address == venue.Address &&
                    o.GeoLat == venue.GeoLat &&
                    o.GeoLng == venue.GeoLng &&
                    o.Id == venue.Id &&
                    o.MainMediaPath == venue.MainMediaPath &&
                    o.Name == venue.Name &&
                    o.Rating == venue.Rating), $"Expected venue {JsonSerializer.Serialize(venue)} to exist.");
            }
        }


        // - - - - /Get User Favourite Tests - - - -



        // - - - - Remove User Favourite Tests - - - -

        // Users.RemoveFavourite helper.
        private static async Task<Dto.ApiAffectedIdResponse> RemoveUserFavourite(object id, object venueId, HttpStatusCode expectedStatusCode)
        {
            var httpResponse = await _client.DeleteAsync($"/users/{id}/favourites/{venueId}");
            Assert.Equal(expectedStatusCode, httpResponse.StatusCode);
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            return JsonSerializer.Deserialize<Dto.ApiAffectedIdResponse>(content);
        }

        // Register a user and add a known venue as favourite; remove that favourite.
        // Should result in OK.
        [Theory]
        [MemberData(nameof(TestData.KnownVenues), MemberType = typeof(TestData))]
        public async Task WhenRemovingAKnownVenueFromAUsersFavourites(Dto.ApiVenue knownVenue)
        {
            var id = (await RegisterNewFakeUser()).ApiAffectedId.Value;
            var addResponse = await AddUserFavourite(id, knownVenue.Id, HttpStatusCode.OK);

            Assert.Empty(addResponse.ApiStatus.Errors);
            Assert.Equal("Favourite added.", addResponse.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, addResponse.ApiStatus.StatusCode);
            Assert.True(addResponse.ApiAffectedId.Value > 0, $"Expected Id > 0, got {addResponse.ApiAffectedId.Value}");

            var response = await RemoveUserFavourite(id, knownVenue.Id, HttpStatusCode.OK);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Favourite removed.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, response.ApiStatus.StatusCode);
            Assert.Equal(addResponse.ApiAffectedId.Value, response.ApiAffectedId.Value);
        }

        // Register a user and take an invalid id for a venue id; try to remove that
        // venue id from the user's favourites.
        // Should result in Bad Request.
        [Theory]
        [MemberData(nameof(TestData.InvalidIds), MemberType = typeof(TestData))]
        public async Task WhenRemovingAnInvalidVenueFromAUsersFavourites(object invalidVenueId)
        {
            var id = (await RegisterNewFakeUser()).ApiAffectedId.Value;
            var response = await RemoveUserFavourite(id, invalidVenueId, HttpStatusCode.BadRequest);

            Assert.NotEmpty(response.ApiStatus.Errors);
            Assert.Single(response.ApiStatus.Errors);
            Assert.Contains("Missing an expected integer (greater than 0) argument: venueId. The value supplied was '", response.ApiStatus.Errors[0]);
            Assert.Equal("Cannot invoke Remove favourite.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.BadRequest, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiAffectedId);
        }

        // Take a known venue and an invalid id for a user id; try to remove that
        // venue from that user's favourites.
        // Should result in Bad Request.
        [Theory]
        [MemberData(nameof(TestData.InvalidIds), MemberType = typeof(TestData))]
        public async Task WhenRemovingAKnownVenueFromAnInvalidUsersFavourites(object invalidUserId)
        {
            var knownVenueId = ((Dto.ApiVenue)TestData.KnownVenues.First().First()).Id;
            var response = await RemoveUserFavourite(invalidUserId, knownVenueId, HttpStatusCode.BadRequest);

            Assert.NotEmpty(response.ApiStatus.Errors);
            Assert.Single(response.ApiStatus.Errors);
            Assert.Contains("Missing an expected integer (greater than 0) argument: id. The value supplied was '", response.ApiStatus.Errors[0]);
            Assert.Equal("Cannot invoke Remove favourite.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.BadRequest, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiAffectedId);
        }

        // Register a user and take a known venue; add that venue to that user's
        // favourites; remove that venue from that user's favourites; remove that
        // venue from that user's favourites again.
        // Should result in OK.
        [Theory]
        [MemberData(nameof(TestData.KnownVenues), MemberType = typeof(TestData))]
        public async Task WhenRemovingAnAlreadyRemovedKnownVenueToAUsersFavourites(Dto.ApiVenue knownVenue)
        {
            var id = (await RegisterNewFakeUser()).ApiAffectedId.Value;
            var addResponse = await AddUserFavourite(id, knownVenue.Id, HttpStatusCode.OK);

            Assert.Empty(addResponse.ApiStatus.Errors);
            Assert.Equal("Favourite added.", addResponse.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, addResponse.ApiStatus.StatusCode);
            Assert.True(addResponse.ApiAffectedId.Value > 0, $"Expected Id > 0, got {addResponse.ApiAffectedId.Value}");

            var removeResponse = await RemoveUserFavourite(id, knownVenue.Id, HttpStatusCode.OK);

            Assert.Empty(removeResponse.ApiStatus.Errors);
            Assert.Equal("Favourite removed.", removeResponse.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, removeResponse.ApiStatus.StatusCode);
            Assert.Equal(addResponse.ApiAffectedId.Value, removeResponse.ApiAffectedId.Value);

            var response = await RemoveUserFavourite(id, knownVenue.Id, HttpStatusCode.OK);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Favourite does not exist.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiAffectedId.Value);
        }

        // Take a known venue and an inexistent id as user id; try to remove that
        // venue from that user's favourites.
        // Should result in OK. (It doesn't exist, so the request can be considered
        // satisfied.)
        [Theory]
        [MemberData(nameof(TestData.InexistentIds), MemberType = typeof(TestData))]
        public async Task WhenRemovingAKnownVenueFromAnInexistentUsersFavourites(object inexistentUserId)
        {
            var knownVenueId = ((Dto.ApiVenue)TestData.KnownVenues.First().First()).Id;
            var response = await RemoveUserFavourite(inexistentUserId, knownVenueId, HttpStatusCode.OK);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Favourite does not exist.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiAffectedId.Value);
        }

        // Register a user and take an inexistent id as venue id; try to remove that
        // venue from that user's favourites.
        // Should result in OK. (It doesn't exist, so the request can be considered
        // satisfied.)
        [Theory]
        [MemberData(nameof(TestData.InexistentIds), MemberType = typeof(TestData))]
        public async Task WhenRemovingAnInexistentVenueFromAUsersFavourites(object inexistentVenueId)
        {
            var id = (await RegisterNewFakeUser()).ApiAffectedId.Value;
            var response = await RemoveUserFavourite(id, inexistentVenueId, HttpStatusCode.OK);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Favourite does not exist.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiAffectedId.Value);
        }


        // - - - - /Remove User Favourite Tests - - - -



        // - - - - Soft Delete Tests - - - - 

        // Users.Deactivate helper.
        private static async Task<Dto.ApiAffectedRowsResponse> SoftDeleteUser(object id, HttpStatusCode expectedStatusCode)
        {
            var httpResponse = await _client.PatchAsync($"/users/{id}/deactivate", null);
            Assert.Equal(expectedStatusCode, httpResponse.StatusCode);
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            return JsonSerializer.Deserialize<Dto.ApiAffectedRowsResponse>(content);
        }

        // Register a user; soft-delete that user.
        // Should result in OK.
        [Fact]
        public async Task WhenSoftDeletingAUser()
        {
            var id = (await RegisterNewFakeUser()).ApiAffectedId.Value;
            var response = await SoftDeleteUser(id, HttpStatusCode.OK);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Deactivate user succeeded.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, response.ApiStatus.StatusCode);
            Assert.Equal(1, response.ApiAffectedRows.Count);
        }

        // Take an invalid id as user id; soft-delete that user.
        // Should result in Bad Request.
        [Theory]
        [MemberData(nameof(TestData.InvalidIds), MemberType = typeof(TestData))]
        public async Task WhenSoftDeletingAUserWithInvalidId(object invalidId)
        {
            var response = await SoftDeleteUser(invalidId, HttpStatusCode.BadRequest);

            Assert.NotEmpty(response.ApiStatus.Errors);
            Assert.Single(response.ApiStatus.Errors);
            Assert.Contains("Missing an expected integer (greater than 0) argument: id. The value supplied was '", response.ApiStatus.Errors[0]);
            Assert.Equal("Cannot invoke Deactivate user.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.BadRequest, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiAffectedRows);
        }

        // Take an inexistent id as user id; soft-delete that user.
        // Should result in Not Found.
        [Theory]
        [MemberData(nameof(TestData.InexistentIds), MemberType = typeof(TestData))]
        public async Task WhenSoftDeletingAnInexistentUser(int inexistentId)
        {
            var response = await SoftDeleteUser(inexistentId, HttpStatusCode.NotFound);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Deactivate user did nothing. Could not find user.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.NotFound, response.ApiStatus.StatusCode);
            Assert.Equal(0, response.ApiAffectedRows.Count);
        }

        // Take a known user; soft-delete that user.
        // Should result in OK.
        [Theory]
        [MemberData(nameof(TestData.KnownUsers), MemberType = typeof(TestData))]
        public async Task WhenSoftDeletingAKnownUser(Dto.ApiUser knownUser)
        {
            var response = await SoftDeleteUser(knownUser.Id, HttpStatusCode.OK);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Deactivate user succeeded.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, response.ApiStatus.StatusCode);
            Assert.Equal(1, response.ApiAffectedRows.Count);
        }

        // Register a user; soft-delete that user. Repeat n times.
        // Should result in OK.
        [Theory]
        [InlineData(4)]
        [InlineData(3)]
        public async Task SoftDeletingAUserIsIndempotent(int iterations)
        {
            var id = (await RegisterNewFakeUser()).ApiAffectedId.Value;
            for (int i = 0; i < iterations; i++)
            {
                await SoftDeleteUser(id, HttpStatusCode.OK);
            }
        }


        // - - - - /Soft Delete Tests - - - - 



        // - - - - Soft Undelete Tests - - - - 

        // Users.Activate helper.
        private static async Task<Dto.ApiAffectedRowsResponse> SoftUndeleteUser(object id, HttpStatusCode expectedStatusCode)
        {
            var httpResponse = await _client.PatchAsync($"/users/{id}/activate", null);
            Assert.Equal(expectedStatusCode, httpResponse.StatusCode);
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            return JsonSerializer.Deserialize<Dto.ApiAffectedRowsResponse>(content);
        }

        // Register a user; soft-undelete that user.
        // Should result in OK.
        [Fact]
        public async Task WhenSoftUndeletingAUser()
        {
            var id = (await RegisterNewFakeUser()).ApiAffectedId.Value;
            var response = await SoftUndeleteUser(id, HttpStatusCode.OK);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Activate user succeeded.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, response.ApiStatus.StatusCode);
            Assert.Equal(1, response.ApiAffectedRows.Count);
        }

        // Take an invalid id as user id; soft-undelete that user.
        // Should result in Bad Request.
        [Theory]
        [MemberData(nameof(TestData.InvalidIds), MemberType = typeof(TestData))]
        public async Task WhenSoftUndeletingAUserWithInvalidId(object invalidId)
        {
            var response = await SoftUndeleteUser(invalidId, HttpStatusCode.BadRequest);

            Assert.NotEmpty(response.ApiStatus.Errors);
            Assert.Single(response.ApiStatus.Errors);
            Assert.Contains("Missing an expected integer (greater than 0) argument: id. The value supplied was '", response.ApiStatus.Errors[0]);
            Assert.Equal("Cannot invoke Activate user.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.BadRequest, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiAffectedRows);
        }

        // Take an inexistent id as user id; soft-undelete that user.
        // Should result in Not Found.
        [Theory]
        [MemberData(nameof(TestData.InexistentIds), MemberType = typeof(TestData))]
        public async Task WhenSoftUndeletingAnInexistentUser(int inexistentId)
        {
            var response = await SoftUndeleteUser(inexistentId, HttpStatusCode.NotFound);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Activate user did nothing. Could not find user.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.NotFound, response.ApiStatus.StatusCode);
            Assert.Equal(0, response.ApiAffectedRows.Count);
        }

        // Take a known user; soft-undelete that user.
        // Should result in OK.
        [Theory]
        [MemberData(nameof(TestData.KnownUsers), MemberType = typeof(TestData))]
        public async Task WhenSoftUndeletingAKnownUser(Dto.ApiUser knownUser)
        {
            var response = await SoftUndeleteUser(knownUser.Id, HttpStatusCode.OK);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Activate user succeeded.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, response.ApiStatus.StatusCode);
            Assert.Equal(1, response.ApiAffectedRows.Count);
        }

        // Register a user; soft-undelete that user. Repeat n times.
        // Should result in OK.
        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        public async Task SoftUndeletingAnExistingUserIsIndempotent(int iterations)
        {
            var id = (await RegisterNewFakeUser()).ApiAffectedId.Value;
            for (int i = 0; i < iterations; i++)
            {
                await SoftUndeleteUser(id, HttpStatusCode.OK);
            }
        }


        // - - - - /Soft Undelete Tests - - - - 



        // - - - - Get Tests - - - - 

        // Users.Get helper.
        private static async Task<Dto.ApiUserResponse> GetUser(object id, HttpStatusCode expectedStatusCode)
        {
            var httpResponse = await _client.GetAsync($"/users/{id}");
            Assert.Equal(expectedStatusCode, httpResponse.StatusCode);
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            return JsonSerializer.Deserialize<Dto.ApiUserResponse>(content);
        }
        // Users.GetByUsername helper.
        private static async Task<Dto.ApiUserResponse> GetUserByUsername(object username, HttpStatusCode expectedStatusCode)
        {
            var httpResponse = await _client.GetAsync($"/users/find?username={username}");
            Assert.Equal(expectedStatusCode, httpResponse.StatusCode);
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            return JsonSerializer.Deserialize<Dto.ApiUserResponse>(content);
        }

        // Register a user; get that user.
        // Should result in OK. User should match.
        [Fact]
        public async Task WhenGettingAUser()
        {
            var request = _testUsers.Generate();
            var id = (await RegisterGivenUser(request, HttpStatusCode.Created)).ApiAffectedId.Value;
            var response = await GetUser(id, HttpStatusCode.OK);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Get user returned a result.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, response.ApiStatus.StatusCode);
            Assert.Equal(id, response.ApiUser.Id);

            Assert.Equal(request.Email, response.ApiUser.Email);
            Assert.Equal(request.Name, response.ApiUser.Name);
            Assert.Equal(request.Surname, response.ApiUser.Surname);
            Assert.Equal(request.Username, response.ApiUser.Username);
        }

        // Register a user; get that user by username.
        // Should result in OK. User should match.
        [Fact]
        public async Task WhenGettingAUserByUsername()
        {
            var request = _testUsers.Generate();
            var id = (await RegisterGivenUser(request, HttpStatusCode.Created)).ApiAffectedId.Value;
            var response = await GetUser(id, HttpStatusCode.OK);
            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Get user returned a result.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, response.ApiStatus.StatusCode);
            Assert.Equal(id, response.ApiUser.Id);

            Assert.Equal(request.Email, response.ApiUser.Email);
            Assert.Equal(request.Name, response.ApiUser.Name);
            Assert.Equal(request.Surname, response.ApiUser.Surname);
            Assert.Equal(request.Username, response.ApiUser.Username);

            var responseGetByName = await GetUserByUsername(request.Username, HttpStatusCode.OK);
            Assert.Empty(responseGetByName.ApiStatus.Errors);
            Assert.Equal("Get user by username returned a result.", responseGetByName.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, responseGetByName.ApiStatus.StatusCode);
            Assert.Equal(id, responseGetByName.ApiUser.Id);

            Assert.Equal(request.Email, responseGetByName.ApiUser.Email);
            Assert.Equal(request.Name, responseGetByName.ApiUser.Name);
            Assert.Equal(request.Surname, responseGetByName.ApiUser.Surname);
            Assert.Equal(request.Username, responseGetByName.ApiUser.Username);
        }

        // Take a known user; get that user.
        // Should result in OK. User should match.
        [Theory]
        [MemberData(nameof(TestData.KnownUsers), MemberType = typeof(TestData))]
        public async Task WhenGettingAKnownUser(Dto.ApiUser knownUser)
        {
            var response = await GetUser(knownUser.Id, HttpStatusCode.OK);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Get user returned a result.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, response.ApiStatus.StatusCode);

            Assert.NotNull(response.ApiUser);
            Assert.Equal(knownUser.Email, response.ApiUser.Email);
            Assert.Equal(knownUser.Id, response.ApiUser.Id);
            Assert.Equal(knownUser.Name, response.ApiUser.Name);
            Assert.Equal(knownUser.Surname, response.ApiUser.Surname);
            Assert.Equal(knownUser.Username, response.ApiUser.Username);
        }

        // Take an inexistent id as user id; get that user.
        // Should result in Not Found.
        [Theory]
        [MemberData(nameof(TestData.InexistentIds), MemberType = typeof(TestData))]
        public async Task WhenGettingAnInexistentUser(int inexistentId)
        {
            var response = await GetUser(inexistentId, HttpStatusCode.NotFound);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Get user did not return a result.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.NotFound, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiUser);
        }

        // Take an invalid id as user id; get that user.
        // Should result in Bad Request.
        [Theory]
        [MemberData(nameof(TestData.InvalidIds), MemberType = typeof(TestData))]
        public async Task WhenGettingAUserWithInvalidId(object invalidId)
        {
            var response = await GetUser(invalidId, HttpStatusCode.BadRequest);

            Assert.NotEmpty(response.ApiStatus.Errors);
            Assert.Single(response.ApiStatus.Errors);
            Assert.Contains("Missing an expected integer (greater than 0) argument: id. The value supplied was '", response.ApiStatus.Errors[0]);
            Assert.Equal("Cannot invoke Get user.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.BadRequest, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiUser);
        }


        // - - - - /Get Tests - - - - 



        // - - - - Get Some Tests - - - -

        // Users.GetSome helper.
        private static async Task<Dto.ApiUsersResponse> GetSomeUsers(int startRow, int rowCount)
        {
            var httpResponse = await _client.GetAsync($"/users?startRow={startRow}&rowCount={rowCount}");
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            return JsonSerializer.Deserialize<Dto.ApiUsersResponse>(content);
        }

        // Users.GetCount helper.
        private static async Task<int> GetUserCount()
        {
            var httpResponse = await _client.GetAsync($"/users/count");
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            var response = JsonSerializer.Deserialize<Dto.ApiAffectedRowsResponse>(content);
            return response.ApiAffectedRows.Count;
        }

        // See Note on API paging traits.
        // Register n users; then get some, specifying various start rows and row
        // counts.
        // Should result in OK with matching users if returned users can be compared,
        // Not Found otherwise.
        // Note: This test might be more susceptible than others to concurrent db
        // activity, but should be fine for small-scale dev/low volume testing.
        [Theory]
        // Baseline.
        [InlineData(1, 1, 1)]
        // Vary row count.
        [InlineData(1, 1, 2)] 
        [InlineData(1, 1, 0)]
        [InlineData(1, 1, -1)]
        [InlineData(1, 1, -42)]
        // Vary start row.
        [InlineData(1, 2, 1)]
        [InlineData(1, 0, 1)]
        [InlineData(1, -1, 1)]
        [InlineData(1, -42, 1)]
        // Vary number of users to add.
        [InlineData(2, 1, 1)]
        [InlineData(0, 1, 1)]
        [InlineData(-1, 1, 1)]
        [InlineData(-42, 1, 1)]
        // Hit known user boundary.
        [InlineData(5, 8, 1)]
        [InlineData(5, 8, 2)]
        [InlineData(5, 8, 4)]
        [InlineData(5, 8, 5)]
        // Various.
        [InlineData(5, 8, 10)]
        [InlineData(5, 9, 10)]
        [InlineData(5, 9, -2)]
        [InlineData(15, 15, -15)]
        [InlineData(15, 15, 15)] 
        [InlineData(1, 15, 15)]
        [InlineData(0, 0, 0)]
        [InlineData(-2, -3, -1)]
        public async Task WhenGettingSomeUsers(int numberOfUsersToAdd, int startRow, int rowCount)
        {
            var totalUsersBeforeTest = await GetUserCount();
            Assert.True(totalUsersBeforeTest >= 0,
                $"Expected total number of users before test >= 0, got {totalUsersBeforeTest}.");

            // Add (register) the number of users specified.
            List<Dto.ApiRegisterUser> addedUsers = new();
            for (int i = 0; i < numberOfUsersToAdd; i++)
            {
                var request = _testUsers.Generate();
                var response = await RegisterGivenUser(request, HttpStatusCode.Created);
                Assert.Equal(HttpStatusCode.Created, response.ApiStatus.StatusCode);
                addedUsers.Add(request);
            }

            // There may or may not have been users in the data already, so totalUsersAfterTest
            // may or may not equal addedUsers.Count.
            var totalUsersAfterTest = await GetUserCount();
            Assert.True(totalUsersAfterTest >= 0,
                $"Expected total number of users >= 0, got {totalUsersAfterTest}.");

            var getSomeResponse = await GetSomeUsers(startRow, rowCount);
            Assert.IsType<Dto.ApiUsersResponse>(getSomeResponse);
            Assert.NotNull(getSomeResponse.ApiStatus);
            Assert.NotNull(getSomeResponse.ApiStatus.Message);

            // The API turns negative values into positive, so we do the same.
            var parsedStartRow = startRow < 0 ? Math.Abs(startRow) : startRow;
            var parsedRowCount = rowCount < 0 ? Math.Abs(rowCount) : rowCount;

            // if our start row is less that the total rows, and our row count says
            // we want some or all (0 is all upto a limit) of what's there then
            if (parsedStartRow < totalUsersAfterTest && parsedRowCount >= 0)
            {
                // We expect at least some users returned.
                Assert.Equal(HttpStatusCode.OK, getSomeResponse.ApiStatus.StatusCode);
                Assert.Equal("Get users returned a result.", getSomeResponse.ApiStatus.Message);
                Assert.Empty(getSomeResponse.ApiStatus.Errors);
                Assert.NotEmpty(getSomeResponse.ApiUser);
                Assert.True(getSomeResponse.ApiUser.Count >= 0);

                if (totalUsersAfterTest > totalUsersBeforeTest && parsedStartRow >= totalUsersBeforeTest)
                {
                    // We can compare the users we added to the users we got in the
                    // response.

                    // Frame the comparison window.
                    int startPos = parsedStartRow - totalUsersBeforeTest;
                    int endPos = parsedRowCount < totalUsersAfterTest 
                        ? (parsedRowCount - totalUsersBeforeTest)
                        : totalUsersAfterTest - totalUsersBeforeTest;

                    int i = startPos;

                    // Compare the results.
                    while (i <= endPos) 
                    {
                        Assert.Equal(addedUsers[i].Email, getSomeResponse.ApiUser[i - startPos].Email);
                        Assert.Equal(addedUsers[i].Name, getSomeResponse.ApiUser[i - startPos].Name);
                        Assert.Equal(addedUsers[i].Surname, getSomeResponse.ApiUser[i - startPos].Surname);
                        Assert.Equal(addedUsers[i].Username, getSomeResponse.ApiUser[i - startPos].Username);
                        i++;
                    }
                }
                else
                {
                    // We can't compare because the users we've added aren't in
                    // the set returned.
                }
            }
            else
            {
                // We don't expect any users returned.
                Assert.Equal(HttpStatusCode.NotFound, getSomeResponse.ApiStatus.StatusCode);
                Assert.Equal("Get users did not return a result.", getSomeResponse.ApiStatus.Message);
                Assert.Empty(getSomeResponse.ApiStatus.Errors);
                Assert.Null(getSomeResponse.ApiUser);
            }

        }

        // - - - - /Get Some Tests - - - -



        // - - - - Register User Tests - - - - 

        // Register a new user; then get that user.
        // Should result in OK, users should match.
        [Fact]
        public async Task WhenRegisteringANewUser()
        {
            var request = _testUsers.Generate();
            var addResponse = await RegisterGivenUser(request, HttpStatusCode.Created);
            Assert.Empty(addResponse.ApiStatus.Errors);
            Assert.Equal("User registered.", addResponse.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.Created, addResponse.ApiStatus.StatusCode);
            Assert.True(addResponse.ApiAffectedId.Value > 0, $"Expected Id > 0, got {addResponse.ApiAffectedId.Value}");

            var id = addResponse.ApiAffectedId.Value;

            var response = await GetUser(id, HttpStatusCode.OK);
            Assert.Equal(request.Email, response.ApiUser.Email);
            Assert.Equal(id, response.ApiUser.Id);
            Assert.Equal(request.Name, response.ApiUser.Name);
            Assert.Equal(request.Surname, response.ApiUser.Surname);
            Assert.Equal(request.Username, response.ApiUser.Username);
        }

        // Generate a user and give it an invalid username; register that user.
        // Should result in Bad Request.
        [Theory]
        [InlineData("ab")]
        [InlineData("z")]
        [InlineData("")]
        [InlineData(null)]
        public async Task WhenRegisteringANewUserWithInvalidUsername(string invalidUsername)
        {
            var request = _testUsers.Generate();
            request.Username = invalidUsername;
            var response = await RegisterGivenUser(request, HttpStatusCode.BadRequest);

            var e = response.ApiStatus.Errors;
            Assert.NotNull(e);
            Assert.True(e.Count > 0, $"Expected error count > 0, got {e.Count}");
            var isMatch1 = (e.FirstOrDefault(s => s.Contains("Username must be longer. At least 3 in length.")) ?? "").Any();
            var isMatch2 = (e.FirstOrDefault(s => s.Contains("Missing needed info:")) ?? "").Any();
            var isMatch3 = (e.FirstOrDefault(s => s.Contains("Username")) ?? "").Any();
            var expectedMatch = isMatch1 || (isMatch2 && isMatch3);
            Assert.True(expectedMatch, $"Expected error match was false.");

            Assert.Equal("Wonky info received.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.BadRequest, response.ApiStatus.StatusCode);
        }

        // Generate a user and give it an invalid email; register that user.
        // Should result in Bad Request.
        [Theory]
        [InlineData("ab")]
        [InlineData("z")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("ab@")]
        [InlineData("z.")]
        public async Task WhenRegisteringANewUserWithInvalidEmail(string invalidEmail)
        {
            var request = _testUsers.Generate();
            request.Email = invalidEmail;
            var response = await RegisterGivenUser(request, HttpStatusCode.BadRequest);

            var e = response.ApiStatus.Errors;
            Assert.NotNull(e);
            Assert.True(e.Count > 0, $"Expected error count > 0, got {e.Count}");
            var isMatch1 = (e.FirstOrDefault(s => s.Contains("Email doesn't look right.")) ?? "").Any();
            var isMatch2 = (e.FirstOrDefault(s => s.Contains("Missing needed info:")) ?? "").Any();
            var isMatch3 = (e.FirstOrDefault(s => s.Contains("Email")) ?? "").Any();
            var expectedMatch = isMatch1 || (isMatch2 && isMatch3);
            Assert.True(expectedMatch, $"Expected error match was false.");

            Assert.Equal("Wonky info received.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.BadRequest, response.ApiStatus.StatusCode);

        }

        // Generate a user and give it an invalid password; register that user.
        // Should result in Bad Request.
        [Theory]
        [InlineData("ab")]
        [InlineData("z")]
        [InlineData("")]
        [InlineData(null)]
        public async Task WhenRegisteringANewUserWithInvalidPassword(string invalidPassword)
        {
            var request = _testUsers.Generate();
            request.Password = invalidPassword;
            var response = await RegisterGivenUser(request, HttpStatusCode.BadRequest);

            var e = response.ApiStatus.Errors;
            Assert.NotNull(e);
            Assert.True(e.Count > 0, $"Expected error count > 0, got {e.Count}");
            var isMatch1 = (e.FirstOrDefault(s => s.Contains("Password must be longer. At least 8 in length.")) ?? "").Any();
            var isMatch2 = (e.FirstOrDefault(s => s.Contains("Missing needed info:")) ?? "").Any();
            var isMatch3 = (e.FirstOrDefault(s => s.Contains("Password")) ?? "").Any();
            var expectedMatch = isMatch1 || (isMatch2 && isMatch3);
            Assert.True(expectedMatch, $"Expected error match was false.");

            Assert.Equal("Wonky info received.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.BadRequest, response.ApiStatus.StatusCode);
        }

        // Generate a user and register it; register that user again.
        // Should result in Unprocessable Entity.
        [Fact]
        public async Task WhenRegisteringADuplicateUser()
        {
            var request = _testUsers.Generate();
            var addResponse = await RegisterGivenUser(request, HttpStatusCode.Created);
            Assert.Equal("User registered.", addResponse.ApiStatus.Message);
            Assert.Empty(addResponse.ApiStatus.Errors);
            Assert.Equal(HttpStatusCode.Created, addResponse.ApiStatus.StatusCode);

            var response = await RegisterGivenUser(request, HttpStatusCode.UnprocessableEntity);
            Assert.Equal("User is already registered.", response.ApiStatus.Message);
            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.ApiStatus.StatusCode);

            Assert.Null(response.ApiAffectedId);
        }

        // Generate a user and register it; generate a second user and give it the
        // email of the first user and register it.
        // Should result in Unprocessable Entity.
        [Fact]
        public async Task WhenRegisteringANewUserWithAnAlreadyExistingEmail()
        {
            var request1 = _testUsers.Generate();
            var response1 = await RegisterGivenUser(request1, HttpStatusCode.Created);
            Assert.Equal("User registered.", response1.ApiStatus.Message);
            Assert.Empty(response1.ApiStatus.Errors);
            Assert.Equal(HttpStatusCode.Created, response1.ApiStatus.StatusCode);

            var request2 = _testUsers.Generate();
            request2.Email = request1.Email;
            var response2 = await RegisterGivenUser(request2, HttpStatusCode.UnprocessableEntity);
            Assert.Equal("User is already registered.", response2.ApiStatus.Message);
            Assert.Empty(response2.ApiStatus.Errors);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response2.ApiStatus.StatusCode);

            Assert.Null(response2.ApiAffectedId);
        }

        // Generate a user and register it; generate a second user and give it the
        // username of the first user and register it.
        // Should result in Unprocessable Entity.
        [Fact]
        public async Task WhenRegisteringANewUserWithAnAlreadyExistingUsername()
        {
            var request1 = _testUsers.Generate();
            var response1 = await RegisterGivenUser(request1, HttpStatusCode.Created);
            Assert.Equal("User registered.", response1.ApiStatus.Message);
            Assert.Empty(response1.ApiStatus.Errors);
            Assert.Equal(HttpStatusCode.Created, response1.ApiStatus.StatusCode);

            var request2 = _testUsers.Generate();
            request2.Username = request1.Username;
            var response2 = await RegisterGivenUser(request2, HttpStatusCode.UnprocessableEntity);
            Assert.Equal("User is already registered.", response2.ApiStatus.Message);
            Assert.Empty(response2.ApiStatus.Errors);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response2.ApiStatus.StatusCode);

            Assert.Null(response2.ApiAffectedId);
        }


        // - - - - /Register User Tests - - - - 



        // - - - - Get User Status Tests - - - - 

        // Users.GetAccountStatus helper.
        public static async Task<Dto.ApiUserAccountStatusResponse> GetUserStatus(object id, HttpStatusCode expectedStatusCode)
        {
            var httpResponse = await _client.GetAsync($"/users/{id}/status");
            Assert.Equal(expectedStatusCode, httpResponse.StatusCode);
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            return JsonSerializer.Deserialize<Dto.ApiUserAccountStatusResponse>(content);
        }

        // Register a user; get that user's status.
        // Should result in OK.
        [Fact]
        public async Task WhenGettingAUserStatus()
        {
            var id = (await RegisterNewFakeUser()).ApiAffectedId.Value;
            var response = await GetUserStatus(id, HttpStatusCode.OK);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Get user account status returned a result.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, response.ApiStatus.StatusCode);
            Assert.Equal((int)User.AccountStatus.Active, response.ApiUserAccountStatus.Id);
            Assert.Equal("Active", response.ApiUserAccountStatus.Status);
        }

        // Take an inexistent id as user id; get that user's status.
        // Should result in Not Found.
        [Theory]
        [MemberData(nameof(TestData.InexistentIds), MemberType = typeof(TestData))]
        public async Task WhenGettingAnInexistentUserStatus(int inexistentId)
        {
            var response = await GetUserStatus(inexistentId, HttpStatusCode.NotFound);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Get user account status did not return a result.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.NotFound, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiUserAccountStatus);
        }

        // Take an invalid id as user id; get that user's status.
        // Should result in Bad Request.
        [Theory]
        [MemberData(nameof(TestData.InvalidIds), MemberType = typeof(TestData))]
        public async Task WhenGettingAUserStatusWithInvalidId(object invalidId)
        {
            var response = await GetUserStatus(invalidId, HttpStatusCode.BadRequest);

            Assert.NotEmpty(response.ApiStatus.Errors);
            Assert.Single(response.ApiStatus.Errors);
            Assert.Contains("Missing an expected integer (greater than 0) argument: id. The value supplied was '", response.ApiStatus.Errors[0]);
            Assert.Equal("Cannot invoke Get user account status.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.BadRequest, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiUserAccountStatus);
        }


        // - - - - /Get User Status Tests - - - - 

    }
}
