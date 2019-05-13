using Santolibre.Map.Search.Geocoding;

namespace Santolibre.Map.Search.WebService.ApiControllers.v1.Models
{
    public class Location
    {
        public string Name { get; set; }
        public GeoCoordinates GeoCoordinates { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
    }
}
