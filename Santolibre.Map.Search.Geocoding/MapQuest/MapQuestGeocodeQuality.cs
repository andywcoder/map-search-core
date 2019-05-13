using System;
using System.Text.RegularExpressions;

namespace Santolibre.Map.Search.Geocoding.MapQuest
{
    public class MapQuestGeocodeQuality
    {
        public MapQuestGeocodeQualityGranularity Granularity { get; private set; }
        public MapQuestGeocodeQualityConfidence FullStreetLevelConfidence { get; private set; }
        public MapQuestGeocodeQualityConfidence AdministrativeAreaLevelConfidence { get; private set; }
        public MapQuestGeocodeQualityConfidence PostalCodeLevelConfidence { get; private set; }

        public static MapQuestGeocodeQuality Parse(string geocodeQualityCode)
        {
            var match = Regex.Match(geocodeQualityCode, @"^([PLIBAZ]\d)([ABCX])([ABCX])([ABCX])$");
            if (match.Success)
            {
                return new MapQuestGeocodeQuality()
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

        private static MapQuestGeocodeQualityConfidence ParseGeocodeQualityConfidence(string value)
        {
            switch (value)
            {
                case "A":
                    return MapQuestGeocodeQualityConfidence.Exact;
                case "B":
                    return MapQuestGeocodeQualityConfidence.Good;
                case "C":
                    return MapQuestGeocodeQualityConfidence.Approx;
                case "X":
                    return MapQuestGeocodeQualityConfidence.Unknown;
                default:
                    throw new Exception("Unknown geocode quality confidence");
            }
        }

        private static MapQuestGeocodeQualityGranularity ParseGeocodeQualityGranularity(string value)
        {
            switch (value)
            {
                case "P1":
                    return MapQuestGeocodeQualityGranularity.Point;
                case "L1":
                    return MapQuestGeocodeQualityGranularity.Address;
                case "I1":
                    return MapQuestGeocodeQualityGranularity.Intersection;
                case "B1":
                    return MapQuestGeocodeQualityGranularity.Street;
                case "B2":
                    return MapQuestGeocodeQualityGranularity.Street;
                case "B3":
                    return MapQuestGeocodeQualityGranularity.Street;
                case "A1":
                    return MapQuestGeocodeQualityGranularity.Country;
                case "A3":
                    return MapQuestGeocodeQualityGranularity.State;
                case "A4":
                    return MapQuestGeocodeQualityGranularity.County;
                case "A5":
                    return MapQuestGeocodeQualityGranularity.City;
                case "A6":
                    return MapQuestGeocodeQualityGranularity.Neighborhood;
                case "Z1":
                    return MapQuestGeocodeQualityGranularity.Zip;
                case "Z2":
                    return MapQuestGeocodeQualityGranularity.Zip;
                case "Z3":
                    return MapQuestGeocodeQualityGranularity.Zip;
                case "Z4":
                    return MapQuestGeocodeQualityGranularity.Zip;
                default:
                    throw new Exception("Unknown geocode quality granularity");
            }
        }
    }
}
