using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Operations.Indexes;
using Raven.Client.Documents.Session;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using Santolibre.Map.Search.Lib.Models;

namespace Santolibre.Map.Search.Lib.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IConfiguration _configuration;
        private readonly IDocumentStore _documentStore;

        public DocumentService(IConfiguration configuration)
        {
            _configuration = configuration;
            _documentStore = new DocumentStore { Urls = new string[] { configuration.GetValue<string>("AppSettings:RavenDbUrl") }, Database = configuration.GetValue<string>("AppSettings:RavenDbDefaultDatabase") };
            _documentStore.Initialize();
            new PointOfInterest_ByTagsEnglishNameAndCoordinates().Execute(_documentStore);
            new PointOfInterest_ByTagsGermanNameAndCoordinates().Execute(_documentStore);
            new PointOfInterest_ByDateUpdated().Execute(_documentStore);
        }

        public IDocumentSession OpenDocumentSession()
        {
            return _documentStore.OpenSession();
        }

        public void RunDeleteByQueryOperation(DeleteByQueryOperation deleteByQueryOperation)
        {
            var operation = _documentStore.Operations.Send(deleteByQueryOperation);
            operation.WaitForCompletion();
        }

        public T RunOperation<T>(IMaintenanceOperation<T> maintenanceOperation)
        {
            var result = _documentStore.Maintenance.Send(maintenanceOperation);
            return result;
        }

        public void CompactDocumentStore()
        {
            var indexNames = _documentStore.Maintenance.Send(new GetIndexNamesOperation(0, int.MaxValue));

            var settings = new CompactSettings
            {
                DatabaseName = _configuration.GetValue<string>("AppSettings:RavenDbDefaultDatabase"),
                Documents = true,
                Indexes = indexNames
            };
            var operation = _documentStore.Maintenance.Server.Send(new CompactDatabaseOperation(settings));
            operation.WaitForCompletion();
        }
    }
}
