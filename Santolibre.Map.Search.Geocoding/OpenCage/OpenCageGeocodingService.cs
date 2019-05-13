using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Linq;
using System.Net;

namespace Santolibre.Map.Search.Geocoding.OpenCage
{
    public class OpenCageGeocodingService : IGeocodingService
    {
        private readonly IConfiguration _configuration;

        public OpenCageGeocodingService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public GeocodingResult Search(string locationSerchString)
        {
            var webClient = new WebClient();
            var result = webClient.DownloadString($"https://api.opencagedata.com/geocode/v1/json?key={_configuration.GetValue<string>("AppSettings:OpenCageApiKey")}&q=" + locationSerchString);
            var response = JsonConvert.DeserializeObject<OpenCageSearchResponse>(result);
            var foundLocation = response.Results.First();

            var geocodingResult = new GeocodingResult()
            {
                ZipCode = foundLocation.Components.Postcode,
                GeoCoordinates = foundLocation.Geometry,
                Radius = foundLocation.GetRadius(),
                GeocodeQuality = null
            };

            return geocodingResult;
        }
    }
}
