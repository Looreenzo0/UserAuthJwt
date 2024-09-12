

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserAuthJwt.Application.Dto;
using UserAuthJwt.Application.Models;
using UserAuthJwt.Domain.Entities;

namespace UserAuthJwt.Infrastructure.Services
{
    public interface IUserService
    {
        Task<User> Authenticate(string username, string password);
        Task<UserDto> Register(RegisterModel model);
        Task DeleteUser(int userId);
        Task<IEnumerable<ContactModel>> GetAllUsers();
    }
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public UserService(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<User> Authenticate(string username, string password)
        {
            var user = await _context.Users
                .Include(u => u.Contact)
                .SingleOrDefaultAsync(u => u.Username == username);

            if (user == null || !VerifyPasswordHash(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<UserDto> Register(RegisterModel model)
        {
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                throw new Exception("Username already exists.");
            }

            var contact = new Contact
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            await _context.Contacts.AddAsync(contact);
            await _context.SaveChangesAsync();

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == model.RoleName);
            if (role == null)
            {
                throw new Exception("Role not found.");
            }

            var user = new User
            {
                Username = model.Username,
                ContactId = contact.Id,
                PasswordHash = CreatePasswordHash(model.Password),
                RoleId = role.Id
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                ContactId = user.ContactId,
                RoleId = user.RoleId
            };
        }

        public async Task<IEnumerable<ContactModel>> GetAllUsers()
        {
            var q = await (from o in _context.Users
                           join o1 in _context.Contacts on o.ContactId equals o1.Id into o1Default
                           from o1 in o1Default.DefaultIfEmpty()
                           join o2 in _context.Roles on o.RoleId equals o2.Id into o2Default
                           from o2 in o2Default.DefaultIfEmpty()

                           select new ContactModel
                           {
                               Id = o.Id,
                               FirstName = o1.FirstName,
                               LastName = o1.LastName,
                               Username = o.Username,
                               Password = o.PasswordHash,
                               RoleName = o2.Name,
                               Email = o1.Email,
                               PhoneNumber = o1.PhoneNumber,
                               Photo = "https://images.pexels.com/photos/1043474/pexels-photo-1043474.jpeg?auto=compress&cs=tinysrgb&w=1200"
                           }).AsNoTracking().ToListAsync();

            return q;
        }

        private string CreatePasswordHash(string password)
        {
            // Implement a proper password hashing logic here (e.g., BCrypt)
            return password; // For simplicity, storing the plain password here. Change this for production.
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            // Implement hash verification logic here
            // For simplicity, assuming the storedHash is the plain password in this example
            return password == storedHash;
        }

        public async Task DeleteUser(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            // Find the associated contact
            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == user.ContactId);
            if (contact == null)
            {
                throw new Exception("Associated contact not found.");
            }

            // Remove the user and the associated contact
            _context.Users.Remove(user);
            _context.Contacts.Remove(contact);

            // Save changes to the database
            await _context.SaveChangesAsync();
        }
    }
}
