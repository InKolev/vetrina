using System;
using System.Threading;
using MediatR;
using Microsoft.Extensions.Logging;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Mediatr.Commands;
using Vetrina.Shared;

namespace Vetrina.Server.Jobs
{
    public interface IScrapeLidlPromotionsJob : IJob
    {

    }

    public class ScrapeLidlPromotionsJob : IScrapeLidlPromotionsJob
    {
        private readonly IMediator mediator;
        private readonly ILogger<ScrapeLidlPromotionsJob> logger;

        public ScrapeLidlPromotionsJob(
            IMediator mediator,
            ILogger<ScrapeLidlPromotionsJob> logger)
        {
            this.mediator = mediator;
            this.logger = logger;
        }

        public void Execute(CancellationToken cancellationToken)
        {
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
