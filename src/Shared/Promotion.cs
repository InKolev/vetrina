using System;

namespace Vetrina.Shared
{
    public class Promotion
    {
        public int Id { get; set; }

        public DateTime PromotionStartingFrom { get; set; }

        public DateTime PromotionEndingAt { get; set; }

        public double Price { get; set; }

        public string PriceRaw { get; set; }

        public double OfficialPrice { get; set; }

        public string DiscountPercentage { get; set; }

        public string DescriptionRaw { get; set; }

        public string DescriptionSearch { get; set; }

        public string ImageUrl { get; set; }

        public Store Store { get; set; }
    }
}