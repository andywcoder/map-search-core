using Raven.Client.Documents.Indexes;
using System.Linq;

namespace Santolibre.Map.Search.Lib.Models
{
    public class PointOfInterest_ByTagsGermanAndCoordinates : AbstractIndexCreationTask<PointOfInterest>
    {
        public class Result
        {
            public string Name { get; set; }
            public string[] TagKeyValueSearch { get; set; }
            public GeoLocation Location { get; set; }
        }

        public PointOfInterest_ByTagsGermanAndCoordinates()
        {
            Map = pointsOfInterest => from pointOfInterest in pointsOfInterest
                                      select new
                                      {
                                          Name = pointOfInterest.Name,
                                          TagKeyValueSearch = pointOfInterest.TagKeyValueSearch["de"].ToArray(),
                                          Location = CreateSpatialField(pointOfInterest.Location.Latitude, pointOfInterest.Location.Longitude)
                                      };

            Index("TagKeyValueSearch", FieldIndexing.Search);
        }
    }
}
