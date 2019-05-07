using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Santolibre.Map.Search.Lib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Santolibre.Map.Search.Lib.Services
{
    public class SearchService : ISearchService
    {
        private readonly IDocumentService _documentService;
        private readonly ILocationSearchService _locationSearchService;
        private readonly ILogger<ISearchService> _logger;

        public SearchService(IDocumentService documentService, ILocationSearchService locationSearchService, ILogger<ISearchService> logger)
        {
            _documentService = documentService;
            _locationSearchService = locationSearchService;
            _logger = logger;
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

                var location = _locationSearchService.Search(locationTerm);
                if (location != null)
                {
                    var pointsOfInterest = GetPointsOfInterest(poiTerm.ToLower().Split(' ').Distinct().ToArray(), location.GeoLocation.Latitude, location.GeoLocation.Longitude, location.Radius, 200);
                    if (pointsOfInterest.Any())
                    {
                        searchResult.PointsOfInterest = pointsOfInterest;
                        searchResult.Location = location.GeoLocation;
                        searchResult.Radius = location.Radius;
                    }
                    else
                    {
                        searchResult.Name = location.Name;
                        searchResult.Location = location.GeoLocation;
                        searchResult.Radius = location.Radius;
                        searchResult.ZoomLevel = location.ZoomLevel;
                        searchResult.GeocodeQuality = location.GeocodeQuality;
                    }
                }
            }
            else
            {
                var unspecificTerm = searchTerm;

                var location = _locationSearchService.Search(unspecificTerm);
                if (location != null)
                {
                    searchResult.Name = location.Name;
                    searchResult.Location = location.GeoLocation;
                    searchResult.Radius = location.Radius;
                    searchResult.ZoomLevel = location.ZoomLevel;
                    searchResult.GeocodeQuality = location.GeocodeQuality;
                }
                else if (latitude.HasValue && longitude.HasValue)
                {
                    var pointsOfInterest = GetPointsOfInterest(unspecificTerm.ToLower().Split(' ').Distinct().ToArray(), latitude.Value, longitude.Value, null, 200);
                    if (pointsOfInterest.Any())
                    {
                        searchResult.PointsOfInterest = pointsOfInterest;
                        if (pointsOfInterest.Count == 1)
                        {
                            searchResult.Location = pointsOfInterest[0].Location;
                            searchResult.Radius = 1;

                        }
                        else
                        {
                            searchResult.Location = new GeoLocation() { Latitude = latitude.Value, Longitude = longitude.Value };
                            pointsOfInterest.Sort((x, y) => x.Location.GetDistanceToPoint(searchResult.Location).CompareTo(y.Location.GetDistanceToPoint(searchResult.Location)));
                            searchResult.Radius = pointsOfInterest[pointsOfInterest.Count / 2].Location.GetDistanceToPoint(searchResult.Location);
                        }
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

                var location = _locationSearchService.Search(locationTerm);
                if (location != null)
                {
                    var poiSuggestions = GetPointOfInterestSuggestions(poiTerm.ToLower().Split(' ').Distinct().Select(x => x + "*").ToArray(), location.GeoLocation.Latitude, location.GeoLocation.Longitude, location.Radius, 5);
                    if (poiSuggestions.Any())
                    {
                        poiSuggestions.ForEach(x => { x.Value = $"{x.Value} {combinerTerm} {location.Name}"; });
                        suggestions.AddRange(poiSuggestions);
                    }
                    else
                    {
                        suggestions.Add(new Suggestion() { Value = location.Name });
                    }
                }
            }
            else if (latitude.HasValue && longitude.HasValue)
            {
                var unspecificTerm = searchTerm;

                var location = _locationSearchService.Search(unspecificTerm);
                if (location != null)
                {
                    suggestions.Add(new Suggestion() { Value = location.Name });
                }
                else if (latitude.HasValue && longitude.HasValue)
                {
                    suggestions.AddRange(GetPointOfInterestSuggestions(unspecificTerm.ToLower().Split(' ').Distinct().Select(x => x + "*").ToArray(), latitude.Value, longitude.Value, null, 5));
                }
            }

            return suggestions;
        }

        private List<PointOfInterest> GetPointsOfInterest(string[] poiTerms, double latitude, double longitude, double? radius, int take)
        {
            using (var session = _documentService.OpenDocumentSession())
            {
                var query = session.Query<PointOfInterest_ByTagsEnglishNameAndCoordinates.Result, PointOfInterest_ByTagsEnglishNameAndCoordinates>();

                if (radius.HasValue)
                {
                    query = query.Spatial(x => x.Location, y => y.WithinRadius(radius.Value, latitude, longitude));
                }

                query = query.Search(x => x.TagKeyValueSearch, poiTerms[0]);
                poiTerms.Skip(1).ToList().ForEach(x => { query = query.Search(y => y.TagKeyValueSearch, x, options: SearchOptions.And); });

                var pointsOfInterest = query
                    .OrderByDistance(x => x.Location, latitude, longitude)
                    .ProjectInto<PointOfInterest>()
                    .Take(take)
                    .ToList();
                return pointsOfInterest;
            }
        }

        private List<Suggestion> GetPointOfInterestSuggestions(string[] poiTerms, double latitude, double longitude, double? radius, int take)
        {
            var pointsOfInterest = GetPointsOfInterest(poiTerms, latitude, longitude, radius, take);

            var suggestions = new List<Suggestion>();
            foreach (var pointOfInterest in pointsOfInterest)
            {
                var valueComponents = pointOfInterest.Type.Split('_').ToList();
                if (!string.IsNullOrEmpty(pointOfInterest.Name))
                {
                    valueComponents.AddRange(pointOfInterest.Name.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries));
                }
                suggestions.Add(new Suggestion() { Value = string.Join(" ", valueComponents.Distinct().Select(x => x.First().ToString().ToUpper() + x.Substring(1))) });
            }
            return suggestions.Distinct().ToList();
        }
    }
}
