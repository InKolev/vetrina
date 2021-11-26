using Microsoft.EntityFrameworkCore;
using Vetrina.Server.Domain;

namespace Vetrina.Server.Persistence
{
    public class VetrinaDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public VetrinaDbContext(DbContextOptions<VetrinaDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
