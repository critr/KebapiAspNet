using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Kebapi.Test
{
    // Tests on Venues.
    // Note that Authorisation tests are handled elsewhere (AuthorisationTests.cs),
    // however Venues do not currently have any access restrictions.

    // Note: When performing Gets for Users and Venues, the API has the following
    // paging traits:
    // - Non-numeric values for start row and row count are converted to zero.
    // - For row count, zero means 'all' or no limit other than page size for
    //   row count.
    // - For start row, zero means start at first.
    // - Negative numerics are interpreted as non-negative.

    [Collection(WebServerCollection.Name)]
    public class VenueTests : IClassFixture<WebServerFixture>
    {
        private static HttpClient _client;

        public VenueTests(WebServerFixture fixture)
        {
            _client = fixture.CreateClient();
        }

        // Venues.Get helper.
        private static async Task<Dto.ApiVenueResponse> GetVenue(object id, HttpStatusCode expectedStatusCode)
        {
            var httpResponse = await _client.GetAsync($"/venues/{id}");
            Assert.Equal(expectedStatusCode, httpResponse.StatusCode);
            var content = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            return JsonSerializer.Deserialize<Dto.ApiVenueResponse>(content);
        }

        // Take an inexistent id as venue id; get that venue.
        // Should result in Not Found.
        [Theory]
        [MemberData(nameof(TestData.InexistentIds), MemberType = typeof(TestData))]
        public async Task WhenGettingAnInexistentVenue(int inexistentId)
        {
            var response = await GetVenue(inexistentId, HttpStatusCode.NotFound);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Get venue did not return a result.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.NotFound, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiVenue);
        }

        // Take an invalid id as venue id; get that venue.
        // Should result in Bad Request.
        [Theory]
        [MemberData(nameof(TestData.InvalidIds), MemberType = typeof(TestData))]
        public async Task WhenGettingAVenueWithInvalidId(object invalidId)
        {
            var response = await GetVenue(invalidId, HttpStatusCode.BadRequest);

            Assert.NotEmpty(response.ApiStatus.Errors);
            Assert.Single(response.ApiStatus.Errors);
            Assert.Contains("Missing an expected integer (greater than 0) argument: id. The value supplied was '", response.ApiStatus.Errors[0]);
            Assert.Equal("Cannot invoke Get venue.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.BadRequest, response.ApiStatus.StatusCode);
            Assert.Null(response.ApiVenue);
        }

        // NOTE: The API purposely does not support adding and removing venues,
        // which complicates testing, which is the point. Here, we go with
        // using known venues when needed for the tests.

        // Take a known venue; get that venue.
        // Should result in OK. Venue details should match.
        [Theory]
        [MemberData(nameof(TestData.KnownVenues), MemberType = typeof(TestData))]
        public async Task WhenGettingAKnownVenue(Dto.ApiVenue knownVenue)
        {
            var response = await GetVenue(knownVenue.Id, HttpStatusCode.OK);

            Assert.Empty(response.ApiStatus.Errors);
            Assert.Equal("Get venue returned a result.", response.ApiStatus.Message);
            Assert.Equal(HttpStatusCode.OK, response.ApiStatus.StatusCode);

            Assert.NotNull(response.ApiVenue);
            Assert.Equal(knownVenue.Address, response.ApiVenue.Address);
            Assert.Equal(knownVenue.GeoLat, response.ApiVenue.GeoLat);
            Assert.Equal(knownVenue.GeoLng, response.ApiVenue.GeoLng);
            Assert.Equal(knownVenue.Id, response.ApiVenue.Id);
            Assert.Equal(knownVenue.MainMediaPath, response.ApiVenue.MainMediaPath);
            Assert.Equal(knownVenue.Name, response.ApiVenue.Name);
            Assert.Equal(knownVenue.Rating, response.ApiVenue.Rating);
        }

    }
}
