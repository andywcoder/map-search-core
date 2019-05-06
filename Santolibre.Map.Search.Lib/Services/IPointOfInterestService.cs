using Santolibre.Map.Search.Lib.Models;
using System.Collections.Generic;

namespace Santolibre.Map.Search.Lib.Services
{
    public interface IPointOfInterestService
    {
        void ImportPointsOfInterest(string filename);
        void RemoveOldPointsOfInterest(int days);
        void CompactPointsOfInterest();
        SearchResult GetSearchResult(string searchTerm, double? latitude, double? longitude);
        List<Suggestion> GetSuggestions(string searchTerm, double? latitude, double? longitude);
    }
}
