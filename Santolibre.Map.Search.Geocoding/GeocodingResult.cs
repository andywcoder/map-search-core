namespace Santolibre.Map.Search.Geocoding
{
    public class GeocodingResult
    {
        public GeoCoordinates GeoCoordinates { get; set; }
        public string Type { get; set; }
        public string ZipCode { get; internal set; }
        public double Radius { get; internal set; }
        public string Street { get; internal set; }
        public string AdminArea { get; internal set; }
        public string CountryCode { get; internal set; }
    }
}
