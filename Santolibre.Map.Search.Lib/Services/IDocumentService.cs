using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Session;

namespace Santolibre.Map.Search.Lib.Services
{
    public interface IDocumentService
    {
        IDocumentSession OpenDocumentSession();
        void RunDeleteByQueryOperation<TEntity, TIndexCreator>(DeleteByQueryOperation<TEntity, TIndexCreator> deleteByQueryOperation) where TIndexCreator : AbstractIndexCreationTask, new();
        void CompactDocumentStore();
    }
}
