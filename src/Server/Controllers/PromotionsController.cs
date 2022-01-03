using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Index;
using Lucene.Net.Queries;
using Lucene.Net.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Controllers.Abstract;
using Vetrina.Server.Models;
using Vetrina.Server.Persistence;
using Vetrina.Shared;
using Vetrina.Shared.SearchModels;

namespace Vetrina.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionsController : ApiControllerBase
    {
        private readonly VetrinaDbContext vetrinaDbContext;
        private readonly ILuceneIndex luceneIndex;
        private readonly ITransliterationService transliterationService;

        public PromotionsController(
            VetrinaDbContext vetrinaDbContext,
            ILuceneIndex luceneIndex,
            ITransliterationService transliterationService)
        {
            this.vetrinaDbContext = vetrinaDbContext;
            this.luceneIndex = luceneIndex;
            this.transliterationService = transliterationService;
        }

        [HttpPost("search")]
        public async Task<ActionResult<LuceneSearchResponse<Promotion>>> SearchPromotionsAsync(
            SearchPromotionsRequest searchPromotionsRequest)
        {
            var stopwatch = Stopwatch.StartNew();

            if (string.IsNullOrWhiteSpace(searchPromotionsRequest.SearchTerm))
            {
                return BadRequest("SearchTerm cannot be null or whitespace.");
            }

            var searchTermInCyrillic =
                await transliterationService.LatinToCyrillicAsync(
                    searchPromotionsRequest.SearchTerm.ToLowerInvariant(),
                    LanguageHint.Bulgarian);

            // Search by prefix query. 
            // TODO: Experiment with n-gram or fuzzy-match queries and analyze performance/accuracy.
            var luceneQuery =
                new PrefixQuery(
                    new Term(
                        nameof(Promotion.DescriptionSearch),
                        searchTermInCyrillic));

            var luceneFilter = new BooleanFilter();

            // If none of the store flags is enabled, search in all stores by default.
            if (!searchPromotionsRequest.IncludeKaufland && !searchPromotionsRequest.IncludeLidl)
            {
                luceneFilter.Add(CreateStoreTermFilter(Store.Kaufland), Occur.SHOULD);
                luceneFilter.Add(CreateStoreTermFilter(Store.Lidl), Occur.SHOULD);
            }
            else
            {
                if (searchPromotionsRequest.IncludeLidl)
                {
                    luceneFilter.Add(CreateStoreTermFilter(Store.Lidl), Occur.SHOULD);
                }

                if (searchPromotionsRequest.IncludeKaufland)
                {
                    luceneFilter.Add(CreateStoreTermFilter(Store.Kaufland), Occur.SHOULD);
                }
            }

            var luceneSearchResponse =
                await luceneIndex.SearchAsync(
                    query: luceneQuery,
                    sortBy: Sort.RELEVANCE,
                    filter: luceneFilter,
                    maxNumberOfDocuments: searchPromotionsRequest.MaxNumberOfDocs);

            stopwatch.Stop();
            luceneSearchResponse.ProcessingTimeMs = stopwatch.Elapsed.TotalMilliseconds;

            return Ok(luceneSearchResponse);
        }

        [HttpPost("browse")]
        public async Task<ActionResult<BrowsePromotionsResponse>> BrowsePromotionsAsync(
            BrowsePromotionsRequest browsePromotionsRequest)
        {
            //var skip = (browsePromotionsRequest.Page - 1) * browsePromotionsRequest.PageSize;
            //var take = browsePromotionsRequest.PageSize;

            var skip = browsePromotionsRequest.Skip;
            var take = browsePromotionsRequest.Take;

            var promotions =
                await this.vetrinaDbContext.Promotions
                    .OrderBy(x => x.Id)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();

            return new BrowsePromotionsResponse()
            {
                Promotions = promotions,
                TotalPromotionsCount = await this.vetrinaDbContext.Promotions.CountAsync()
            };
        }

        [HttpGet("get/{promotionId}")]
        public async Task<ActionResult<GetPromotionResponse>> GetPromotionAsync(int promotionId)
        {
            var promotion = await this.vetrinaDbContext.FindAsync<Promotion>(promotionId);

            if (promotion == null)
            {
                return new GetPromotionResponse(GetPromotionResponseType.NotFound);
            }

            return new GetPromotionResponse(GetPromotionResponseType.Successful, promotion);
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
