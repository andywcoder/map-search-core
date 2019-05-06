using Raven.Client.Documents.Indexes;
using System.Linq;

namespace Santolibre.Map.Search.Lib.Models
{
    public class PointOfInterest_ByTagsAndCoordinates : AbstractIndexCreationTask<PointOfInterest>
    {
        public class Result
        {
            public string[] TagValueSearch { get; set; }
            public GeoLocation Location { get; set; }
        }

        public PointOfInterest_ByTagsAndCoordinates()
        {
            Map = pointsOfInterest => from pointOfInterest in pointsOfInterest
                                      select new
                                      {
                                          TagValueSearch = pointOfInterest.FilteredTagKeyValues.ToArray(),
                                          Location = CreateSpatialField(pointOfInterest.Location.Latitude, pointOfInterest.Location.Longitude)
                                      };

            Index("TagValueSearch", FieldIndexing.Search);
        }
    }
}
