using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using System.Linq;

namespace Santolibre.Map.Search.Lib.Models
{
    public class PointOfInterest_ByTagsAndCoordinates : AbstractIndexCreationTask<PointOfInterest>
    {
        public PointOfInterest_ByTagsAndCoordinates()
        {
            Map = pointsOfInterest => from pointOfInterest in pointsOfInterest
                                      select new
                                      {
                                          TagValueSearch = pointOfInterest.FilteredTags.Select(x => x.Key + " " + x.Value),
                                          _SpatialLocation = SpatialGenerate("Location", pointOfInterest.Location.Latitude, pointOfInterest.Location.Longitude)
                                      };

            Indexes.Add(x => x.TagValueSearch, FieldIndexing.Analyzed);
        }
    }
}
