
using System.Data;
using System.Text.Json.Serialization;

namespace UserAuthJwt.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public int ContactId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }

        // Navigation property
        [JsonIgnore]
        public Contact Contact { get; set; }

        // Add RoleId or Role navigation property
        public int RoleId { get; set; }
        [JsonIgnore]
        public Role Role { get; set; } // Assuming Role entity exists
    }
}
