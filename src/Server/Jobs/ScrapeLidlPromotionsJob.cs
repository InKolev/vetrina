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
    public class ScrapeLidlPromotionsJob : IScrapeLidlPromotionsJob
    {
        private readonly IMediator mediator;
        private readonly ILogger<ScrapeLidlPromotionsJob> logger;
        private readonly FeatureFlagsOptions featureFlags;

        public ScrapeLidlPromotionsJob(
            IMediator mediator,
            ILogger<ScrapeLidlPromotionsJob> logger,
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
                logger.LogInformation($"{nameof(ScrapeLidlPromotionsJob)} disabled by feature flag. Skipping further actions.");

                return;
            }

            logger.LogInformation($"Beginning execution of {nameof(ScrapeLidlPromotionsJob)}...");

            try
            {
                var command = new ScrapeLidlPromotionsCommand
                {
                    PromotionWeek = PromotionWeek.ThisWeek,
                    PromotionsPageUrl = "https://www.lidl.bg/",
                    CorrelationId = Guid.NewGuid()
                };

                var result = mediator.Send(command, cancellationToken).Result;

                logger.LogInformation(
                    result.IsSuccess
                        ? $"Successfully finished execution of {nameof(ScrapeLidlPromotionsJob)}."
                        : $"Failed execution of {nameof(ScrapeLidlPromotionsJob)}. Reason: {result.Message}.");
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    $"Failed to scrape Lidl promotions. Reason: {exception.Message}");
            }
        }
    }
}
