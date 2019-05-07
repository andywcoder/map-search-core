using Santolibre.Map.Search.Lib.Models;
using System.Collections.Generic;

namespace Santolibre.Map.Search.Lib.Services
{
    public interface ISearchService
    {
        SearchResult GetSearchResult(string searchTerm, double? latitude, double? longitude);
        List<Suggestion> GetSuggestions(string searchTerm, double? latitude, double? longitude);
    }
}
