using Microsoft.Extensions.Logging;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Streams;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Santolibre.Map.Search.Lib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Santolibre.Map.Search.Lib.Services
{
    public class PointOfInterestService : IPointOfInterestService
    {
        private readonly IDocumentService _documentService;
        private readonly ILocationSearchService _locationSearchService;
        private readonly ILogger<IPointOfInterestService> _logger;

        public PointOfInterestService(IDocumentService documentService, ILocationSearchService locationSearchService, ILogger<IPointOfInterestService> logger)
        {
            _documentService = documentService;
            _locationSearchService = locationSearchService;
            _logger = logger;
        }

        public void ImportPointsOfInterest(string filename)
        {
            _logger.LogInformation("Importing points of interest from " + filename);
            _logger.LogTrace("Filtering out points of interest that don't contain one of the following tag keys: sport, amenity, leisure, shop, tourism");

            var includeElementsWithTagKeys = new string[] { "sport", "amenity", "leisure", "shop", "tourism" };
            var excludeElementsWithTagValues = new string[] { "guidepost" };

            var pointsOfInterest = new List<PointOfInterest>();
            using (var fileStream = new FileInfo(filename).OpenRead())
            {
                using (var source = new PBFOsmStreamSource(fileStream))
                {
                    var filtered = from element in source
                                   where element.Type == OsmGeoType.Node ||
                                         (element.Type == OsmGeoType.Way && includeElementsWithTagKeys.Any(x => element.Tags.ContainsKey(x)))
                                   select element;

                    using (var completeSource = filtered.ToComplete())
                    {
                        var totalCounter = 0;
                        var counter = 0;

                        foreach (var element in completeSource)
                        {
                            if (includeElementsWithTagKeys.Any(x => element.Tags.ContainsKey(x)) &&
                                !excludeElementsWithTagValues.Any(x => element.Tags.Any(y => y.Value == x)))
                            {
                                var pointOfInterest = new PointOfInterest
                                {
                                    Id = element.Id.ToString(),
                                    Tags = element.Tags.ToDictionary(x => x.Key, x => x.Value),
                                    DateUpdated = DateTime.UtcNow
                                };
                                SetName(element, pointOfInterest);
                                SetCategoryAndType(element, pointOfInterest, includeElementsWithTagKeys);
                                SetFilteredTagKeyValues(element, pointOfInterest);
                                SetLocation(element, pointOfInterest);

                                if (!pointsOfInterest.Any(x => x.Id == pointOfInterest.Id) && pointOfInterest.FilteredTagKeyValues.Any())
                                {
                                    _logger.LogTrace($"Adding point of interest {counter} to import batch, Id={pointOfInterest.Id}");
                                    counter++;
                                    totalCounter++;
                                    pointsOfInterest.Add(pointOfInterest);
                                }
                                else
                                {
                                    _logger.LogWarning("Duplicated point of interest detecetd");
                                }
                            }

                            if (pointsOfInterest.Count >= 5000)
                            {
                                SavePointsOfInterest(pointsOfInterest);
                            }

                            /*if (totalCounter > 20000)
                            {
                                break;
                            }*/
                        }
                        SavePointsOfInterest(pointsOfInterest);

                        _logger.LogInformation($"{totalCounter} points of interest imported");
                    }
                }
            }
        }

        private void SetName(ICompleteOsmGeo element, PointOfInterest pointOfInterest)
        {
            if (element.Tags.ContainsKey("name"))
            {
                pointOfInterest.Name = element.Tags["name"];
            }
        }

        private void SetCategoryAndType(ICompleteOsmGeo element, PointOfInterest pointOfInterest, string[] tagKeys)
        {
            foreach (var tagKey in tagKeys)
            {
                if (element.Tags.ContainsKey(tagKey))
                {
                    pointOfInterest.Category = tagKey;
                    pointOfInterest.Type = element.Tags[tagKey];
                    break;
                }
            }
        }

        private void SetFilteredTagKeyValues(ICompleteOsmGeo element, PointOfInterest pointOfInterest)
        {
            // Filter tags by keys and values
            var removeTagsByValues = new string[] { "no", "limited" };
            var removeTagsByKeys = new string[] { "artist_name", "capacity", "check_date", "collection_times", "color", "colour", "created_by", "currency", "description", "ele", "email", "fax", "fee", "fixme", "height", "image", "is_in", "layer", "level", "material", "note", "opening_hours", "payment", "phone", "ref", "rooms", "seats", "source", "start_date", "stop_position", "uic", "url", "website", "wikidata" };
            var removeTagsByKeysStartsWith = new string[] { "addr:", "contact:", "currency:", "note:", "openGeoDB:", "opening_hours:", "payment:", "ref:", "uic:", "uic_" };

            element.Tags.RemoveAll(x =>
                string.IsNullOrEmpty(x.Key) ||
                string.IsNullOrEmpty(x.Value) ||
                Regex.IsMatch(x.Value, @"^\d+$") ||
                removeTagsByValues.Contains(x.Value) ||
                removeTagsByKeys.Contains(x.Key) ||
                removeTagsByKeysStartsWith.Any(y => x.Key.StartsWith(y)));

            // Merge keys and values and filter list
            var removeTagKeyValues = new string[] { "access", "alt_name", "amenity", "brand", "container", "cuisine", "denomination", "information", "loc_name", "material", "name", "official_name", "old_name", "operator", "religion", "surface", "tourism", "wikipedia", "yes" };
            var removeTagKeyValuesStartsWith = new string[] { "name:", "post_box:" };

            var filteredTagKeyValues = element.Tags.Select(x => x.Key.ToLower()).Concat(element.Tags.Select(x => x.Value.ToLower())).Distinct().ToList();
            filteredTagKeyValues.RemoveAll(x =>
                removeTagKeyValues.Contains(x) ||
                removeTagKeyValuesStartsWith.Any(y => x.StartsWith(y)));

            pointOfInterest.FilteredTagKeyValues = filteredTagKeyValues;

            _logger.LogTrace($"Filtered tags, Id={pointOfInterest.Id}, FilteredTagKeyValues={string.Join(", ", pointOfInterest.FilteredTagKeyValues)}");
        }

        private void SetLocation(ICompleteOsmGeo element, PointOfInterest pointOfInterest)
        {
            if (element is Node node && node.Longitude.HasValue && node.Latitude.HasValue)
            {
                pointOfInterest.Location = new GeoLocation() { Latitude = node.Latitude.Value, Longitude = node.Longitude.Value };
            }
            else
            {
                if (element is CompleteWay way)
                {
                    pointOfInterest.Location = new GeoLocation() { Latitude = way.Nodes.Average(x => x.Latitude.Value), Longitude = way.Nodes.Average(x => x.Longitude.Value) };
                }
            }
        }

        private void SavePointsOfInterest(List<PointOfInterest> pointsOfInterest)
        {
            try
            {
                using (var session = _documentService.OpenDocumentSession())
                {
                    foreach (var pointOfInterest in pointsOfInterest)
                    {
                        session.Store(pointOfInterest);
                    }
                    _logger.LogTrace("Saving points of interest batch");
                    session.SaveChanges();
                }
                _logger.LogTrace("Clearing points of interest batch");

                GC.Collect();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "There was an error while saving the data (" + e.Message + ")");
            }
            pointsOfInterest.Clear();
        }

        public void RemoveOldPointsOfInterest(int days)
        {
            _logger.LogInformation($"Removing points of interest that are older than {days} days");

            var dateTime = DateTime.UtcNow - new TimeSpan(days, 0, 0, 0);
            _documentService.RunDeleteByQueryOperation(new DeleteByQueryOperation<PointOfInterest, PointOfInterest_ByDateUpdated>(x => x.DateUpdated < dateTime));
            _logger.LogInformation("Points of interest removed");
        }

        public void CompactPointsOfInterest()
        {
            _logger.LogInformation("Compacting document store");
            _documentService.CompactDocumentStore();
            _logger.LogInformation("Document store compacted");
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
                    var pointsOfInterest = GetPointsOfInterest(poiTerm.Split(' '), location.GeoLocation.Latitude, location.GeoLocation.Longitude, location.Radius, 200);
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
                    var pointsOfInterest = GetPointsOfInterest(unspecificTerm.Split(' '), latitude.Value, longitude.Value, null, 200);
                    if (pointsOfInterest.Any())
                    {
                        searchResult.PointsOfInterest = pointsOfInterest;
                        if (pointsOfInterest.Count == 1)
                        {
                            searchResult.Location = pointsOfInterest[0].Location;
                            searchResult.Radius = 5;

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
                    var poiSuggestions = GetPointOfInterestSuggestions(poiTerm.Split(' ').Select(x => x + "*").ToArray(), location.GeoLocation.Latitude, location.GeoLocation.Longitude, location.Radius, 5);
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
                    suggestions.AddRange(GetPointOfInterestSuggestions(unspecificTerm.Split(' ').Select(x => x + "*").ToArray(), latitude.Value, longitude.Value, null, 5));
                }
            }

            return suggestions;
        }

        private List<PointOfInterest> GetPointsOfInterest(string[] poiTerms, double latitude, double longitude, double? radius, int take)
        {
            using (var session = _documentService.OpenDocumentSession())
            {
                var query = session.Query<PointOfInterest_ByTagsAndCoordinates.Result, PointOfInterest_ByTagsAndCoordinates>();
                if (radius.HasValue)
                {
                    query = query.Spatial(x => x.Location, y => y.WithinRadius(radius.Value, latitude, longitude));
                }
                query = query.Search(x => x.TagValueSearch, poiTerms[0]);
                for (var i = 1; i < poiTerms.Length; i++)
                {
                    query = query.Search(x => x.TagValueSearch, poiTerms[i], options: SearchOptions.And);
                }
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
                    valueComponents.AddRange(pointOfInterest.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                }
                suggestions.Add(new Suggestion() { Value = string.Join(" ", valueComponents.Select(x => x.First().ToString().ToUpper() + x.Substring(1))) });
            }
            return suggestions.Distinct().ToList();
        }
    }
}
