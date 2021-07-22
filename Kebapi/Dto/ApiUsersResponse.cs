using System.Collections.Generic;

namespace Kebapi.Dto
{
    public class ApiUsersResponse 
    {
        public ApiStatus ApiStatus { get; set; }
        public List<ApiUser> ApiUser { get; set; }
    }
}
