using Santolibre.Map.Search.Lib.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Santolibre.Map.Search.Lib.Repositories
{
    public interface ITranslationRepository
    {
        Task<TranslationResult[]> GetTranslationAsync(Language from, Language to, IEnumerable<string> terms);
    }
}
