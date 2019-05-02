using Raven.Client.Documents.Indexes;
using System.Linq;

namespace Santolibre.Map.Search.Lib.Models
{
    public class PointOfInterest_ByDateUpdated : AbstractIndexCreationTask<PointOfInterest>
    {
        public PointOfInterest_ByDateUpdated()
        {
            Map = pointsOfInterest => from pointOfInterest in pointsOfInterest
                                      select new
                                      {
                                          DateUpdated = pointOfInterest.DateUpdated
                                      };

            Indexes.Add(x => x.DateUpdated, FieldIndexing.Default);
        }
    }
}
