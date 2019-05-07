using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Santolibre.Map.Search.Lib.Models
{
    public class PointOfInterest
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("loc")]
        public GeoLocation Location { get; set; }

        [JsonProperty("cat")]
        public string Category { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tags")]
        public Dictionary<string, string> Tags { get; set; }

        [JsonProperty("tkvs")]
        public Dictionary<string, List<string>> TagKeyValueSearch { get; set; } = new Dictionary<string, List<string>>() { { "en", new List<string>() }, { "de", new List<string>() } };

        [JsonProperty("du")]
        public DateTime DateUpdated { get; set; }
    }
}
