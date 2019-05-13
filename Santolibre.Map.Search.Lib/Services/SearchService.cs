using Santolibre.Map.Search.Geocoding;
using Santolibre.Map.Search.Lib.Models;
using Santolibre.Map.Search.Lib.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Santolibre.Map.Search.Lib.Services
{
    public class SearchService : ISearchService
    {
        private readonly IGeocodingService _geocodingService;
        private readonly ILocalizationService _localizationService;
        private readonly IPointOfInterestRepository _pointOfInterestRepository;

        public SearchService(IGeocodingService geocodingService, ILocalizationService localizationService, IPointOfInterestRepository pointOfInterestRepository)
        {
            _geocodingService = geocodingService;
            _localizationService = localizationService;
            _pointOfInterestRepository = pointOfInterestRepository;
        }

        public SearchResult GetSearchResult(string searchTerm, double? latitude, double? longitude)
        {
            var searchResult = new SearchResult();

            var match = Regex.Match(searchTerm, @"^(.{3,})\s(near|around|in)\s(.{3,})$");
            if (match.Success)
            {
                var poiTerm = match.Groups[1].Captures[0].Value;
                var combinerTerm = match.Groups[2].Captures[0].Value;
                var locationTerm = match.Groups[3].Captures[0].Value;

                var geocodingResult = _geocodingService.Search(locationTerm);
                if (geocodingResult != null)
                {
                    searchResult.Center = new GeoCoordinates(geocodingResult.GeoCoordinates.Latitude, geocodingResult.GeoCoordinates.Longitude);
                    searchResult.Radius = geocodingResult.Radius;

                    var pointsOfInterest = _pointOfInterestRepository.GetPointsOfInterest(poiTerm, false, geocodingResult.GeoCoordinates.Latitude, geocodingResult.GeoCoordinates.Longitude, geocodingResult.Radius, 200);
                    if (pointsOfInterest.Any())
                    {
                        searchResult.Locations = pointsOfInterest.ConvertAll(x => (Location)x);

                    }
                    else
                    {
                        searchResult.Locations = new List<Location>()
                        {
                            new Location()
                            {
                                Name = GetGeocodingResultName(geocodingResult),
                                GeoCoordinates = new GeoCoordinates(geocodingResult.GeoCoordinates.Latitude, geocodingResult.GeoCoordinates.Longitude),
                                Category = "address",
                                Type = "address"
                            }
                        };
                    }
                }
            }
            else
            {
                var unspecificTerm = searchTerm;

                var geocodingResult = _geocodingService.Search(unspecificTerm);
                if (geocodingResult != null)
                {
                    searchResult.Center = new GeoCoordinates(geocodingResult.GeoCoordinates.Latitude, geocodingResult.GeoCoordinates.Longitude);
                    searchResult.Radius = geocodingResult.Radius;
                    searchResult.Locations = new List<Location>()
                    {
                        new Location()
                        {
                            Name = GetGeocodingResultName(geocodingResult),
                            GeoCoordinates = new GeoCoordinates(geocodingResult.GeoCoordinates.Latitude, geocodingResult.GeoCoordinates.Longitude),
                            Category = "address",
                            Type = "address"
                        }
                    };
                }
                else if (latitude.HasValue && longitude.HasValue)
                {
                    var pointsOfInterest = _pointOfInterestRepository.GetPointsOfInterest(unspecificTerm, false, latitude.Value, longitude.Value, null, 200);
                    if (pointsOfInterest.Any())
                    {
                        if (pointsOfInterest.Count == 1)
                        {
                            searchResult.Center = pointsOfInterest[0].GeoCoordinates;
                            searchResult.Radius = 1;
                        }
                        else
                        {
                            searchResult.Center = new GeoCoordinates(latitude.Value, longitude.Value);
                            pointsOfInterest.Sort((x, y) => x.GeoCoordinates.GetDistanceToPoint(searchResult.Center).CompareTo(y.GeoCoordinates.GetDistanceToPoint(searchResult.Center)));
                            searchResult.Radius = pointsOfInterest[pointsOfInterest.Count / 2].GeoCoordinates.GetDistanceToPoint(searchResult.Center);
                        }
                        searchResult.Locations = pointsOfInterest.ConvertAll(x => (Location)x);
                    }
                }
            }

            return searchResult;
        }

        public List<Suggestion> GetSuggestions(string searchTerm, double? latitude, double? longitude)
        {
            var suggestions = new List<Suggestion>();

            var match = Regex.Match(searchTerm, @"^(.{3,})\s(near|around|in)\s(.{3,})$");
            if (match.Success)
            {
                var poiTerm = match.Groups[1].Captures[0].Value;
                var combinerTerm = match.Groups[2].Captures[0].Value;
                var locationTerm = match.Groups[3].Captures[0].Value;

                var geocodingResult = _geocodingService.Search(locationTerm);
                if (geocodingResult != null)
                {
                    var poiSuggestions = GetPointOfInterestSuggestions(poiTerm, geocodingResult.GeoCoordinates.Latitude, geocodingResult.GeoCoordinates.Longitude, geocodingResult.Radius, 5);
                    if (poiSuggestions.Any())
                    {
                        poiSuggestions.ForEach(x => { x.Value = $"{x.Value} {combinerTerm} {GetGeocodingResultName(geocodingResult)}"; });
                        suggestions.AddRange(poiSuggestions);
                    }
                    else
                    {
                        suggestions.Add(new Suggestion() { Value = GetGeocodingResultName(geocodingResult) });
                    }
                }
            }
            else if (latitude.HasValue && longitude.HasValue)
            {
                var unspecificTerm = searchTerm;

                var geocodingResult = _geocodingService.Search(unspecificTerm);
                if (geocodingResult != null)
                {
                    suggestions.Add(new Suggestion() { Value = GetGeocodingResultName(geocodingResult) });
                }
                else if (latitude.HasValue && longitude.HasValue)
                {
                    suggestions.AddRange(GetPointOfInterestSuggestions(unspecificTerm, latitude.Value, longitude.Value, null, 5));
                }
            }

            return suggestions;
        }

        private List<Suggestion> GetPointOfInterestSuggestions(string searchTerm, double latitude, double longitude, double? radius, int take)
        {
            var pointsOfInterest = _pointOfInterestRepository.GetPointsOfInterest(searchTerm, true, latitude, longitude, radius, take);

            var suggestions = new List<Suggestion>();
            foreach (var pointOfInterest in pointsOfInterest)
            {
                var valueComponents = pointOfInterest.Type.Split(new char[] { '_', ';' }).ToList();
                if (!string.IsNullOrEmpty(pointOfInterest.Name))
                {
                    valueComponents.AddRange(pointOfInterest.Name.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries));
                }
                suggestions.Add(new Suggestion() { Value = string.Join(" ", valueComponents.Distinct().Select(x => x.First().ToString().ToUpper() + x.Substring(1))) });
            }
            return suggestions.Distinct().ToList();
        }

        private string GetGeocodingResultName(GeocodingResult geocodingResult)
        {
            var name = new List<string>();
            if (!string.IsNullOrEmpty(geocodingResult.Street))
                name.Add(geocodingResult.Street);
            if (!string.IsNullOrEmpty(geocodingResult.AdminArea))
                name.Add(geocodingResult.AdminArea);
            if (!string.IsNullOrEmpty(geocodingResult.CountryCode))
                name.Add(_localizationService.GetCountryName(geocodingResult.CountryCode));
            return string.Join(", ", name);
        }
    }
}
