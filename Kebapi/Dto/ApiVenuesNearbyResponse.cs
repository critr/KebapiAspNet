using System.Collections.Generic;

namespace Kebapi.Dto
{
    public class ApiVenuesNearbyResponse 
    {
        public ApiStatus ApiStatus { get; set; }
        public List<ApiVenueDistance> ApiVenueDistances { get; set; }
    }
}
