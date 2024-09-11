
namespace UserAuthJwt.Application.Models
{
    public class RegisterModel
    {
        public string Username { get; set; }
        public string Password { get; set; }

        // Contact details
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        // Optional: Role assignment based on user input
        public string RoleName { get; set; } // Or use RoleId if preferred
    }
}
