using System.Collections.Generic;

namespace Vetrina.Shared.SearchModels
{
    public class LuceneSearchResponse<T> where T : Promotion
    {
        public int TotalHits { get; set; }

        public List<SearchDocumentHit<T>> Documents { get; set; } = new List<SearchDocumentHit<T>>();

        public float MaxScore { get; set; }

        public double ProcessingTimeMs { get; set; }
    }
}
