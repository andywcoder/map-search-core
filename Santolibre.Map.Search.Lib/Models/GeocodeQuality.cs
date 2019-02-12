using System;
using System.Text.RegularExpressions;

namespace Santolibre.Map.Search.Lib.Models
{
    public class GeocodeQuality
    {
        public GeocodeQualityGranularity Granularity { get; private set; }
        public GeocodeQualityConfidence FullStreetLevelConfidence { get; private set; }
        public GeocodeQualityConfidence AdministrativeAreaLevelConfidence { get; private set; }
        public GeocodeQualityConfidence PostalCodeLevelConfidence { get; private set; }

        public static GeocodeQuality Parse(string geocodeQualityCode)
        {
            var match = Regex.Match(geocodeQualityCode, @"^([PLIBAZ]\d)([ABCX])([ABCX])([ABCX])$");
            if (match.Success)
            {
                return new GeocodeQuality()
                {
                    Granularity = ParseGeocodeQualityGranularity(match.Groups[1].Captures[0].Value),
                    FullStreetLevelConfidence = ParseGeocodeQualityConfidence(match.Groups[2].Captures[0].Value),
                    AdministrativeAreaLevelConfidence = ParseGeocodeQualityConfidence(match.Groups[3].Captures[0].Value),
                    PostalCodeLevelConfidence = ParseGeocodeQualityConfidence(match.Groups[4].Captures[0].Value)
                };
            }
            else
            {
                return null;
            }
        }

        private static GeocodeQualityConfidence ParseGeocodeQualityConfidence(string value)
        {
            switch (value)
            {
                case "A":
                    return GeocodeQualityConfidence.Exact;
                case "B":
                    return GeocodeQualityConfidence.Good;
                case "C":
                    return GeocodeQualityConfidence.Approx;
                case "X":
                    return GeocodeQualityConfidence.Unknown;
                default:
                    throw new Exception("Unknown geocode quality confidence");
            }
        }

        private static GeocodeQualityGranularity ParseGeocodeQualityGranularity(string value)
        {
            switch (value)
            {
                case "P1":
                    return GeocodeQualityGranularity.Point;
                case "L1":
                    return GeocodeQualityGranularity.Address;
                case "I1":
                    return GeocodeQualityGranularity.Intersection;
                case "B1":
                    return GeocodeQualityGranularity.Street;
                case "B2":
                    return GeocodeQualityGranularity.Street;
                case "B3":
                    return GeocodeQualityGranularity.Street;
                case "A1":
                    return GeocodeQualityGranularity.Country;
                case "A3":
                    return GeocodeQualityGranularity.State;
                case "A4":
                    return GeocodeQualityGranularity.County;
                case "A5":
                    return GeocodeQualityGranularity.City;
                case "A6":
                    return GeocodeQualityGranularity.Neighborhood;
                case "Z1":
                    return GeocodeQualityGranularity.Zip;
                case "Z2":
                    return GeocodeQualityGranularity.Zip;
                case "Z3":
                    return GeocodeQualityGranularity.Zip;
                case "Z4":
                    return GeocodeQualityGranularity.Zip;
                default:
                    throw new Exception("Unknown geocode quality granularity");
            }
        }
    }
}
