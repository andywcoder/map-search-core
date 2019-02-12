using Santolibre.Map.Search.Lib.Models;
using System;
using System.Collections.Generic;

namespace Santolibre.Map.Search.WebService.Controllers.v1.Models
{
    public class PointOfInterest
    {
        public long Id { get; set; }
        public GeoLocation Location { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> Tags { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
