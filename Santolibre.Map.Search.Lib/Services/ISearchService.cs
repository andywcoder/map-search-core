using Santolibre.Map.Search.Lib.Models;
using System.Collections.Generic;

namespace Santolibre.Map.Search.Lib.Services
{
    public interface ISearchService
    {
        void ImportPointsOfInterest(string filename);
        void RemoveOldPointsOfInterest(int days);
        void UpdateSuggestions();
        void CompactPointsOfInterest();
        SearchResult Search(string searchTerm, double? latitude, double? longitude, double searchRadius);
        List<Suggestion> GetSuggestions(string searchTerm, double? latitude, double? longitude, double searchRadius);
    }
}
