
namespace UserAuthJwt.Application.Models
{
    public class ContactModel
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? RoleName { get; set; }
        public string? Photo { get; set; }
    }
}
