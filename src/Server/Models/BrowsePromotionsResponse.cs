using System.Collections.Generic;
using Vetrina.Shared;

namespace Vetrina.Server.Models
{
    public class BrowsePromotionsResponse
    {
        public List<Promotion> Promotions { get; set; }

        public int TotalPromotionsCount { get; set; }
    }
}