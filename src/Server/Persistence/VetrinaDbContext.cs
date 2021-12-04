using Microsoft.EntityFrameworkCore;
using Vetrina.Server.Domain;
using Vetrina.Shared;

namespace Vetrina.Server.Persistence
{
    public class VetrinaDbContext : DbContext
    {
        public VetrinaDbContext(DbContextOptions<VetrinaDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Promotion> Promotions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
