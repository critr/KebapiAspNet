using System;

namespace Kebapi.Dto
{
    public class ApiSecurityToken
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}
