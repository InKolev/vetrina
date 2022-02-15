using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Lucene.Net.Index;
using Lucene.Net.Queries;
using Lucene.Net.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vetrina.Autogen.API.Client.Contracts;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Controllers.Abstract;
using Vetrina.Server.Domain;
using Vetrina.Server.Persistence;
using Vetrina.Shared;
using Vetrina.Shared.SearchModels;
using BrowsePromotionsRequest = Vetrina.Server.Models.BrowsePromotionsRequest;
using BrowsePromotionsResponse = Vetrina.Server.Models.BrowsePromotionsResponse;
using GetPromotionResponse = Vetrina.Server.Models.GetPromotionResponse;
using GetPromotionResponseType = Vetrina.Server.Models.GetPromotionResponseType;
using Promotion = Vetrina.Shared.Promotion;
using SearchPromotionsRequest = Vetrina.Server.Models.SearchPromotionsRequest;
using Store = Vetrina.Shared.Store;

namespace Vetrina.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionsController : ApiControllerBase
    {
        private readonly VetrinaDbContext vetrinaDbContext;
        private readonly ILuceneIndex luceneIndex;
        private readonly ITransliterationService transliterationService;
        private readonly IPromotionsClient promotionsClient;
        private readonly IHttpClientFactory httpClientFactory;

        public PromotionsController(
            VetrinaDbContext vetrinaDbContext,
            ILuceneIndex luceneIndex,
            ITransliterationService transliterationService,
            IPromotionsClient promotionsClient)
        {
            this.vetrinaDbContext = vetrinaDbContext;
            this.luceneIndex = luceneIndex;
            this.transliterationService = transliterationService;
            this.promotionsClient = promotionsClient;
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

            // If the search term is already in cyrillic, no transformations will be applied.
            // The transliteration service will leave the string untouched.
            var searchTermInCyrillic =
                await transliterationService.LatinToCyrillicAsync(
                    searchPromotionsRequest.SearchTerm.ToLowerInvariant(),
                    LanguageHint.Bulgarian);

#pragma warning disable CS4014
            // These calls are intentionally not awaited.
            // We don't care about losing a couple of records when tracking user activity.
            this.vetrinaDbContext.SearchRecords.AddAsync(new SearchRecord(searchTermInCyrillic, GetCallerIpAddress()));
            this.vetrinaDbContext.SaveChangesAsync();
#pragma warning restore CS4014

            // TODO: Experiment with n-gram or fuzzy-match queries and analyze performance/accuracy.
            var luceneQuery =
                new PrefixQuery(
                    new Term(
                        nameof(Promotion.DescriptionSearch),
                        searchTermInCyrillic));

            var luceneFilter = new BooleanFilter();

            // If none of the store flags is enabled, search in all stores by default.
            if (!searchPromotionsRequest.IncludeKaufland 
                && !searchPromotionsRequest.IncludeLidl 
                && !searchPromotionsRequest.IncludeBilla)
            {
                luceneFilter.Add(CreateStoreTermFilter(Store.Kaufland), Occur.SHOULD);
                luceneFilter.Add(CreateStoreTermFilter(Store.Lidl), Occur.SHOULD);
                luceneFilter.Add(CreateStoreTermFilter(Store.Billa), Occur.SHOULD);
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

                if (searchPromotionsRequest.IncludeBilla)
                {
                    luceneFilter.Add(CreateStoreTermFilter(Store.Billa), Occur.SHOULD);
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

        //[HttpPost("send-to-external-server")]
        //public async Task<ActionResult<int>> SendPromotionsToExternalServer(SendPromotionsToExternalServerRequest request)
        //{
        //    var promotions = await this.vetrinaDbContext.Promotions.Where(x => x.Store == request.Store).ToListAsync();

        //    var seededPromotionsCount = await this.promotionsClient.SeedPromotionsAsync(
        //        promotions.Select(x => new Autogen.API.Client.Contracts.Promotion
        //        {
        //            DescriptionRaw = x.DescriptionRaw,
        //            DescriptionSearch = x.DescriptionSearch,
        //            DiscountPercentage = x.DiscountPercentage,
        //            Id = x.Id,
        //            Store = (Autogen.API.Client.Contracts.Store)x.Store,
        //            ImageUrl = x.ImageUrl,
        //            OfficialPrice = x.OfficialPrice,
        //            Price = x.Price,
        //            PriceRaw = x.PriceRaw,
        //            PromotionEndingAt = x.PromotionEndingAt,
        //            PromotionStartingFrom = x.PromotionStartingFrom
        //        }).ToList());

        //    return this.Ok(seededPromotionsCount);
        //}

        [HttpPost("seed")]
        public async Task<ActionResult<int>> SeedPromotions(List<Promotion> promotions)
        {
            await this.vetrinaDbContext.AddRangeAsync(promotions);

            var affectedRecords = await this.vetrinaDbContext.SaveChangesAsync();

            return this.Ok(affectedRecords);
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

    public class SendPromotionsToExternalServerRequest
    {
        public string ExternalServerUrl { get; set; }

        public Store Store { get; set; }
    }
}
