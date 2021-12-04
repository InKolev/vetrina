using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Vetrina.Server.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Creates the database if it doesn't exist and applies all pending migrations automatically.
        /// </summary>
        /// <typeparam name="T">The database context.</typeparam>
        /// <param name="applicationBuilder">ApplicationBuilder</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Task</returns>
        public static async Task ApplyPendingMigrations<T>(
            this IApplicationBuilder applicationBuilder,
            CancellationToken cancellationToken) where T : DbContext
        {
            using var scope = applicationBuilder.ApplicationServices.CreateScope();

            var logger = scope.ServiceProvider.GetService<ILogger>();
            var dbContext = scope.ServiceProvider.GetService<T>();
            var dbContextName = typeof(T).Name;

            try
            {
                logger.LogInformation($"[{dbContextName}]: Checking for pending migrations...");

                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
                if (pendingMigrations.Any())
                {
                    foreach (var pendingMigration in pendingMigrations)
                    {
                        logger.LogInformation($"[{dbContextName}]: Pending migration {pendingMigration}");
                    }

                    await dbContext.Database.MigrateAsync(cancellationToken);

                    logger.LogInformation($"[{dbContextName}]: Migrations successfully applied.");
                }
                else
                {
                    logger.LogInformation($"[{dbContextName}]: No pending migrations.");
                }
            }
            catch (Exception e)
            {
                logger.LogError($"[{dbContextName}]: Failure while attempting to execute database migrations. Reason: {e.Message}");
                throw;
            }
        }
    }
}
