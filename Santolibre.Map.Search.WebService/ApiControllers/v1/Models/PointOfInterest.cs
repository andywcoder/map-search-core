using System;
using System.Collections.Generic;

namespace Santolibre.Map.Search.WebService.ApiControllers.v1.Models
{
    public class PointOfInterest : Location
    {
        public long Id { get; set; }
        public Dictionary<string, string> Tags { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
