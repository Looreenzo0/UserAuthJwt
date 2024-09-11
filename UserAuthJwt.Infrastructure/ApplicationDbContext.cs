

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UserAuthJwt.Domain.Entities;

namespace UserAuthJwt.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Contact> Contacts { get; set; }
    }
}
