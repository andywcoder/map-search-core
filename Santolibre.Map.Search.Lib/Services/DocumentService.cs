using Microsoft.Extensions.Configuration;
using Raven.Client;
using Raven.Client.Document;
using Santolibre.Map.Search.Lib.Models;

namespace Santolibre.Map.Search.Lib.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentStore _documentStore;

        public DocumentService(IConfiguration configuration)
        {
            _documentStore = new DocumentStore { Url = configuration.GetValue<string>("RavenDbUrl"), DefaultDatabase = configuration.GetValue<string>("RavenDbDefaultDatabase"), ApiKey = configuration.GetValue<string>("RavenDbApiKey") };
            _documentStore.Initialize();
            new PointOfInterest_ByTagsAndCoordinates().Execute(_documentStore);
            new PointOfInterest_ByDateUpdated().Execute(_documentStore);
        }

        public IDocumentSession OpenDocumentSession()
        {
            return _documentStore.OpenSession();
        }

        public void CompactDocumentStore()
        {
            var operation = _documentStore.DatabaseCommands.GlobalAdmin.CompactDatabase("Santolibre.Map");
            operation.WaitForCompletion();
        }
    }
}
