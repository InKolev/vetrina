using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vetrina.Server.Persistence;

namespace Vetrina.Server.HostedServices
{
    public class InitializeVetrinaDatabase : IHostedService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<InitializeVetrinaDatabase> logger;

        public InitializeVetrinaDatabase(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<InitializeVetrinaDatabase> logger)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var vetrinaDbContext = scope.ServiceProvider.GetRequiredService<VetrinaDbContext>();

                logger.LogInformation("Checking for pending migrations...");

                var pendingMigrations =
                    (await vetrinaDbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

                if (pendingMigrations.Any())
                {
                    foreach (var pendingMigration in pendingMigrations)
                    {
                        logger.LogInformation($"Pending migration --> {pendingMigration}");
                    }

                    await vetrinaDbContext.Database.MigrateAsync(cancellationToken);

                    logger.LogInformation("Migrations successfully applied.");
                }
                else
                {
                    logger.LogInformation("No pending migrations.");
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Failure while attempting to execute database migrations. Reason: {e.Message}");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}