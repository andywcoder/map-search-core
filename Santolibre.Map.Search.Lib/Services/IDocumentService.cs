using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Session;

namespace Santolibre.Map.Search.Lib.Services
{
    public interface IDocumentService
    {
        IDocumentSession OpenDocumentSession();
        void RunDeleteByQueryOperation(DeleteByQueryOperation deleteByQueryOperation);
        T RunOperation<T>(IMaintenanceOperation<T> maintenanceOperation);
        void CompactDocumentStore();
    }
}
