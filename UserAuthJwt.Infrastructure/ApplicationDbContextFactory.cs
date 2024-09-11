
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserAuthJwt.Infrastructure
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySQL("Server=localhost;Database=user_auth;User=root;Password=Phrdc2022@5rdrt;TreatTinyAsBoolean=false;");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
