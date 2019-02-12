using System;
using Newtonsoft.Json;

namespace Santolibre.Map.Search.Lib.Models
{
    public class MapQuestSearchResultLocation
    {
        public string Street { get; set; }
        public string AdminArea6 { get; set; }
        public string AdminArea6Type { get; set; }
        public string AdminArea5 { get; set; }
        public string AdminArea5Type { get; set; }
        public string AdminArea4 { get; set; }
        public string AdminArea4Type { get; set; }
        public string AdminArea3 { get; set; }
        public string AdminArea3Type { get; set; }
        public string AdminArea2 { get; set; }
        public string AdminArea2Type { get; set; }
        public string AdminArea1 { get; set; }
        public string AdminArea1Type { get; set; }
        public string PostalCode { get; set; }
        public string GeocodeQualityCode { get; set; }
        public string GeocodeQuality { get; set; }
        public string DragPoint { get; set; }
        public string SideOfStreet { get; set; }
        public string LinkId { get; set; }
        public string UnknownInput { get; set; }
        public string Type { get; set; }

        [JsonProperty("latLng")]
        public GeoLocation Location { get; set; }

        public double GetRadius()
        {
            switch (GeocodeQuality)
            {
                case "COUNTRY":
                    return 100;
                case "STATE":
                    return 100;
                case "COUNTY":
                    return 50;
                case "CITY":
                    return 30;
                case "ZIP":
                    return 30;
                case "ZIP_EXTENDED":
                    return 30;
                case "NEIGHBORHOOD":
                    return 10;
                case "STREET":
                    return 5;
                case "INTERSECTION":
                    return 5;
                case "ADDRESS":
                    return 5;
                case "POINT":
                    return 5;
            }
            return 10;
        }

        public int GetZoomLevel()
        {
            switch (GeocodeQuality)
            {
                case "COUNTRY":
                    return 10;
                case "STATE":
                    return 10;
                case "COUNTY":
                    return 11;
                case "CITY":
                    return 14;
                case "ZIP":
                    return 15;
                case "ZIP_EXTENDED":
                    return 15;
                case "NEIGHBORHOOD":
                    return 16;
                case "STREET":
                    return 18;
                case "INTERSECTION":
                    return 18;
                case "ADDRESS":
                    return 18;
                case "POINT":
                    return 18;
            }
            return 15;
        }
    }
}