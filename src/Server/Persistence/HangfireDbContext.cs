using Microsoft.EntityFrameworkCore;

namespace Vetrina.Server.Persistence
{
    public class HangfireDbContext : DbContext
    {
        public HangfireDbContext(DbContextOptions<HangfireDbContext> options)
            : base(options)
        {
        }
    }
}