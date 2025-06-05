using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspNetMvcSample.Models
{
    // https://ef.readthedocs.org/en/latest/

    public class SqliteApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IHostEnvironment env;
        private readonly AppTenant tenant;

        public SqliteApplicationDbContext(IHostEnvironment env, AppTenant tenant)
        {
            this.env = env;
            this.tenant = tenant;
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string tenantDbName = tenant.Name.Replace(" ", "-").ToLowerInvariant();
            string connectionString = $"FileName={tenantDbName}.db";
            optionsBuilder.UseSqlite(connectionString);

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}