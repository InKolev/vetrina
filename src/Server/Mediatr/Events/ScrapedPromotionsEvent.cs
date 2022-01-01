using System;
using System.Collections.Generic;
using MediatR;
using Vetrina.Shared;

namespace Vetrina.Server.Mediatr.Events
{
    public class ScrapedPromotionsEvent : INotification
    {
        public Store Store { get; set; }

        public PromotionWeek PromotionWeek { get; set; }

        public List<Promotion> Promotions { get; set; }

        public DateTime EventDate { get; set; }

        public Guid EventId { get; set; }

        public Guid CorrelationId { get; set; }
    }
}
