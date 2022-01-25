using System;
using System.Threading;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vetrina.Server.Mediatr.Commands;
using Vetrina.Server.Options;
using Vetrina.Shared;

namespace Vetrina.Server.Jobs
{
    public class ScrapeKauflandPromotionsJob : IScrapeKauflandPromotionsJob
    {
        private readonly IMediator mediator;
        private readonly ILogger<ScrapeKauflandPromotionsJob> logger;
        private readonly FeatureFlagsOptions featureFlags;

        public ScrapeKauflandPromotionsJob(
            IMediator mediator,
            ILogger<ScrapeKauflandPromotionsJob> logger,
            IOptionsSnapshot<FeatureFlagsOptions> featureFlagsOptionsSnapshot)
        {
            this.mediator = mediator;
            this.logger = logger;
            this.featureFlags = featureFlagsOptionsSnapshot.Value;
        }

        public void Execute(CancellationToken cancellationToken)
        {
            if (!this.featureFlags.EnableScrapingJobs)
            {
                logger.LogInformation($"{nameof(ScrapeKauflandPromotionsJob)} disabled by feature flag. Skipping further actions.");

                return;
            }

            logger.LogInformation($"Beginning execution of {nameof(ScrapeKauflandPromotionsJob)}...");

            try
            {
                var command = new ScrapeKauflandPromotionsCommand
                {
                    PromotionWeek = PromotionWeek.ThisWeek,
                    PromotionsPageUrl = "https://www.kaufland.bg/aktualni-predlozheniya/ot-ponedelnik/obsht-pregled.html",
                    CorrelationId = Guid.NewGuid()
                };

                var result = mediator.Send(command, cancellationToken).Result;

                logger.LogInformation(
                    result.IsSuccess
                        ? $"Successfully finished execution of {nameof(ScrapeKauflandPromotionsJob)}."
                        : $"Failed execution of {nameof(ScrapeKauflandPromotionsJob)}. Reason: {result.Message}.");
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    $"Failed to scrape Kaufland promotions. Reason: {exception.Message}");
            }
        }
    }
}
