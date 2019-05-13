using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Linq;
using System.Net;

namespace Santolibre.Map.Search.Geocoding.MapQuest
{
    /// <summary>
    /// https://developer.mapquest.com/documentation/open/geocoding-api/address/get/
    /// </summary>
    public class MapQuestGeocodingService : IGeocodingService
    {
        private readonly IConfiguration _configuration;

        public MapQuestGeocodingService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public GeocodingResult Search(string locationSerchString)
        {
            var webClient = new WebClient();
            var result = webClient.DownloadString($"http://open.mapquestapi.com/geocoding/v1/address?key={_configuration.GetValue<string>("AppSettings:MapQuestSearchApiKey")}&location=" + locationSerchString);
            var response = JsonConvert.DeserializeObject<MapQuestSearchResponse>(result);
            var foundLocation = response.Results.First().Locations.First();

            var geocodeQuality = MapQuestGeocodeQuality.Parse(foundLocation.GeocodeQualityCode);
            if (geocodeQuality == null)
            {
                return null;
            }
            if (geocodeQuality.AdministrativeAreaLevelConfidence == MapQuestGeocodeQualityConfidence.Unknown &&
                geocodeQuality.PostalCodeLevelConfidence == MapQuestGeocodeQualityConfidence.Unknown &&
                geocodeQuality.FullStreetLevelConfidence == MapQuestGeocodeQualityConfidence.Unknown)
            {
                return null;
            }

            var geocodingResult = new GeocodingResult()
            {
                ZipCode = foundLocation.PostalCode,
                GeoCoordinates = foundLocation.Location,
                Radius = foundLocation.GetRadius(),
                GeocodeQuality = geocodeQuality
            };

            if (!string.IsNullOrEmpty(foundLocation.Street))
                geocodingResult.Street = foundLocation.Street;
            if (!string.IsNullOrEmpty(foundLocation.AdminArea5))
                geocodingResult.AdminArea = foundLocation.AdminArea5;
            if (!string.IsNullOrEmpty(foundLocation.AdminArea1))
                geocodingResult.CountryCode = foundLocation.AdminArea1;

            return geocodingResult;
        }
    }
}
