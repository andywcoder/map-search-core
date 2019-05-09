using Raven.Client.Documents.Indexes;
using System.Collections.Generic;
using System.Linq;

namespace Santolibre.Map.Search.Lib.Models
{
    public class PointOfInterest_ByTagsEnglishNameAndCoordinates : AbstractIndexCreationTask<PointOfInterest>
    {
        public class Result
        {
            public string[] TagKeyValueSearch { get; set; }
            public GeoLocation Location { get; set; }
        }

        public PointOfInterest_ByTagsEnglishNameAndCoordinates()
        {
            Map = pointsOfInterest => from pointOfInterest in pointsOfInterest
                                      select new
                                      {
                                          TagKeyValueSearch = pointOfInterest.TagKeyValueSearch["en"].Concat(new List<string>() { pointOfInterest.Name }).ToArray(),
                                          Location = CreateSpatialField(pointOfInterest.Location.Latitude, pointOfInterest.Location.Longitude)
                                      };

            Index("TagKeyValueSearch", FieldIndexing.Search);
            Analyze("TagKeyValueSearch", "Santolibre.RavenDB.Analyzers.LowerCaseNonDiacriticEnglishStopWordsAnalyzer, Santolibre.RavenDB.Analyzers");
        }
    }
}
