
namespace Kebapi.Dto
{
    public class DalUser 
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        //public byte RoleId { get; set; }
        public string Role { get; set; }
        public byte AccountStatusId { get; set; }
    }
}
