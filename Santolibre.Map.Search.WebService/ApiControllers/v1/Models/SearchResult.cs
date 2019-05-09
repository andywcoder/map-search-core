using Santolibre.Map.Search.Lib.Models;
using System.Collections.Generic;

namespace Santolibre.Map.Search.WebService.ApiControllers.v1.Models
{
    public class SearchResult
    {
        public string Name { get; set; }
        public GeoLocation Location { get; set; }
        public GeocodeQuality GeocodeQuality { get; set; }
        public double Radius { get; set; }
        public List<PointOfInterest> PointsOfInterest { get; set; }
    }
}
