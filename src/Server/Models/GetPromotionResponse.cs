using Vetrina.Shared;

namespace Vetrina.Server.Models
{
    public class GetPromotionResponse
    {
        public GetPromotionResponse(
            GetPromotionResponseType type,
            Promotion promotion = default,
            string details = default)
        {
            Type = type;
            Promotion = promotion;
            Details = details;
        }

        public string Details { get; set; }

        public GetPromotionResponseType Type { get; set; }

        public Promotion Promotion { get; set; }
    }
}