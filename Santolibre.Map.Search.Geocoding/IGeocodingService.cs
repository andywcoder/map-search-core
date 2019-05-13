namespace Santolibre.Map.Search.Geocoding
{
    public interface IGeocodingService
    {
        GeocodingResult Search(string locationSerchString);
    }
}
