using System;
using System.Threading;
using MediatR;
using Microsoft.Extensions.Logging;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Mediatr.Commands;
using Vetrina.Shared;

namespace Vetrina.Server.Jobs
{
    public interface IScrapeKauflandPromotionsJob : IJob
    {

    }

    public class ScrapeKauflandPromotionsJob : IScrapeKauflandPromotionsJob
    {
        private readonly IMediator mediator;
        private readonly ILogger<ScrapeKauflandPromotionsJob> logger;

        public ScrapeKauflandPromotionsJob(
            IMediator mediator,
            ILogger<ScrapeKauflandPromotionsJob> logger)
        {
            this.mediator = mediator;
            this.logger = logger;
        }

        public void Execute(CancellationToken cancellationToken)
        {
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
