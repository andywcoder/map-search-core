using Newtonsoft.Json;

namespace Santolibre.Map.Search.Lib.Models
{
    public class GeoLocation
    {
        [JsonProperty("lat")]
        public double Latitude { get; set; }

        [JsonProperty("lng")]
        public double Longitude { get; set; }
    }
}
