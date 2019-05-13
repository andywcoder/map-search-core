using Raven.Client.Documents;
using Santolibre.Map.Search.Lib.Models;
using Santolibre.RavenDB.Analyzers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Santolibre.Map.Search.Lib.Repositories
{
    public class PointOfInterestRepository : IPointOfInterestRepository
    {
        private readonly IDocumentRepository _documentService;

        public PointOfInterestRepository(IDocumentRepository documentService)
        {
            _documentService = documentService;
        }

        public List<PointOfInterest> GetPointsOfInterest(string searchTerm, bool addWildcards, double latitude, double longitude, double? radius, int take)
        {
            var searchTerms = LowerCaseNonDiacriticEnglishStopWordsAnalyzer.Tokenize(searchTerm).Distinct();

            using (var session = _documentService.OpenDocumentSession())
            {
                var query = session.Query<PointOfInterest_ByTagsEnglishNameAndCoordinates.Result, PointOfInterest_ByTagsEnglishNameAndCoordinates>();

                if (radius.HasValue)
                {
                    query = query.Spatial(x => x.Location, y => y.WithinRadius(radius.Value, latitude, longitude));
                }

                query = searchTerms.Aggregate(query, (q, term) => q.Search(y => y.TagKeyValueSearch, addWildcards ? term + "*" : term, options: SearchOptions.And));

                var pointsOfInterest = query
                    .OrderByDistance(x => x.Location, latitude, longitude)
                    .ProjectInto<PointOfInterest>()
                    .Take(take)
                    .ToList();
                return pointsOfInterest;
            }
        }

        public void SavePointsOfInterest(List<PointOfInterest> pointsOfInterest)
        {
            using (var session = _documentService.OpenDocumentSession())
            {
                foreach (var pointOfInterest in pointsOfInterest)
                {
                    session.Store(pointOfInterest);
                }
                session.SaveChanges();
            }
        }

        public int CountPointsOfInterest(string searchTerm)
        {
            using (var session = _documentService.OpenDocumentSession())
            {
                var query = session.Query<PointOfInterest_ByTagsEnglishNameAndCoordinates.Result, PointOfInterest_ByTagsEnglishNameAndCoordinates>();
                var pointsOfInterestCount = query.Where(x => x.TagKeyValueSearch.Contains(searchTerm)).Count();
                return pointsOfInterestCount;
            }
        }
    }
}
