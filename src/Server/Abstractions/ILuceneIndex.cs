using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Vetrina.Shared;
using Vetrina.Shared.SearchModels;

namespace Vetrina.Server.Abstractions
{
    public interface ILuceneIndex : IDisposable
    {
        Task IndexDocumentAsync(Document document);

        Task IndexDocumentsAsync(IEnumerable<Document> documents);

        Task DeleteAllDocuments();

        Task DeleteAllDocumentsWhere(Query query);

        Task DeleteAllDocumentsWhere(Term term);

        Task<LuceneSearchResponse<Promotion>> SearchAsync(
            Query query,
            Sort sortBy = null,
            Filter filter = null,
            int? maxNumberOfDocuments = null);
    }
}