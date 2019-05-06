using Newtonsoft.Json;
using System;

namespace Santolibre.Map.Search.Lib.Models
{
    public class GeoLocation
    {
        [JsonProperty("lat")]
        public double Latitude { get; set; }

        [JsonProperty("lng")]
        public double Longitude { get; set; }

        public float GetDistanceToPoint(GeoLocation location)
        {
            var R = 6371;
            var dLat = (location.Latitude - Latitude).ToRad();
            var dLon = (location.Longitude - Longitude).ToRad();
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(Latitude.ToRad()) * Math.Cos(location.Latitude.ToRad()) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;
            return (float)d;
        }
    }
}
