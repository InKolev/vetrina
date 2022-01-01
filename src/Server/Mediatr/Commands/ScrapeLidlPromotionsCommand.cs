using System;
using MediatR;
using Vetrina.Shared;

namespace Vetrina.Server.Mediatr.Commands
{
    public class ScrapeLidlPromotionsCommand : IRequest<CommandResult>
    {
        public string PromotionsPageUrl { get; set; }

        public PromotionWeek PromotionWeek { get; set; }

        public Guid CorrelationId { get; set; }
    }
}
