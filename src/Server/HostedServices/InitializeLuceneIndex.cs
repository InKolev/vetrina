using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Extensions;
using Vetrina.Server.Persistence;

namespace Vetrina.Server.HostedServices
{
    public class InitializeLuceneIndex : IHostedService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILuceneIndex luceneIndex;
        private readonly ILogger<InitializeLuceneIndex> logger;

        public InitializeLuceneIndex(
            IServiceScopeFactory serviceScopeFactory,
            ILuceneIndex luceneIndex,
            ILogger<InitializeLuceneIndex> logger)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.luceneIndex = luceneIndex;
            this.logger = logger;
        }
            
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var vetrinaDbContext = scope.ServiceProvider.GetRequiredService<VetrinaDbContext>();

                logger.LogInformation("Loading promotional items from database...");

                var allPromotions =
                    await vetrinaDbContext.Promotions
                        .ToListAsync(cancellationToken);

                if (allPromotions.Any())
                {
                    logger.LogInformation($"Indexing {allPromotions.Count} promotional items to Lucene...");

                    await luceneIndex.DeleteAllDocuments();

                    await luceneIndex.IndexDocumentsAsync(
                        allPromotions.Select(promotion =>
                            promotion.MapToLuceneDocument()));

                    logger.LogInformation($"Successfully indexed {allPromotions.Count} promotional items.");
                }
                else
                {
                    logger.LogInformation("No promotions found in database. Skipped indexing step.");
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Failure while attempting to initialize Lucene index from database. Reason: {e.Message}");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
