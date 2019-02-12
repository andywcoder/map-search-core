using Microsoft.Extensions.Logging;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Streams;
using Raven.Client;
using Santolibre.Map.Search.Lib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                            if (tagKeys.Any(x => element.Tags.ContainsKey(x)))
                            {
                                var pointOfInterest = new PointOfInterest();
                                pointOfInterest.DateUpdated = DateTime.UtcNow;
                                pointOfInterest.Id = element.Id;
                                pointOfInterest.Tags = element.Tags.ToDictionary(x => x.Key, x => x.Value);
                                element.Tags.RemoveAll(x => string.IsNullOrEmpty(x.Key));
                                element.Tags.RemoveAll(x => x.Value == "no");
                                pointOfInterest.FilteredTags = element.Tags.ToDictionary(x => x.Key, x => x.Value);
                                foreach (var tagKey in tagKeys)
                                {
                                    if (element.Tags.ContainsKey(tagKey))
                                    {
                                        pointOfInterest.Category = tagKey;
                                        pointOfInterest.Type = element.Tags[tagKey];
                                        break;
                                    }
                                }
                                if (element.Tags.ContainsKey("name"))
                                {
                                    pointOfInterest.Name = element.Tags["name"];
                                }

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

                                if (!pointsOfInterest.Any(x => x.Id == pointOfInterest.Id))
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
            using (var session = _documentService.OpenDocumentSession())
            {
                var dateTime = DateTime.UtcNow - new TimeSpan(days, 0, 0, 0);
                var operation = session.Advanced.DeleteByIndex<PointOfInterest, PointOfInterest_ByDateUpdated>(x => x.DateUpdated < dateTime);
                operation.WaitForCompletion();
            }
            _logger.LogInformation("Points of interest removed");
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
                var query = session.Query<PointOfInterest, PointOfInterest_ByTagsAndCoordinates>();
                query = query.Search(x => x.TagValueSearch, poiTerms[0]);
                for (var i = 1; i < poiTerms.Length; i++)
                {
                    query = query.Search(x => x.TagValueSearch, poiTerms[i], options: SearchOptions.And);
                }
                var pointsOfInterest = query.ProjectFromIndexFieldsInto<PointOfInterest>()
                    .Customize(x => x.SortByDistance())
                    .Spatial(x => x.Location, y => y.WithinRadius(radius, latitude, longitude)).Take(200).ToList();
                return pointsOfInterest;
            }
        }
    }
}
