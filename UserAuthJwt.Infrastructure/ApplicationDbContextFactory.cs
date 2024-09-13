
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserAuthJwt.Infrastructure
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = "Server=localhost;Database=user_auth;User=root;Password=Phrdc2022@5rdrt;TreatTinyAsBoolean=false;";

            // Specify the MySQL version you are using. Example: MySQL 8.0.29
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

            // Use Pomelo's UseMySql method to configure the connection
            optionsBuilder.UseMySql(connectionString, serverVersion,
                options => options.EnableRetryOnFailure() // Optional: Enables retry on transient failures
            );

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
