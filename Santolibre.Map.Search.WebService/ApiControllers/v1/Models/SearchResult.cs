using Santolibre.Map.Search.Geocoding;
using System.Collections.Generic;

namespace Santolibre.Map.Search.WebService.ApiControllers.v1.Models
{
    public class SearchResult
    {
        public double Radius { get; set; }
        public GeoCoordinates Center { get; set; }
        public List<Location> Locations { get; set; }
    }
}
