namespace Santolibre.Map.Search.Geocoding.OpenCage
{
    public class OpenCageSearchResult
    {
        public OpenCageSearchResultComponents Components { get; set; }

        public int Confidence { get; set; }

        public string Formatted { get; set; }

        public GeoCoordinates Geometry { get; set; }

        public double GetRadius()
        {
            switch (Components.Type)
            {
                case "country":
                    return 100;
                case "state":
                    return 100;
                case "county":
                    return 50;
                case "city":
                    return 20;
                case "zip":
                    return 10;
                case "zip_extended":
                    return 10;
                case "neighborhood":
                    return 5;
                case "street":
                    return 1;
                case "intersection":
                    return 1;
                case "address":
                    return 1;
                case "point":
                    return 1;
            }
            return 10;
        }
    }
}
