using Xunit;

namespace Kebapi.Test
{
    /// <summary>
    /// Declare and tag a container collection class for Xunit. See 
    /// <see cref="CollectionDefinitionAttribute"/>.
    /// </summary>
    // The interfaces attached to this container class will apply to all members
    // of this collection. So, attaching ICollectionFixture<WebServerFixture> means
    // every member gets a distinct copy of our WebServerFixture to run through.
    // Members in turn are tagged with: [Collection(WebServerCollection.Name)]
    [CollectionDefinition(Name)]
    public class WebServerCollection : ICollectionFixture<WebServerFixture>
    {
        public const string Name = "Web Server API Tests";
    }
}
