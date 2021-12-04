using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Extensions;
using Vetrina.Shared;
using Vetrina.Shared.SearchModels;

namespace Vetrina.Server.Services
{
    public class InMemoryLuceneIndex : ILuceneIndex
    {
        private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        private const int DefaultMaxReturnedDocumentsCount = 24;

        private readonly Analyzer analyzer;
        private readonly RAMDirectory index;
        private readonly IndexWriter indexWriter;
        private readonly IndexWriterConfig indexWriterConfiguration;

        public InMemoryLuceneIndex()
        {
            analyzer = new SimpleAnalyzer(AppLuceneVersion);
            index = new RAMDirectory();
            indexWriterConfiguration = new IndexWriterConfig(AppLuceneVersion, analyzer);
            indexWriter = new IndexWriter(index, indexWriterConfiguration);
        }

        public Task IndexDocumentAsync(Document document)
        {
            indexWriter.AddDocument(document);
            indexWriter.Flush(triggerMerge: false, applyAllDeletes: false);

            return Task.CompletedTask;
        }

        public Task IndexDocumentsAsync(IEnumerable<Document> documents)
        {
            indexWriter.AddDocuments(documents);
            indexWriter.Flush(triggerMerge: false, applyAllDeletes: false);
            indexWriter.ForceMerge(1);
            indexWriter.Commit();

            return Task.CompletedTask;
        }

        public Task<LuceneSearchResponse<Promotion>> SearchAsync(
            Query query,
            Sort sortBy = null,
            Filter filter = null,
            int? maxNumberOfDocuments = null)
        {
            // Set default sortBy if not explicitly specified.
            sortBy ??= Sort.RELEVANCE;

            // Re-using the index writer to get real-time updates
            using var indexReader = indexWriter.GetReader(applyAllDeletes: true);

            var indexSearcher = new IndexSearcher(indexReader);

            var topFieldDocs =
                indexSearcher.Search(
                    query,
                    filter,
                    maxNumberOfDocuments.GetValueOrDefault(DefaultMaxReturnedDocumentsCount),
                    sortBy,
                    doDocScores: true,
                    doMaxScore: true);

            var searchResponse = new LuceneSearchResponse<Promotion>
            {
                TotalHits = topFieldDocs.TotalHits,
                Documents = topFieldDocs.ScoreDocs.Select(x => x.MapToSearchDocumentHit(indexSearcher)).ToList(),
                MaxScore = topFieldDocs.ScoreDocs.Any() ? topFieldDocs.MaxScore : 0f
            };

            return Task.FromResult(searchResponse);
        }

        public Task DeleteAllDocuments()
        {
            indexWriter.DeleteAll();
            indexWriter.Commit();

            return Task.CompletedTask;
        }

        public Task DeleteAllDocumentsWhere(Query query)
        {
            indexWriter.DeleteDocuments(query);
            indexWriter.Commit();

            return Task.CompletedTask;
        }

        public Task DeleteAllDocumentsWhere(Term term)
        {
            indexWriter.DeleteDocuments(term);
            indexWriter.Commit();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                indexWriter?.Dispose();
                index?.Dispose();
                analyzer?.Dispose();
            }
        }

        ~InMemoryLuceneIndex()
        {
            Dispose(false);
        }
    }
}