using System;
using MediatR;
using Vetrina.Server.Mediatr.Shared;
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
