using Santolibre.Map.Search.Lib.Models;

namespace Santolibre.Map.Search.Lib.Services
{
    public interface ILocationSearchService
    {
        Location Search(string locationString);
    }
}
