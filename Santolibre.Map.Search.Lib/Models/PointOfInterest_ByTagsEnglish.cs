using Raven.Client.Documents.Indexes;
using System.Linq;

namespace Santolibre.Map.Search.Lib.Models
{
    public class PointOfInterest_ByTagsEnglish : AbstractIndexCreationTask<PointOfInterest>
    {
        public class Result
        {
            public string[] TagKeyValueSearch { get; set; }
        }

        public PointOfInterest_ByTagsEnglish()
        {
            Map = pointsOfInterest => from pointOfInterest in pointsOfInterest
                                      select new
                                      {
                                          TagKeyValueSearch = pointOfInterest.TagKeyValueSearch["en"].ToArray()
                                      };

            Index("TagKeyValueSearch", FieldIndexing.Search);
        }
    }
}
