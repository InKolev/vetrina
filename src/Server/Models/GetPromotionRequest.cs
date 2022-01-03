using Microsoft.AspNetCore.Mvc;

namespace Vetrina.Server.Models
{
    public class GetPromotionRequest
    {
        [FromQuery(Name = "promotionId")]
        public int PromotionId { get; set; }
    }
}