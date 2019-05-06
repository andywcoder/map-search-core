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
    public class SearchService : ISearchService
    {
        private readonly IDocumentService _documentService;
        private readonly ILocationSearchService _locationSearchService;
        private readonly ILogger<SearchService> _logger;

        public SearchService(IDocumentService documentService, ILocationSearchService locationSearchService, ILogger<SearchService> logger)
        {
            _documentService = documentService;
            _locationSearchService = locationSearchService;
            _logger = logger;
        }

        public void ImportPointsOfInterest(string filename)
        {
            _logger.LogInformation("Importing points of interest from " + filename);
            _logger.LogTrace("Filtering out points of interest that don't contain one of the following tag keys: sport, amenity, leisure, shop, tourism");

            var tagKeys = new string[] { "sport", "amenity", "leisure", "shop", "tourism" };

            var pointsOfInterest = new List<PointOfInterest>();
            using (var fileStream = new FileInfo(filename).OpenRead())
            {
                using (var source = new PBFOsmStreamSource(fileStream))
                {
                    var filtered = from element in source
                                   where element.Type == OsmGeoType.Node ||
                                         (element.Type == OsmGeoType.Way && tagKeys.Any(x => element.Tags.ContainsKey(x)))
                                   select element;

                    using (var completeSource = filtered.ToComplete())
                    {
                        var totalCounter = 0;
                        var counter = 0;

                        foreach (var element in completeSource)
                        {
                            if (tagKeys.Any(x => element.Tags.ContainsKey(x) && !element.Tags.Any(y => y.Value == "guidepost")))
                            {
                                var pointOfInterest = new PointOfInterest
                                {
                                    DateUpdated = DateTime.UtcNow,
                                    Id = element.Id.ToString(),
                                    Tags = element.Tags.ToDictionary(x => x.Key, x => x.Value)
                                };
                                if (element.Tags.ContainsKey("name"))
                                {
                                    pointOfInterest.Name = element.Tags["name"];
                                }
                                foreach (var tagKey in tagKeys)
                                {
                                    if (element.Tags.ContainsKey(tagKey))
                                    {
                                        pointOfInterest.Category = tagKey;
                                        pointOfInterest.Type = element.Tags[tagKey];
                                        break;
                                    }
                                }

                                element.Tags.RemoveAll(x => string.IsNullOrEmpty(x.Key));
                                element.Tags.RemoveAll(x =>
                                    x.Value == "no" ||
                                    x.Value == "limited" ||
                                    Regex.IsMatch(x.Value, @"^\d+$"));
                                element.Tags.RemoveAll(x =>
                                    x.Key == "website" ||
                                    x.Key == "phone" ||
                                    x.Key == "source" ||
                                    x.Key == "opening_hours" ||
                                    x.Key == "image" ||
                                    x.Key == "ele" ||
                                    x.Key == "ref" ||
                                    x.Key == "uic" ||
                                    x.Key == "payment" ||
                                    x.Key == "currency" ||
                                    x.Key == "layer" ||
                                    x.Key == "collection_times" ||
                                    x.Key == "created_by" ||
                                    x.Key == "level" ||
                                    x.Key == "email" ||
                                    x.Key == "wikidata" ||
                                    x.Key == "artist_name" ||
                                    x.Key == "start_date" ||
                                    x.Key == "stop_position" ||
                                    x.Key == "fax" ||
                                    x.Key == "height" ||
                                    x.Key == "fee" ||
                                    x.Key == "note" ||
                                    x.Key == "capacity" ||
                                    x.Key == "url" ||
                                    x.Key == "rooms" ||
                                    x.Key == "seats" ||
                                    x.Key == "fixme" ||
                                    x.Key == "colour" ||
                                    x.Key == "color" ||
                                    x.Key == "material" ||
                                    x.Key.StartsWith("openGeoDB:") ||
                                    x.Key.StartsWith("note:") ||
                                    x.Key.StartsWith("ref:") ||
                                    x.Key.StartsWith("uic:") ||
                                    x.Key.StartsWith("uic_") ||
                                    x.Key.StartsWith("payment:") ||
                                    x.Key.StartsWith("currency:") ||
                                    x.Key.StartsWith("addr:") ||
                                    x.Key.StartsWith("opening_hours:") ||
                                    x.Key.StartsWith("contact:"));

                                //_logger.LogTrace($"Filtered tags keys: {string.Join(", ", element.Tags.Select(x => x.Key))}");
                                //_logger.LogTrace($"Filtered tags values: {string.Join(", ", element.Tags.Select(x => x.Value))}");

                                pointOfInterest.FilteredTagKeyValues = element.Tags.Select(x => x.Key.ToLower()).Concat(element.Tags.Select(x => x.Value.ToLower())).Distinct().ToList();
                                pointOfInterest.FilteredTagKeyValues.RemoveAll(x =>
                                    x == "yes" ||
                                    x == "information" ||
                                    x == "name" ||
                                    x == "description" ||
                                    x == "old_name" ||
                                    x == "official_name" ||
                                    x == "loc_name" ||
                                    x == "access" ||
                                    x == "surface" ||
                                    x == "alt_name" ||
                                    x == "container" ||
                                    x == "wikipedia" ||
                                    x == "operator" ||
                                    x == "religion" ||
                                    x == "denomination" ||
                                    x == "cuisine" ||
                                    x == "operator" ||
                                    x == "material" ||
                                    x == "tourism" ||
                                    x == "amenity" ||
                                    x == "brand" ||
                                    x.StartsWith("post_box:") ||
                                    x.StartsWith("name:"));

                                _logger.LogTrace($"Filtered tags: {string.Join(", ", pointOfInterest.FilteredTagKeyValues)}");

                                var node = element as Node;
                                if (node != null && node.Longitude.HasValue && node.Latitude.HasValue)
                                {
                                    pointOfInterest.Location = new GeoLocation() { Latitude = node.Latitude.Value, Longitude = node.Longitude.Value };
                                }
                                else
                                {
                                    var way = element as CompleteWay;
                                    if (way != null)
                                    {
                                        pointOfInterest.Location = new GeoLocation() { Latitude = way.Nodes.Average(x => x.Latitude.Value), Longitude = way.Nodes.Average(x => x.Longitude.Value) };
                                    }
                                }

                                if (!pointsOfInterest.Any(x => x.Id == pointOfInterest.Id) && pointOfInterest.FilteredTagKeyValues.Any())
                                {
                                    _logger.LogTrace("Adding point of interest " + counter + " to import batch");
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

                            if (totalCounter > 20000)
                            {
                                break;
                            }
                        }
                        SavePointsOfInterest(pointsOfInterest);

                        _logger.LogInformation($"{totalCounter} points of interest imported");
                    }
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

        public void UpdateSuggestions()
        {
            using (var session = _documentService.OpenDocumentSession())
            {
                var suggestions = session
                    .Query<PointOfInterest, PointOfInterest_ByTagsAndCoordinates>()
                    .ProjectInto<PointOfInterest_ByTagsAndCoordinates.Result>()
                    //.Search(x => x.TagValueSearch, "o")
                    .ToList();
                var allSuggestions = suggestions.SelectMany(x => x.TagValueSearch).ToList();
                var distinctSuggestions = allSuggestions.Distinct().OrderBy(x => x).ToList();
            }
        }

        public void CompactPointsOfInterest()
        {
            _logger.LogInformation("Compacting document store");
            _documentService.CompactDocumentStore();
            _logger.LogInformation("Document store compacted");
        }

        public SearchResult Search(string searchTerm, double? latitude, double? longitude, double searchRadius)
        {
            string[] poiTerms = null;
            string locationTerm;
            var combiners = new string[] { " near ", " around ", " in " };
            var poiAndLocation = searchTerm.Split(combiners, StringSplitOptions.RemoveEmptyEntries);
            if (poiAndLocation.Length == 2)
            {
                poiTerms = poiAndLocation[0].Split(' ');
                locationTerm = poiAndLocation[1];
            }
            else
            {
                locationTerm = poiAndLocation[0];
            }

            var searchResult = new SearchResult();

            var location = _locationSearchService.Search(locationTerm);
            if (location != null)
            {
                searchResult.Name = location.Name;
                searchResult.Location = location.GeoLocation;
                searchResult.Radius = location.Radius;
                searchResult.ZoomLevel = location.ZoomLevel;
                searchResult.GeocodeQuality = location.GeocodeQuality;

                if (poiTerms != null)
                {
                    searchResult.PointsOfInterest = SearchPointsOfInterest(poiTerms, location.GeoLocation.Latitude, location.GeoLocation.Longitude, location.Radius);
                }
            }
            else if (latitude.HasValue && longitude.HasValue)
            {
                var pointsOfInterest = SearchPointsOfInterest(locationTerm.Split(' '), latitude.Value, longitude.Value, searchRadius);
                if (pointsOfInterest.Any())
                {
                    searchResult.PointsOfInterest = pointsOfInterest;
                    searchResult.Location = new GeoLocation() { Latitude = latitude.Value, Longitude = longitude.Value };
                    searchResult.Radius = searchRadius;
                }
            }

            return searchResult;
        }

        private List<PointOfInterest> SearchPointsOfInterest(string[] poiTerms, double latitude, double longitude, double radius)
        {
            using (var session = _documentService.OpenDocumentSession())
            {
                var query = session.Query<PointOfInterest_ByTagsAndCoordinates.Result, PointOfInterest_ByTagsAndCoordinates>();
                query = query.Spatial(x => x.Location, y => y.WithinRadius(radius, latitude, longitude));
                query = query.Search(x => x.TagValueSearch, poiTerms[0]);
                for (var i = 1; i < poiTerms.Length; i++)
                {
                    query = query.Search(x => x.TagValueSearch, poiTerms[i], options: SearchOptions.And);
                }
                var pointsOfInterest = query
                    .OrderByDistance(x => x.Location, latitude, longitude)
                    .ProjectInto<PointOfInterest>()
                    .Take(200)
                    .ToList();
                return pointsOfInterest;
            }
        }

        public List<Suggestion> GetSuggestions(string searchTerm, double? latitude, double? longitude, double radius)
        {
            using (var session = _documentService.OpenDocumentSession())
            {
                var query = session.Query<PointOfInterest_ByTagsAndCoordinates.Result, PointOfInterest_ByTagsAndCoordinates>();
                query = query.Spatial(x => x.Location, y => y.WithinRadius(radius, latitude.Value, longitude.Value));
                query = query.Search(x => x.TagValueSearch, searchTerm + "*");
                var pointsOfInterest = query
                    .ProjectInto<PointOfInterest>()
                    .Take(200)
                    .ToList();
                return pointsOfInterest.OrderBy(x => x.Name).Select(x => new Suggestion() { Value = x.Type + " " + x.Name }).ToList();
            }
        }
    }
}
