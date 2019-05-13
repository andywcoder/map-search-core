using System;
using System.Collections.Generic;

namespace Santolibre.Map.Search.Lib.Models
{
    public class PointOfInterest : Location
    {
        public string Id { get; set; }
        public Dictionary<string, string> Tags { get; set; }
        public Dictionary<string, List<string>> TagKeyValueSearch { get; set; } = new Dictionary<string, List<string>>() { { "en", new List<string>() }, { "de", new List<string>() } };
        public DateTime DateUpdated { get; set; }
    }
}
