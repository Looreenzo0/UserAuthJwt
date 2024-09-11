

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
            var q = await (from o in _context.Contacts
                           join o1 in _context.Users on o.Id equals o1.ContactId into o1Default
                           from o1 in o1Default.DefaultIfEmpty()
                           join o2 in _context.Roles on o1.RoleId equals o2.Id into o2Default
                           from o2 in o2Default.DefaultIfEmpty()

                           select new ContactModel
                           {
                               Id = o.Id,
                               FirstName = o.FirstName,
                               LastName = o.LastName,
                               Username = o1.Username,
                               RoleName = o2.Name,
                               Email = o.Email,
                               PhoneNumber = o.PhoneNumber
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
    }
}
