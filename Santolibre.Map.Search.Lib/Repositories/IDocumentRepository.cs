using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Session;

namespace Santolibre.Map.Search.Lib.Repositories
{
    public interface IDocumentRepository
    {
        IDocumentSession OpenDocumentSession();
        void RunDeleteByQueryOperation(DeleteByQueryOperation deleteByQueryOperation);
        T RunOperation<T>(IMaintenanceOperation<T> maintenanceOperation);
        void CompactDocumentStore();
    }
}
