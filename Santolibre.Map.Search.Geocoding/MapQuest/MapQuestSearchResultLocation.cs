using Newtonsoft.Json;

namespace Santolibre.Map.Search.Geocoding.MapQuest
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
        public GeoCoordinates Location { get; set; }

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
                    return 20;
                case "ZIP":
                    return 10;
                case "ZIP_EXTENDED":
                    return 10;
                case "NEIGHBORHOOD":
                    return 5;
                case "STREET":
                    return 1;
                case "INTERSECTION":
                    return 1;
                case "ADDRESS":
                    return 1;
                case "POINT":
                    return 1;
            }
            return 10;
        }
    }
}
