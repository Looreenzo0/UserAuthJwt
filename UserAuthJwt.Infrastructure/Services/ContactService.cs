
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserAuthJwt.Application.Models;
using UserAuthJwt.Domain.Entities;

namespace UserAuthJwt.Infrastructure.Services
{
    public interface IContactService
    {
        Task <IEnumerable<ContactModel>> GetAllContacts();
    }
    public class ContactService : IContactService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        public ContactService(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<IEnumerable<ContactModel>> GetAllContacts()
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
    }
}
