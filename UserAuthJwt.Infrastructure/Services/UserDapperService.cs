
using Dapper;
using BCrypt.Net;
using UserAuthJwt.Application.Dto;
using UserAuthJwt.Application.Models;
using UserAuthJwt.Domain.Entities;

namespace UserAuthJwt.Infrastructure.Services
{
    public interface IUserDapperService
    {
        Task<User> Authenticate(string username, string password);
        Task<UserDto> Register(RegisterModel model);
        Task DeleteUser(int userId);
        Task<IEnumerable<ContactModel>> GetAllUsers();
    }
    public class UserDapperService : IUserDapperService
    {
        private readonly DapperDbContext _dbContext;

        public UserDapperService(DapperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> Authenticate(string username, string password)
        {
            using (var connection = _dbContext.CreateConnection())
            {
                const string sql = @"
            SELECT u.*, r.Name AS RoleName
            FROM Users u
            JOIN Roles r ON u.RoleId = r.Id
            WHERE u.Username = @Username
        ";

                var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });

                // Return null if the user doesn't exist or password doesn't match
                return user != null && VerifyPasswordHash(password, user.PasswordHash) ? user : null;
            }
        }


        public async Task<UserDto> Register(RegisterModel model)
        {
            using (var connection = _dbContext.CreateConnection())
            {
                // Check if username already exists
                const string checkUserSql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
                var userExists = await connection.ExecuteScalarAsync<bool>(checkUserSql, new { Username = model.Username });
                if (userExists)
                {
                    throw new Exception("Username already exists.");
                }

                // Insert contact and get the new ID
                const string insertContactSql = @"
            INSERT INTO Contacts (FirstName, LastName, Email, PhoneNumber)
            VALUES (@FirstName, @LastName, @Email, @PhoneNumber);
            SELECT LAST_INSERT_ID();
        ";

                var contactId = await connection.ExecuteScalarAsync<int>(insertContactSql, new
                {
                    model.FirstName,
                    model.LastName,
                    model.Email,
                    model.PhoneNumber
                });

                // Get role ID
                const string getRoleIdSql = "SELECT Id FROM Roles WHERE Name = @RoleName";
                var roleId = await connection.ExecuteScalarAsync<int?>(getRoleIdSql, new { RoleName = model.RoleName });

                if (roleId == null)
                {
                    throw new Exception("Role not found.");
                }

                // Hash the password using BCrypt
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                // Insert user and get the new ID
                const string insertUserSql = @"
            INSERT INTO Users (Username, ContactId, PasswordHash, RoleId, RoleName)
            VALUES (@Username, @ContactId, @PasswordHash, @RoleId, @RoleName);
            SELECT LAST_INSERT_ID();
        ";

                var userId = await connection.ExecuteScalarAsync<int>(insertUserSql, new
                {
                    model.Username,
                    ContactId = contactId,
                    PasswordHash = hashedPassword,  // Save hashed password
                    RoleId = roleId.Value,
                    model.RoleName
                });

                // Fetch the RoleName for the newly created user
                const string getUserWithRoleSql = @"
            SELECT u.Username, u.ContactId, u.RoleId, r.Name AS RoleName
            FROM Users u
            JOIN Roles r ON u.RoleId = r.Id
            WHERE u.Id = @UserId
        ";

                var userDto = await connection.QuerySingleOrDefaultAsync<UserDto>(getUserWithRoleSql, new { UserId = userId });

                return userDto;
            }
        }

        public async Task<IEnumerable<ContactModel>> GetAllUsers()
        {
            const string sql = @"
        SELECT 
            u.Id,
            c.FirstName,
            c.LastName,
            u.Username,
            u.PasswordHash AS Password,
            r.Name AS RoleName,
            c.Email,
            c.PhoneNumber,
            'https://images.pexels.com/photos/1043474/pexels-photo-1043474.jpeg?auto=compress&cs=tinysrgb&w=1200' AS Photo
        FROM Users u
        LEFT JOIN Contacts c ON u.ContactId = c.Id
        LEFT JOIN Roles r ON u.RoleId = r.Id
    ";

            using (var connection = _dbContext.CreateConnection())
            {
                return await connection.QueryAsync<ContactModel>(sql);
            }
        }

        public async Task DeleteUser(int userId)
        {
            using (var connection = _dbContext.CreateConnection())
            {
                // Open the connection synchronously
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Find the user
                        var user = await connection.QuerySingleOrDefaultAsync<User>(
                            "SELECT * FROM Users WHERE Id = @UserId",
                            new { UserId = userId },
                            transaction
                        );

                        if (user == null)
                        {
                            throw new Exception("User not found.");
                        }

                        // Find the associated contact
                        var contact = await connection.QuerySingleOrDefaultAsync<Contact>(
                            "SELECT * FROM Contacts WHERE Id = @ContactId",
                            new { ContactId = user.ContactId },
                            transaction
                        );

                        if (contact == null)
                        {
                            throw new Exception("Associated contact not found.");
                        }

                        // Delete the user
                        await connection.ExecuteAsync(
                            "DELETE FROM Users WHERE Id = @UserId",
                            new { UserId = userId },
                            transaction
                        );

                        // Delete the contact
                        await connection.ExecuteAsync(
                            "DELETE FROM Contacts WHERE Id = @ContactId",
                            new { ContactId = user.ContactId },
                            transaction
                        );

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch
                    {
                        // Rollback the transaction in case of an error
                        transaction.Rollback();
                        throw; // Re-throw the exception to be handled by the caller
                    }
                }
            }
        }
        private string CreatePasswordHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

    }
}
