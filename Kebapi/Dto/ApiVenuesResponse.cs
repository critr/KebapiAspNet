using System.Collections.Generic;

namespace Kebapi.Dto
{
    public class ApiVenuesResponse 
    {
        public ApiStatus ApiStatus { get; set; }
        public List<ApiVenue> ApiVenue { get; set; }
    }
}
