using Newtonsoft.Json;
using System;

namespace Santolibre.Map.Search.Lib
{
    public class GeoCoordinates
    {
        [JsonProperty("lat")]
        public double Latitude { get; set; }

        [JsonProperty("lng")]
        public double Longitude { get; set; }

        public GeoCoordinates(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public float GetDistanceToPoint(GeoCoordinates geoCoordinates)
        {
            var R = 6371;
            var dLat = (geoCoordinates.Latitude - Latitude).ToRad();
            var dLon = (geoCoordinates.Longitude - Longitude).ToRad();
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(Latitude.ToRad()) * Math.Cos(geoCoordinates.Latitude.ToRad()) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;
            return (float)d;
        }
    }
}
