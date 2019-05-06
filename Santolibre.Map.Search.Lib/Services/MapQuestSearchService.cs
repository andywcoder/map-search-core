using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Santolibre.Map.Search.Lib.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Santolibre.Map.Search.Lib.Services
{
    /// <summary>
    /// https://developer.mapquest.com/documentation/open/geocoding-api/address/get/
    /// </summary>
    public class MapQuestSearchService : ILocationSearchService
    {
        private readonly ILocalizationService _localizationService;
        private readonly IConfiguration _configuration;

        public MapQuestSearchService(ILocalizationService localizationService, IConfiguration configuration)
        {
            _localizationService = localizationService;
            _configuration = configuration;
        }

        public Location Search(string location)
        {
            var webClient = new WebClient();
            var result = webClient.DownloadString($"http://open.mapquestapi.com/geocoding/v1/address?key={_configuration.GetValue<string>("AppSettings:MapQuestSearchApiKey")}&location=" + location);
            var response = JsonConvert.DeserializeObject<MapQuestSearchResponse>(result);
            var foundLocation = response.Results.First().Locations.First();

            var geocodeQuality = GeocodeQuality.Parse(foundLocation.GeocodeQualityCode);
            if (geocodeQuality == null)
            {
                return null;
            }
            if (geocodeQuality.AdministrativeAreaLevelConfidence == GeocodeQualityConfidence.Unknown &&
                geocodeQuality.PostalCodeLevelConfidence == GeocodeQualityConfidence.Unknown &&
                geocodeQuality.FullStreetLevelConfidence == GeocodeQualityConfidence.Unknown)
            {
                return null;
            }

            var name = new List<string>();
            if (!string.IsNullOrEmpty(foundLocation.Street))
                name.Add(foundLocation.Street);
            if (!string.IsNullOrEmpty(foundLocation.AdminArea5))
                name.Add(foundLocation.AdminArea5);
            if (!string.IsNullOrEmpty(foundLocation.AdminArea1))
                name.Add(_localizationService.GetCountryName(foundLocation.AdminArea1));

            return new Location()
            {
                Name = string.Join(", ", name),
                ZipCode = foundLocation.PostalCode,
                GeoLocation = foundLocation.Location,
                ZoomLevel = foundLocation.GetZoomLevel(),
                Radius = foundLocation.GetRadius(),
                GeocodeQuality = geocodeQuality
            };
        }
    }
}
