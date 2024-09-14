
namespace UserAuthJwt.Application.Dto
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int ContactId { get; set; }
        public int RoleId { get; set; }

        public string RoleName { get; set; }
    }
}
