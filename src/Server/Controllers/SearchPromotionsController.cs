using System.Diagnostics;
using System.Threading.Tasks;
using Lucene.Net.Index;
using Lucene.Net.Queries;
using Lucene.Net.Search;
using Microsoft.AspNetCore.Mvc;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Controllers.Abstract;
using Vetrina.Shared;
using Vetrina.Shared.Request;

namespace Vetrina.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchPromotionsController : ApiControllerBase
    {
        private readonly ILuceneIndex luceneIndex;
        private readonly ITransliterationService transliterationService;

        public SearchPromotionsController(
            ILuceneIndex luceneIndex,
            ITransliterationService transliterationService)
        {
            this.luceneIndex = luceneIndex;
            this.transliterationService = transliterationService;
        }

        [HttpPost]
        public async Task<IActionResult> SearchPromotions(SearchPromotionsRequest request)
        {
            var stopwatch = Stopwatch.StartNew();

            if (string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                return BadRequest("SearchTerm cannot be null or whitespace.");
            }

            var searchTermInCyrillic =
                await transliterationService.LatinToCyrillicAsync(
                    request.SearchTerm.ToLowerInvariant(),
                    LanguageHint.Bulgarian);

            // Search by prefix query.
            var query =
                new PrefixQuery(
                    new Term(
                        nameof(Promotion.DescriptionSearch),
                        searchTermInCyrillic));

            var filter = new BooleanFilter();

            // If none of the store flags is enabled, search in all stores by default.
            if (!request.IncludeKaufland && !request.IncludeLidl)
            {
                filter.Add(CreateStoreTermFilter(Store.Kaufland), Occur.SHOULD);
                filter.Add(CreateStoreTermFilter(Store.Lidl), Occur.SHOULD);
            }
            else
            {
                if (request.IncludeLidl)
                {
                    filter.Add(CreateStoreTermFilter(Store.Lidl), Occur.SHOULD);
                }

                if (request.IncludeKaufland)
                {
                    filter.Add(CreateStoreTermFilter(Store.Kaufland), Occur.SHOULD);
                }
            }

            var luceneSearchResponse =
                await luceneIndex.SearchAsync(
                    query: query,
                    sortBy: Sort.RELEVANCE,
                    filter: filter,
                    maxNumberOfDocuments: request.MaxNumberOfDocs);

            stopwatch.Stop();
            luceneSearchResponse.ProcessingTimeMs = stopwatch.Elapsed.TotalMilliseconds;

            return Ok(luceneSearchResponse);
        }

        private static Filter CreateStoreTermFilter(Store store)
        {
            var filter =
                NumericRangeFilter.NewInt32Range(
                    field: nameof(Promotion.Store),
                    precisionStep: 1,
                    min: (int)store,
                    max: (int)store,
                    minInclusive: true,
                    maxInclusive: true);

            return filter;
        }
    }
}
