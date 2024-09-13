
using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data;
using UserAuthJwt.Application.Models;

namespace UserAuthJwt.Infrastructure.Services
{
    public interface IContactDapperService
    {
        Task<IEnumerable<ContactModel>> GetAllContacts();
    }

    public class ContactDapperService : IContactDapperService
    {
        private readonly DapperDbContext _dbContext;
        public ContactDapperService(DapperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        //private IDbConnection CreateConnection()
        //{
        //    // Use the connection string from configuration
        //    var connectionString = _config.GetConnectionString("DefaultConnection")
        //        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        //    return new MySqlConnection(connectionString);
        //}

        public async Task<IEnumerable<ContactModel>> GetAllContacts()
        {
            using (var connection = _dbContext.CreateConnection())
            {
                const string sql = @"
                    SELECT
                        c.Id,
                        c.FirstName,
                        c.LastName,
                        u.Username,
                        r.Name AS RoleName,
                        c.Email,
                        c.PhoneNumber
                    FROM Contacts c
                    LEFT JOIN Users u ON c.Id = u.ContactId
                    LEFT JOIN Roles r ON u.RoleId = r.Id
                ";

                var contacts = await connection.QueryAsync<ContactModel>(sql);

                return contacts;
            }
        }
    }
}
