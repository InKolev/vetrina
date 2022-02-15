using System;
using System.Threading;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vetrina.Server.Mediatr.Commands;
using Vetrina.Server.Options;
using Vetrina.Shared;

namespace Vetrina.Server.Jobs;

public class ScrapeBillaPromotionsJob : IScrapeBillaPromotionsJob
{
    private readonly IMediator mediator;
    private readonly ILogger<ScrapeBillaPromotionsJob> logger;
    private readonly FeatureFlagsOptions featureFlags;

    public ScrapeBillaPromotionsJob(
        IMediator mediator,
        ILogger<ScrapeBillaPromotionsJob> logger,
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
            logger.LogInformation($"{nameof(ScrapeBillaPromotionsJob)} disabled by feature flag. Skipping further actions.");

            return;
        }

        logger.LogInformation($"Beginning execution of {nameof(ScrapeBillaPromotionsJob)}...");

        try
        {
            var command = new ScrapeBillaPromotionsCommand()
            {
                PromotionWeek = PromotionWeek.ThisWeek,
                PromotionsPageUrl = "https://ssbbilla.site/weekly",
                CorrelationId = Guid.NewGuid()
            };

            var result = mediator.Send(command, cancellationToken).Result;

            logger.LogInformation(
                result.IsSuccess
                    ? $"Successfully finished execution of {nameof(ScrapeBillaPromotionsJob)}."
                    : $"Failed execution of {nameof(ScrapeBillaPromotionsJob)}. Reason: {result.Message}.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                $"Failed to scrape Billa promotions. Reason: {exception.Message}");
        }
    }
}