namespace Santolibre.Map.Search.Lib.Models
{
    public class Location
    {
        public GeoLocation GeoLocation { get; set; }
        public string Name { get; set; }
        public string ZipCode { get; set; }
        public double Radius { get; set; }
        public GeocodeQuality GeocodeQuality { get; set; }
    }
}
