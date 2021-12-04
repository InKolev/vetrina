using System;
using Lucene.Net.Documents;
using Lucene.Net.Documents.Extensions;
using Lucene.Net.Search;
using Vetrina.Shared;
using Vetrina.Shared.SearchModels;

namespace Vetrina.Server.Extensions
{
    public static class LuceneMappingExtensions
    {
        public static Promotion MapToPromotionalItem(this Document luceneDocument)
        {
            return new Promotion
            {
                Id = luceneDocument
                    .GetField(nameof(Promotion.Id))
                    .GetInt32ValueOrDefault(),

                DiscountPercentage = luceneDocument
                    .GetField(nameof(Promotion.DiscountPercentage))
                    .GetStringValue(),

                OfficialPrice = luceneDocument
                    .GetField(nameof(Promotion.OfficialPrice))
                    .GetStringValue(),

                Store = (Store)luceneDocument
                    .GetField(nameof(Promotion.Store))
                    .GetInt64ValueOrDefault(),

                DescriptionRaw = luceneDocument
                    .GetField(nameof(Promotion.DescriptionRaw))
                    .GetStringValue(),

                DescriptionSearch = luceneDocument
                    .GetField(nameof(Promotion.DescriptionSearch))
                    .GetStringValue(),

                ImageUrl = luceneDocument
                    .GetField(nameof(Promotion.ImageUrl))
                    .GetStringValue(),

                Price = luceneDocument
                    .GetField(nameof(Promotion.Price))
                    .GetStringValue(),

                PromotionStartingFrom = new DateTime(
                    luceneDocument
                        .GetField(nameof(Promotion.PromotionStartingFrom))
                        .GetInt64ValueOrDefault(), DateTimeKind.Unspecified),

                PromotionEndingAt = new DateTime(
                    luceneDocument
                        .GetField(nameof(Promotion.PromotionEndingAt))
                        .GetInt64ValueOrDefault(), DateTimeKind.Unspecified)
            };
        }

        public static Document MapToLuceneDocument(this Promotion promotion)
        {
            return new Document
            {
                new Int32Field(
                    nameof(Promotion.Id),
                    (int) promotion.Id,
                    Field.Store.YES),

                new Int32Field(
                    nameof(Promotion.Store),
                    (int) promotion.Store,
                    Field.Store.YES),

                new TextField(
                    nameof(Promotion.DescriptionRaw),
                    promotion.DescriptionRaw,
                    Field.Store.YES),

                new TextField(
                    nameof(Promotion.DescriptionSearch),
                    promotion.DescriptionSearch,
                    Field.Store.YES),

                new StringField(
                    nameof(Promotion.OfficialPrice),
                    promotion.OfficialPrice ?? string.Empty,
                    Field.Store.YES),

                new StringField(
                    nameof(Promotion.Price),
                    promotion.Price ?? string.Empty,
                    Field.Store.YES),

                new StringField(
                    nameof(Promotion.DiscountPercentage),
                    promotion.DiscountPercentage ?? string.Empty,
                    Field.Store.YES),

                new StringField(
                    nameof(Promotion.ImageUrl),
                    promotion.ImageUrl ?? string.Empty,
                    Field.Store.YES),

                new Int64Field(
                    nameof(Promotion.PromotionStartingFrom),
                    promotion.PromotionStartingFrom.Ticks,
                    Field.Store.YES),

                new Int64Field(
                    nameof(Promotion.PromotionEndingAt),
                    promotion.PromotionEndingAt.Ticks,
                    Field.Store.YES)
            };
        }

        public static SearchDocumentHit<Promotion> MapToSearchDocumentHit(this ScoreDoc scoreDoc, IndexSearcher indexSearcher)
        {
            return new SearchDocumentHit<Promotion>
            {
                Document = indexSearcher.Doc(scoreDoc.Doc).MapToPromotionalItem(),
                Score = scoreDoc.Score
            };
        }
    }
}