using Microsoft.Extensions.Logging;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Streams;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Operations.Indexes;
using Santolibre.Map.Search.Geocoding;
using Santolibre.Map.Search.Lib.Models;
using Santolibre.Map.Search.Lib.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Santolibre.Map.Search.Lib.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IDocumentRepository _documentService;
        private readonly ITranslationService _translationService;
        private readonly IPointOfInterestRepository _pointOfInterestRepository;
        private readonly ITranslationCacheRepository _translationCacheRepository;
        private readonly ILogger<IMaintenanceService> _logger;

        public MaintenanceService(IDocumentRepository documentService, ITranslationService translationService, IPointOfInterestRepository pointOfInterestRepository, ITranslationCacheRepository translationCacheRepository, ILogger<IMaintenanceService> logger)
        {
            _documentService = documentService;
            _translationService = translationService;
            _pointOfInterestRepository = pointOfInterestRepository;
            _translationCacheRepository = translationCacheRepository;
            _logger = logger;
        }

        public void ImportPointsOfInterest(string filename, Language language)
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
                                SetTagKeyValueSearch(element, pointOfInterest, language);
                                SetLocation(element, pointOfInterest);

                                if (!pointsOfInterest.Any(x => x.Id == pointOfInterest.Id) && pointOfInterest.TagKeyValueSearch["en"].Any())
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

        private void SetTagKeyValueSearch(ICompleteOsmGeo element, PointOfInterest pointOfInterest, Language language)
        {
            // Filter tags by keys and values
            var excludeTagsByValues = new string[] { "no", "limited" };
            var includeTagsByKeys = new string[] { "access", "amenity", "atm", "backrest", "bench", "building", "covered", "cuisine", "dispensing", "drive_through", "fireplace", "fuel", "healthcare", "indoor", "leisure", "lit", "nursery", "parking", "shelter_type", "shop", "sport", "surface", "toilets:wheelchair", "tourism", "vending", "wheelchair" };
            var includeTagsByKeysStartsWith = new string[] { "recycling", "playground" };

            element.Tags.RemoveAll(x =>
                string.IsNullOrEmpty(x.Key) ||
                string.IsNullOrEmpty(x.Value) ||
                Regex.IsMatch(x.Value, @"^\d+$") ||
                excludeTagsByValues.Contains(x.Value) ||
                (!includeTagsByKeys.Contains(x.Key) && !includeTagsByKeysStartsWith.Any(y => x.Key.StartsWith(y))));

            // Merge keys and values and filter list
            var excludeTagKeyValues = new string[] { "brand", "cuisine", "operator", "shelter_type", "surface", "yes" };

            var tagKeyValueSearchEnglish = element.Tags.Select(x => x.Key.ToLower().Replace(":", " ").Replace("_", " ")).Concat(element.Tags.Select(x => x.Value.ToLower().Replace(":", " ").Replace("_", " "))).Distinct().ToList();
            tagKeyValueSearchEnglish.RemoveAll(x =>
                excludeTagKeyValues.Contains(x));

            if (language.HasFlag(Language.EN))
            {
                pointOfInterest.TagKeyValueSearch["en"] = tagKeyValueSearchEnglish;
            }
            if (language.HasFlag(Language.DE))
            {
                pointOfInterest.TagKeyValueSearch["de"] = _translationService.GetTranslation(Language.EN, Language.DE, tagKeyValueSearchEnglish).Select(x => x.Destination).ToList();
            }

            _logger.LogTrace($"Filtered tags, Id={pointOfInterest.Id}, FilteredTagKeyValues={string.Join(", ", pointOfInterest.TagKeyValueSearch["en"])}");
        }

        private void SetLocation(ICompleteOsmGeo element, PointOfInterest pointOfInterest)
        {
            if (element is Node node && node.Longitude.HasValue && node.Latitude.HasValue)
            {
                pointOfInterest.GeoCoordinates = new GeoCoordinates(node.Latitude.Value, node.Longitude.Value);
            }
            else
            {
                if (element is CompleteWay way)
                {
                    pointOfInterest.GeoCoordinates = new GeoCoordinates(way.Nodes.Average(x => x.Latitude.Value), way.Nodes.Average(x => x.Longitude.Value));
                }
            }
        }

        private void SavePointsOfInterest(List<PointOfInterest> pointsOfInterest)
        {
            try
            {
                _logger.LogTrace("Saving points of interest batch");
                _pointOfInterestRepository.SavePointsOfInterest(pointsOfInterest);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "There was an error while saving the data (" + e.Message + ")");
            }

            _logger.LogTrace("Clearing points of interest batch");
            pointsOfInterest.Clear();

            var totalMemoryBefore = GC.GetTotalMemory(false);
            GC.Collect();
            _logger.LogTrace($"Garbage collection, TotalMemoryBefore={totalMemoryBefore}, TotalMemoryAfter={GC.GetTotalMemory(false)}");
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

        public void AnalyzeIndexTerms()
        {
            _logger.LogInformation("Analyzing index terms");
            var terms = _documentService.RunOperation(new GetTermsOperation("PointOfInterest/ByTagsEnglish", "TagKeyValueSearch", null));
            var termDocumentCounts = new Dictionary<string, int>();
            foreach (var term in terms)
            {
                var documentCount = _pointOfInterestRepository.CountPointsOfInterest(term);
                termDocumentCounts.Add(term, documentCount);
            }

            foreach (var termDocumentCount in termDocumentCounts.OrderByDescending(x => x.Value))
            {
                if (termDocumentCount.Value <= 1)
                {
                    _logger.LogError($"Term {termDocumentCount.Key} has {termDocumentCount.Value} points of interest");
                }
                else if (termDocumentCount.Value < 10)
                {
                    _logger.LogWarning($"Term {termDocumentCount.Key} has {termDocumentCount.Value} points of interest");
                }
                else
                {
                    _logger.LogInformation($"Term {termDocumentCount.Key} has {termDocumentCount.Value} points of interest");
                }
            }

            _logger.LogInformation($"{termDocumentCounts.Count(x => x.Value == 1)} terms that have 1 points of interest, {termDocumentCounts.Where(x => x.Value == 1).Sum(x => x.Value)} points of interest in total");
            _logger.LogInformation($"{termDocumentCounts.Count(x => x.Value > 1 && x.Value < 10)} terms that have 2-9 points of interest, {termDocumentCounts.Where(x => x.Value > 1 && x.Value < 10).Sum(x => x.Value)} points of interest in total");
            _logger.LogInformation($"{termDocumentCounts.Count(x => x.Value >= 10)} terms that have 10 or more points of interest, {termDocumentCounts.Where(x => x.Value >= 10).Sum(x => x.Value)} points of interest in total");
        }

        public void UpdateTranslationCache(Language from, Language to)
        {
            var translationCache = new TranslationCache()
            {
                Id = "TagAndValues"
            };

            var terms = _documentService.RunOperation(new GetTermsOperation("PointOfInterest/ByTagsEnglish", "TagKeyValueSearch", null));
            for (var i = 0; i < terms.Length; i += 10)
            {
                _translationService.GetTranslation(from, to, terms.Skip(i).Take(10).ToList()).ForEach(x =>
                {
                    translationCache.Terms.Add(x.Source, new Translations() { GermanTerm = x.Destination });
                });
            }

            _translationCacheRepository.SaveTranslationCache(translationCache);
        }

        public void PopulateTranslationCache(Language language)
        {
            var translationCache = _translationCacheRepository.GetTranslationCache("TagAndValues");
            if (language.HasFlag(Language.DE))
            {
                _translationService.PopulateCache(translationCache.Terms.Select(x => (Language.EN, Language.DE, x.Key, x.Value.GermanTerm)).ToList());
            }
        }
    }
}
