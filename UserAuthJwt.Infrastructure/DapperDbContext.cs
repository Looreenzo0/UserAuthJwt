using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data;

namespace UserAuthJwt.Infrastructure
{
    public class DapperDbContext
    {
        private readonly IConfiguration _config;

        public DapperDbContext(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection CreateConnection()
        {
            var connectionString = _config.GetConnectionString("DefaultConnection")
              ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            return new MySqlConnection(connectionString);
        }

    }
}
