using Raven.Client;

namespace Santolibre.Map.Search.Lib.Services
{
    public interface IDocumentService
    {
        IDocumentSession OpenDocumentSession();
        void CompactDocumentStore();
    }
}
