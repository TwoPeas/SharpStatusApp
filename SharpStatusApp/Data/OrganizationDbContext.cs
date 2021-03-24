using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SharpStatusApp.Data
{
    public class OrganizationDbContext : IdentityDbContext<ApplicationUser>
    {
        public OrganizationDbContext(DbContextOptions<OrganizationDbContext> options)
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
