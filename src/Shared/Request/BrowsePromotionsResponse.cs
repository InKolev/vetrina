using System.Collections.Generic;

namespace Vetrina.Shared.Request
{
    public class BrowsePromotionsResponse
    {
        public List<Promotion> Promotions { get; set; }

        public int TotalPromotionsCount { get; set; }
    }
}