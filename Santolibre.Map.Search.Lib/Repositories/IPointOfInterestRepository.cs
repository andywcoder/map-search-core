using Santolibre.Map.Search.Lib.Models;
using System.Collections.Generic;

namespace Santolibre.Map.Search.Lib.Repositories
{
    public interface IPointOfInterestRepository
    {
        List<PointOfInterest> GetPointsOfInterest(string searchTerms, bool addWildcards, double latitude, double longitude, double? radius, int take);
        void SavePointsOfInterest(List<PointOfInterest> pointsOfInterest);
        int CountPointsOfInterest(string searchTerm);
    }
}
