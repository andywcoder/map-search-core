using Newtonsoft.Json;

namespace Santolibre.Map.Search.Geocoding
{
    public class GeoCoordinates
    {
        [JsonProperty("lat")]
        public double Latitude { get; set; }

        [JsonProperty("lng")]
        public double Longitude { get; set; }
    }
}
