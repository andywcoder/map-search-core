using Santolibre.Map.Search.Lib.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Santolibre.Map.Search.Lib.Repositories
{
    public interface ITranslationRepository
    {
        Task<TranslationResult[]> GetTranslationAsync(string from, string to, IEnumerable<string> terms);
    }
}
