using Santolibre.Map.Search.Lib.Models;

namespace Santolibre.Map.Search.Lib.Services
{
    public interface ISearchService
    {
        void ImportPointsOfInterest(string filename);
        void RemoveOldPointsOfInterest(int days);
        void CompactPointsOfInterest();
        SearchResult Search(string searchTerm, double? latitude, double? longitude, double searchRadius);
    }
}
