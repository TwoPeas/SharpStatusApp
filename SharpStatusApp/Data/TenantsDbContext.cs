using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SharpStatusApp.Data
{
    public class TenantsDbContext : IdentityDbContext<ApplicationUser>
    {
        public TenantsDbContext(DbContextOptions<TenantsDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Organizations");
            base.OnModelCreating(modelBuilder);
        }
    }
}
