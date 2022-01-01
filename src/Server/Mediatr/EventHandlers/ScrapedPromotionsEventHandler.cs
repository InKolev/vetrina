using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lucene.Net.Search;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Extensions;
using Vetrina.Server.Mediatr.Events;
using Vetrina.Server.Persistence;
using Vetrina.Shared;

namespace Vetrina.Server.Mediatr.EventHandlers
{
    public class ScrapedPromotionsEventHandler : INotificationHandler<ScrapedPromotionsEvent>
    {
        private readonly VetrinaDbContext vetrinaDbContext;
        private readonly ILuceneIndex luceneIndex;
        private readonly ILogger<ScrapedPromotionsEventHandler> logger;

        public ScrapedPromotionsEventHandler(
            VetrinaDbContext vetrinaDbContext,
            ILuceneIndex luceneIndex, 
            ILogger<ScrapedPromotionsEventHandler> logger)
        {
            this.vetrinaDbContext = vetrinaDbContext;
            this.luceneIndex = luceneIndex;
            this.logger = logger;
        }

        public async Task Handle(
            ScrapedPromotionsEvent notification,
            CancellationToken cancellationToken)
        {
            logger.LogInformation($"Received {notification.Promotions.Count} promotional items from {notification.Store} for {notification.PromotionWeek}");

            // Archive instead of delete.
            var deletedRows = await vetrinaDbContext.Database
                .ExecuteSqlInterpolatedAsync(
                    sql: $"DELETE FROM Promotions WHERE Store = {(int)notification.Store}",
                    cancellationToken: cancellationToken);

            logger.LogInformation($"Deleted all ({deletedRows}) documents from the Database where Store is {notification.Store}.");

            await vetrinaDbContext.AddRangeAsync(notification.Promotions, cancellationToken);

            var addedRows = await vetrinaDbContext.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation($"Persisted {addedRows} new items to the Database for Store {notification.Store}.");
            
            await luceneIndex.DeleteAllDocumentsWhere(
                new BooleanQuery
                {
                    {CreateStoreQuery(notification.Store), Occur.MUST}
                });

            logger.LogInformation($"Deleted all documents from Lucene index where Store is {notification.Store}.");

            await luceneIndex.IndexDocumentsAsync(
                notification.Promotions.Select(promotionalItem => 
                    promotionalItem.MapToLuceneDocument()));

            logger.LogInformation($"Indexed {notification.Promotions.Count} new documents in Lucene for Store {notification.Store}.");
        }

        private static Query CreateStoreQuery(Store store)
        {
            var query =
                NumericRangeQuery.NewInt32Range(
                    field: nameof(Promotion.Store),
                    precisionStep: 1,
                    min: (int)store,
                    max: (int)store,
                    minInclusive: true,
                    maxInclusive: true);

            return query;
        }
    }
}
