using Newtonsoft.Json;

namespace Santolibre.Map.Search.Geocoding.OpenCage
{
    public class OpenCageSearchResultComponents
    {
        [JsonProperty("_type")]
        public string Type { get; set; }

        public string City { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        public string County { get; set; }

        public string Postcode { get; set; }

        public string Road { get; set; }

        public string Town { get; set; }

        [JsonProperty("house_number")]
        public string HouseNumber { get; set; }

        public string Residential { get; set; }
    }
}
